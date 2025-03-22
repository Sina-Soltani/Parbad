// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Gateway.Pasargad.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Gateway.Pasargad.Api;
using Parbad.Gateway.Pasargad.Api.Models;
using Parbad.Gateway.Pasargad.Internal.Models;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.Gateway.Pasargad;

[Gateway(Name)]
public class PasargadGateway : GatewayBase<PasargadGatewayAccount>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPasargadApi _pasargadApi;
    private readonly PasargadGatewayOptions _gatewayOptions;
    private readonly MessagesOptions _messageOptions;

    private const string InvoiceDateKey = "invoiceDate";
    public const string Name = "Pasargad";

    public PasargadGateway(IHttpContextAccessor httpContextAccessor,
                           IPasargadApi pasargadApi,
                           IGatewayAccountProvider<PasargadGatewayAccount> accountProvider,
                           IOptions<PasargadGatewayOptions> gatewayOptions,
                           IOptions<MessagesOptions> messageOptions)
        : base(accountProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _pasargadApi = pasargadApi;
        _gatewayOptions = gatewayOptions.Value;
        _messageOptions = messageOptions.Value;
    }

    /// <inheritdoc />
    public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        if (invoice == null) throw new ArgumentNullException(nameof(invoice));

        var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

        var invoiceDate = PasargadHelper.GetTimeStamp();
        var timeStamp = invoiceDate;

        var additionalData = invoice.GetPasargadRequestAdditionalData();

        var tokenResponse = await _pasargadApi.GetToken(new PasargadGetTokenRequestModel
                {
                    Username = account.Username,
                    Password = account.Password
                },
                cancellationToken)
            .ConfigureAwaitFalse();

        var response = await _pasargadApi.PurchasePayment(new PasargadPurchaseRequestModel
                       {
                           ServiceCode = 8,
                           ServiceType = "PURCHASE",
                           TerminalNumber = account.TerminalCode,
                           Invoice = invoice.TrackingNumber.ToString(),
                           InvoiceDate = invoiceDate,
                           Amount = invoice.Amount,
                           CallbackApi = invoice.CallbackUrl,
                           
                           PayerMail = additionalData?.Email,
                           Description = additionalData?.Description,
                           NationalCode = additionalData?.NationalCode,
                           Pans = additionalData?.Pans,
                           PayerName = additionalData?.PayerName,
                        },
                       tokenResponse.Token,
                       cancellationToken)
                    .ConfigureAwaitFalse();

        if (response.ResultCode == 0)
        {
            return PaymentRequestResult.Failed(response.ResultMsg, account.Name);
        }

        var form = new Dictionary<string, string>
        {
            { "Token", response.Data.UrlId }
        };

        var result = PaymentRequestResult.SucceedWithPost(account.Name,
                                                          _httpContextAccessor.HttpContext,
                                                          _gatewayOptions.ApiBaseUrl,
                                                          form);

        //result.DatabaseAdditionalData.Add(InvoiceDateKey, invoiceDate);

        return result;
    }

    /// <inheritdoc />
    public override async Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var callbackResult = await PasargadHelper.BindCallbackResultModel(_httpContextAccessor.HttpContext.Request,
                                                                          cancellationToken)
                                                 .ConfigureAwaitFalse();

        if (callbackResult.Status != PasargadCallbackStatusResult.Success)
        {
            return PaymentFetchResult.Failed(_messageOptions.PaymentFailed);
        }

        var result = PaymentFetchResult.ReadyForVerifying();
        result.TransactionCode = callbackResult.ReferenceNumber;

        return result;
    }

    /// <inheritdoc />
    public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var callbackResult = await PasargadHelper.BindCallbackResultModel(_httpContextAccessor.HttpContext.Request,
                                                                          cancellationToken)
                                                 .ConfigureAwaitFalse();

        if (callbackResult.Status != PasargadCallbackStatusResult.Success)
        {
            return PaymentVerifyResult.Failed(_messageOptions.PaymentFailed);
        }

        var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

        var tokenResponse = await _pasargadApi.GetToken(new PasargadGetTokenRequestModel
                {
                    Username = account.Username,
                    Password = account.Password
                },
                cancellationToken)
            .ConfigureAwaitFalse();

        var response = await _pasargadApi.VerifyPayment(new PasargadVerifyPaymentRequestModel
                                                        {
                                                            Invoice = callbackResult.InvoiceId,
                                                            UrlId = context.Payment.Token,
                                                        },
                                                        tokenResponse.Token,
                                                        cancellationToken)
                                         .ConfigureAwaitFalse();

        if (response.ResultCode != 0)
        {
            return PaymentVerifyResult.Failed(response.Data ?? _messageOptions.PaymentFailed);
        }

        return PaymentVerifyResult.Succeed(callbackResult.ReferenceNumber,
                                           _messageOptions.PaymentSucceed);
    }

    /// <inheritdoc />
    public override async Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

        var requestTransaction = context.Transactions.SingleOrDefault(transaction => transaction.Type == TransactionType.Request);

        if (requestTransaction == null)
        {
            return PaymentRefundResult.Failed($"Transaction for Invoice {context.Payment.TrackingNumber} not found");
        }

        //var requestTransactionAdditionalData = requestTransaction.ToDictionary();
        //if (!requestTransactionAdditionalData.TryGetValue(InvoiceDateKey, out var invoiceDate))
        //{
        //    return PaymentRefundResult.Failed($"InvoiceDate for Invoice {context.Payment.TrackingNumber} not found");
        //}

        var tokenResponse = await _pasargadApi.GetToken(new PasargadGetTokenRequestModel
                {
                    Username = account.Username,
                    Password = account.Password
                },
                cancellationToken)
            .ConfigureAwaitFalse();

        var response = await _pasargadApi.RefundPayment(new PasargadRefundPaymentRequestModel
                                                        {
                                                            Invoice = context.Payment.TrackingNumber.ToString(),
                                                            UrlId = context.Payment.Token,
                                                        },
                                                        tokenResponse.Token,
                                                        cancellationToken)
                                         .ConfigureAwaitFalse();

        if (!response.IsSuccess)
        {
            return PaymentRefundResult.Failed(response.Message ?? "Refund failed.");
        }

        return PaymentRefundResult.Succeed();
    }
}
