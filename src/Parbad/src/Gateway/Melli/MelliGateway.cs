// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Gateway.Melli.Api;
using Parbad.Gateway.Melli.Api.Models;
using Parbad.Gateway.Melli.Internal;
using Parbad.Gateway.Melli.Internal.ResultTranslator;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Properties;

namespace Parbad.Gateway.Melli;

/// <summary>
/// Melli Gateway.
/// </summary>
[Gateway(Name)]
public class MelliGateway : GatewayBase<MelliGatewayAccount>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMelliApi _api;
    private readonly IMelliGatewayCrypto _crypto;
    private readonly MelliGatewayOptions _gatewayOptions;
    private readonly MessagesOptions _messageOptions;

    public const string Name = "Melli";

    /// <summary>
    /// Initializes an instance of <see cref="MelliGateway"/>.
    /// </summary>
    public MelliGateway(IGatewayAccountProvider<MelliGatewayAccount> accountProvider,
                        IHttpContextAccessor httpContextAccessor,
                        IMelliApi api,
                        IMelliGatewayCrypto crypto,
                        IOptions<MelliGatewayOptions> gatewayOptions,
                        IOptions<MessagesOptions> messageOptions)
        : base(accountProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _api = api;
        _crypto = crypto;
        _messageOptions = messageOptions.Value;
        _gatewayOptions = gatewayOptions.Value;
    }

    /// <inheritdoc />
    public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        if (invoice == null) throw new ArgumentNullException(nameof(invoice));

        var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

        var signedData = CreateRequestSignData(account, invoice);

        var requestResult = await _api.Request(new MelliApiRequestModel
                                               {
                                                   TerminalId = account.TerminalId,
                                                   MerchantId = account.MerchantId,
                                                   SignData = signedData,
                                                   ReturnUrl = invoice.CallbackUrl,
                                                   LocalDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                                   OrderId = invoice.TrackingNumber,
                                                   Amount = invoice.Amount
                                               },
                                               cancellationToken);

        if (requestResult == null)
        {
            return PaymentRequestResult.Failed(_messageOptions.UnexpectedErrorText);
        }

        var isSucceed = requestResult.ResCode == Constants.SuccessCode;

        if (!isSucceed)
        {
            string message;

            if (requestResult.ResCode == Constants.DuplicateTrackingNumberCode)
            {
                message = _messageOptions.DuplicateTrackingNumber;
            }
            else
            {
                message = !requestResult.Description.IsNullOrEmpty()
                    ? requestResult.Description
                    : MelliRequestResultTranslator.Translate(requestResult.ResCode, _messageOptions);
            }

            return PaymentRequestResult.Failed(message, account.Name, requestResult.ResCode?.ToString());
        }

        var paymentPageUrl = $"{_gatewayOptions.PaymentPageUrl}?token={requestResult.Token}";

        return PaymentRequestResult.SucceedWithRedirect(account.Name, _httpContextAccessor.HttpContext, paymentPageUrl);
    }

    /// <inheritdoc />
    public override async Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var callbackResult = await MelliHelper.BindCallbackResult(_httpContextAccessor.HttpContext.Request,
                                                                  context,
                                                                  _messageOptions,
                                                                  cancellationToken);

        if (!callbackResult.IsSucceeded)
        {
            return PaymentFetchResult.Failed(callbackResult.Message, callbackResult.ResCode.ToString());
        }

        return PaymentFetchResult.ReadyForVerifying();
    }

    /// <inheritdoc />
    public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

        var callbackResult = await MelliHelper.BindCallbackResult(_httpContextAccessor.HttpContext.Request,
                                                                  context,
                                                                  _messageOptions,
                                                                  cancellationToken);

        if (!callbackResult.IsSucceeded)
        {
            return PaymentVerifyResult.Failed(callbackResult.Message, callbackResult.ResCode.ToString());
        }

        var signedData = _crypto.Encrypt(account.TerminalKey, callbackResult.Token);

        var verifyResult = await _api.Verify(new MelliApiVerifyModel
                                             {
                                                 Token = callbackResult.Token,
                                                 SignData = signedData
                                             },
                                             cancellationToken);

        if (verifyResult == null)
        {
            return PaymentVerifyResult.Failed(_messageOptions.UnexpectedErrorText);
        }

        string message;

        if (!verifyResult.Description.IsNullOrEmpty())
        {
            message = verifyResult.Description;
        }
        else
        {
            message = MelliVerifyResultTranslator.Translate(verifyResult.ResCode, _messageOptions);
        }

        var status = verifyResult.ResCode == Constants.SuccessCode
            ? PaymentVerifyResultStatus.Succeed
            : PaymentVerifyResultStatus.Failed;

        return new PaymentVerifyResult
               {
                   Status = status,
                   TransactionCode = verifyResult.RetrivalRefNo,
                   Message = message,
                   GatewayResponseCode = verifyResult.ResCode.ToString()
               };
    }

    /// <inheritdoc />
    public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
    {
        return PaymentRefundResult.Failed(Resources.RefundNotSupports).ToInterfaceAsync();
    }

    private string CreateRequestSignData(MelliGatewayAccount account, Invoice invoice)
    {
        var data = $"{account.TerminalId};{invoice.TrackingNumber};{(long)invoice.Amount}";

        return _crypto.Encrypt(account.TerminalKey, data);
    }
}
