// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Gateway.Saman.Api;
using Parbad.Gateway.Saman.Api.Models;
using Parbad.Gateway.Saman.Internal;
using Parbad.Gateway.Saman.Internal.ResultTranslators;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.Gateway.Saman;

[Gateway(Name)]
public class SamanGateway : GatewayBase<SamanGatewayAccount>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISamanApi _api;
    private readonly SamanGatewayOptions _gatewayOptions;
    private readonly MessagesOptions _messageOptions;

    public const string Name = "Saman";

    public SamanGateway(IGatewayAccountProvider<SamanGatewayAccount> accountProvider,
                        IHttpContextAccessor httpContextAccessor,
                        ISamanApi api,
                        IOptions<SamanGatewayOptions> gatewayOptions,
                        IOptions<MessagesOptions> messageOptions) : base(accountProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _api = api;
        _gatewayOptions = gatewayOptions.Value;
        _messageOptions = messageOptions.Value;
    }

    /// <inheritdoc />
    public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        if (invoice == null) throw new ArgumentNullException(nameof(invoice));

        var account = await GetAccountAsync(invoice);

        var tokenResponse = await _api.RequestToken(new SamanTokenRequest
                                                    {
                                                        Action = "token",
                                                        TerminalId = account.TerminalId,
                                                        Amount = invoice.Amount,
                                                        ResNum = invoice.TrackingNumber.ToString(),
                                                        RedirectUrl = invoice.CallbackUrl,
                                                        CellNumber = invoice.GetSamanCellNumber()
                                                    },
                                                    cancellationToken);

        if (tokenResponse.Status != Constants.TokenSuccessCode)
        {
            var resultMessage = GetResultMessage(tokenResponse.ErrorCode, tokenResponse.ErrorDesc);

            return PaymentRequestResult.Failed(resultMessage, account.Name, tokenResponse.ErrorCode);
        }

        var form = new Dictionary<string, string>
                   {
                       { "Token", tokenResponse.Token },
                       { "GetMethod", "false" }
                   };

        return PaymentRequestResult.SucceedWithPost(account.Name,
                                                    _httpContextAccessor.HttpContext,
                                                    _gatewayOptions.PaymentPageUrl,
                                                    form);
    }

    /// <inheritdoc />
    public override async Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var callbackResponse = await SamanHelper.BindCallbackResponse(_httpContextAccessor.HttpContext.Request, cancellationToken);

        var account = await GetAccountAsync(context.Payment);

        var isCallbackResponseValid = SamanHelper.ValidateCallbackResponse(callbackResponse, context, account, out var failures);

        if (!isCallbackResponseValid)
        {
            return PaymentFetchResult.Failed(failures, callbackResponse.Status);
        }

        var hasReceivedSuccessFromGateway = callbackResponse.Status == Constants.CallbackSuccessCode;

        var resultStatus = hasReceivedSuccessFromGateway ? PaymentFetchResultStatus.ReadyForVerifying : PaymentFetchResultStatus.Failed;

        var resultMessage = hasReceivedSuccessFromGateway ? null : GetResultMessage(callbackResponse.Status);

        var result = new PaymentFetchResult
                     {
                         Status = resultStatus,
                         GatewayResponseCode = callbackResponse.Status,
                         Message = resultMessage
                     };

        var additionalData = SamanHelper.MapCallbackResponseToAdditionalData(callbackResponse);

        result.AddSamanAdditionalData(additionalData);

        return result;
    }

    /// <inheritdoc />
    public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var callbackResponse = await SamanHelper.BindCallbackResponse(_httpContextAccessor.HttpContext.Request, cancellationToken);

        var account = await GetAccountAsync(context.Payment);

        var isCallbackResponseValid = SamanHelper.ValidateCallbackResponse(callbackResponse, context, account, out var failures);

        if (!isCallbackResponseValid)
        {
            return PaymentVerifyResult.Failed(failures, callbackResponse.Status);
        }

        var hasReceivedSuccessFromGateway = callbackResponse.Status == Constants.CallbackSuccessCode;

        string resultMessage;

        if (!hasReceivedSuccessFromGateway)
        {
            resultMessage = GetResultMessage(callbackResponse.Status);

            return PaymentVerifyResult.Failed(resultMessage, callbackResponse.Status);
        }

        var verificationResponse = await _api.Verify(new SamanVerificationRequest
                                                     {
                                                         RefNum = callbackResponse.RefNum,
                                                         TerminalNumber = account.TerminalId
                                                     },
                                                     cancellationToken);

        var isVerificationSucceeded = verificationResponse.TransactionDetail.TerminalNumber.ToString() == account.TerminalId &&
                                      verificationResponse.TransactionDetail.AffectiveAmount == (long)context.Payment.Amount &&
                                      verificationResponse.TransactionDetail.RefNum == callbackResponse.RefNum &&
                                      verificationResponse.ResultCode == Constants.VerificationSuccessCode;

        resultMessage = isVerificationSucceeded
            ? _messageOptions.PaymentSucceed
            : GetResultMessage(verificationResponse.ResultCode.ToString());

        var verifyResultStatus = isVerificationSucceeded ? PaymentVerifyResultStatus.Succeed : PaymentVerifyResultStatus.Failed;

        var result = new PaymentVerifyResult
                     {
                         Status = verifyResultStatus,
                         TransactionCode = verificationResponse.TransactionDetail.Rrn,
                         GatewayResponseCode = verificationResponse.ResultCode.ToString(),
                         Message = resultMessage
                     };

        result.DatabaseAdditionalData.Add(Constants.RefNumKey, callbackResponse.RefNum);

        var additionalData = SamanHelper.MapCallbackResponseToAdditionalData(callbackResponse);

        result.AddSamanAdditionalData(additionalData);

        return result;
    }

    /// <inheritdoc />
    public override async Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

        if (!TryGetRefNumFromTransaction(context, out var refNum))
        {
            throw new
                InvalidOperationException($"No Transaction of type Verification or additional data found for reversing the invoice {context.Payment.TrackingNumber}.");
        }

        var reverseResponse = await _api.Reverse(new SamanReverseRequest
                                                 {
                                                     TerminalNumber = account.TerminalId,
                                                     RefNum = refNum
                                                 },
                                                 cancellationToken);

        var resultStatus = reverseResponse.Success ? PaymentRefundResultStatus.Succeed : PaymentRefundResultStatus.Failed;

        return new PaymentRefundResult
               {
                   Status = resultStatus,
                   GatewayResponseCode = reverseResponse.ResultCode.ToString()
               };
    }

    private static bool TryGetRefNumFromTransaction(InvoiceContext context, out string refNum)
    {
        var verificationTransaction = context.Transactions.SingleOrDefault(transaction => transaction.Type == TransactionType.Verify);

        if (string.IsNullOrEmpty(verificationTransaction?.AdditionalData))
        {
            refNum = null;

            return false;
        }

        return verificationTransaction.ToDictionary().TryGetValue(Constants.RefNumKey, out refNum);
    }

    private string GetResultMessage(string errorCode, string message = null)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        return SamanResultTranslator.Translate(errorCode, _messageOptions);
    }
}
