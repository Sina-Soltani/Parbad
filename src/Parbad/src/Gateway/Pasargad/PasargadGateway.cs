// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Gateway.Pasargad.Api;
using Parbad.Gateway.Pasargad.Api.Models;
using Parbad.Gateway.Pasargad.Internal;
using Parbad.Gateway.Pasargad.Internal.Models;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.Gateway.Pasargad;

[Gateway(Name)]
public class PasargadGateway : GatewayBase<PasargadGatewayAccount>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPasargadApi _pasargadApi;
    private readonly MessagesOptions _messageOptions;

    public const string Name = "Pasargad";
    private const int SuccessCode = 0;
    private const string UrlIdName = "UrlId";

    public PasargadGateway(IHttpContextAccessor httpContextAccessor,
                           IPasargadApi pasargadApi,
                           IGatewayAccountProvider<PasargadGatewayAccount> accountProvider,
                           IOptions<MessagesOptions> messageOptions)
        : base(accountProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _pasargadApi = pasargadApi;
        _messageOptions = messageOptions.Value;
    }

    /// <inheritdoc />
    public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        if (invoice == null) throw new ArgumentNullException(nameof(invoice));

        var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

        var additionalData = invoice.GetPasargadRequestAdditionalData();

        var tokenResponse = await GetToken(account, cancellationToken);

        if (tokenResponse.ResultCode != SuccessCode)
        {
            return PaymentRequestResult.Failed(tokenResponse.ResultMsg, account.Name, tokenResponse.ResultCode.ToString());
        }

        var response = await _pasargadApi.PurchasePayment(new PasargadPurchaseRequestModel
                                                          {
                                                              ServiceCode = 8,
                                                              ServiceType = "PURCHASE",
                                                              TerminalNumber = account.TerminalNumber,
                                                              Invoice = invoice.TrackingNumber.ToString(),
                                                              InvoiceDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                                                              Amount = invoice.Amount,
                                                              CallbackApi = invoice.CallbackUrl,
                                                              Description = additionalData?.Description,
                                                              NationalCode = additionalData?.NationalCode,
                                                              Pans = additionalData?.Pans,
                                                              PayerName = additionalData?.PayerName,
                                                              PayerMail = additionalData?.PayerMail,
                                                              MobileNumber = additionalData?.MobileNumber
                                                          },
                                                          tokenResponse.Token,
                                                          cancellationToken)
                                         .ConfigureAwaitFalse();

        if (response.ResultCode != SuccessCode)
        {
            return PaymentRequestResult.Failed(response.ResultMsg, account.Name, response.ResultCode.ToString());
        }

        var result = PaymentRequestResult.SucceedWithRedirect(account.Name,
                                                              _httpContextAccessor.HttpContext,
                                                              response.Data.Url);

        result.DatabaseAdditionalData.Add(UrlIdName, response.Data.UrlId);

        return result;
    }

    /// <inheritdoc />
    public override async Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var callbackResult = await PasargadHelper.BindCallbackResultModel(_httpContextAccessor.HttpContext.Request,
                                                                          cancellationToken)
                                                 .ConfigureAwaitFalse();

        if (callbackResult.Status != PasargadCallbackResultStatus.Success)
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

        if (callbackResult.Status != PasargadCallbackResultStatus.Success)
        {
            return PaymentVerifyResult.Failed(_messageOptions.PaymentFailed);
        }

        if (callbackResult.InvoiceId != context.Payment.TrackingNumber.ToString())
        {
            return PaymentVerifyResult.Failed(_messageOptions.InvalidDataReceivedFromGateway);
        }

        var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

        var tokenResponse = await GetToken(account, cancellationToken);

        if (tokenResponse.ResultCode != SuccessCode)
        {
            var message = tokenResponse.ResultMsg ?? _messageOptions.PaymentFailed;

            return PaymentVerifyResult.Failed(message, tokenResponse.ResultCode.ToString());
        }

        var urlId = GetUrlIdFromRequestTransaction(context);

        var response = await _pasargadApi.VerifyPayment(new PasargadVerifyPaymentRequestModel
                                                        {
                                                            Invoice = context.Payment.TrackingNumber.ToString(),
                                                            UrlId = urlId
                                                        },
                                                        tokenResponse.Token,
                                                        cancellationToken)
                                         .ConfigureAwaitFalse();

        if (response.ResultCode != SuccessCode)
        {
            var message = response.ResultMsg ?? _messageOptions.PaymentFailed;

            return PaymentVerifyResult.Failed(message, response.ResultCode.ToString());
        }

        return PaymentVerifyResult.Succeed(callbackResult.ReferenceNumber,
                                           _messageOptions.PaymentSucceed);
    }

    /// <inheritdoc />
    public override async Task<IPaymentRefundResult> RefundAsync(InvoiceContext context,
                                                                 Money amount,
                                                                 CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

        var tokenResponse = await GetToken(account, cancellationToken);

        if (tokenResponse.ResultCode != SuccessCode)
        {
            var message = tokenResponse.ResultMsg ?? _messageOptions.PaymentFailed;

            return PaymentRefundResult.Failed(message, tokenResponse.ResultCode.ToString());
        }

        var urlId = GetUrlIdFromRequestTransaction(context);

        var response = await _pasargadApi.RefundPayment(new PasargadRefundPaymentRequestModel
                                                        {
                                                            Invoice = context.Payment.TrackingNumber.ToString(),
                                                            UrlId = urlId
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

    private Task<PasargadGetTokenResponseModel> GetToken(PasargadGatewayAccount account, CancellationToken cancellationToken)
    {
        return _pasargadApi.GetToken(new PasargadGetTokenRequestModel
                                     {
                                         Username = account.Username,
                                         Password = account.Password
                                     },
                                     cancellationToken);
    }

    private static string GetUrlIdFromRequestTransaction(InvoiceContext context)
    {
        var transactionRecord = context.Transactions.SingleOrDefault(transaction => transaction.Type == TransactionType.Request);

        if (transactionRecord == null)
        {
            return null;
        }

        var additionalData = transactionRecord.ToDictionary();

        if (additionalData.TryGetValue(UrlIdName, out var urlId))
        {
            return urlId;
        }

        return null;
    }
}
