// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Gateway.Saman.Internal.Models;
using Parbad.Gateway.Saman.Internal.ResultTranslators;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.Gateway.Saman.Internal;

internal static class SamanHelper
{
    private const string RefNumKey = nameof(RefNumKey);
    public const string AdditionalVerificationDataKey = "SamanAdditionalVerificationData";
    public const string CellNumberPropertyKey = "SamanCellNumber";

    public static SamanTokenRequest CreateTokenRequestModel(Invoice invoice, SamanGatewayAccount account)
    {
        var model = new SamanTokenRequest
                    {
                        Action = "token",
                        TerminalId = account.TerminalId,
                        Amount = invoice.Amount,
                        ResNum = invoice.TrackingNumber.ToString(),
                        RedirectUrl = invoice.CallbackUrl,
                        CellNumber = invoice.GetSamanCellNumber()
                    };

        if (!string.IsNullOrWhiteSpace(invoice.GetSamanCellNumber()))
        {
            model.CellNumber = invoice.GetSamanCellNumber();
        }

        return model;
    }

    public static IPaymentRequestResult CreatePaymentRequestResult(SamanTokenResponse tokenResponse,
                                                                   SamanGatewayAccount account,
                                                                   HttpContext httpContext,
                                                                   SamanGatewayOptions gatewayOptions,
                                                                   MessagesOptions messagesOptions)
    {
        if (tokenResponse.Status == 1)
        {
            var form = new Dictionary<string, string>
                       {
                           { "Token", tokenResponse.Token },
                           { "GetMethod", "false" }
                       };

            return PaymentRequestResult.SucceedWithPost(account.Name, httpContext, gatewayOptions.PaymentPageUrl, form);
        }

        var message = string.IsNullOrWhiteSpace(tokenResponse.ErrorDesc)
            ? SamanResultTranslator.Translate(tokenResponse.ErrorCode, messagesOptions)
            : tokenResponse.ErrorDesc;

        return PaymentRequestResult.Failed(message, account.Name, tokenResponse.ErrorCode);
    }

    public static async Task<SamanCallbackResponse> BindCallbackResponse(HttpRequest httpRequest, CancellationToken cancellationToken)
    {
        var mid = await httpRequest.TryGetParamAsync("MID", cancellationToken).ConfigureAwaitFalse();
        var terminalId = await httpRequest.TryGetParamAsync("TerminalId", cancellationToken).ConfigureAwaitFalse();
        var state = await httpRequest.TryGetParamAsync("State", cancellationToken).ConfigureAwaitFalse();
        var status = await httpRequest.TryGetParamAsync("Status", cancellationToken).ConfigureAwaitFalse();
        var refNum = await httpRequest.TryGetParamAsync("RefNum", cancellationToken).ConfigureAwaitFalse();
        var resNum = await httpRequest.TryGetParamAsync("ResNum", cancellationToken).ConfigureAwaitFalse();
        var securePan = await httpRequest.TryGetParamAsync("SecurePan", cancellationToken).ConfigureAwaitFalse();
        var hashedCardNumber = await httpRequest.TryGetParamAsync("HashedCardNumber", cancellationToken).ConfigureAwaitFalse();
        var traceNo = await httpRequest.TryGetParamAsync("TraceNo", cancellationToken).ConfigureAwaitFalse();
        var rrn = await httpRequest.TryGetParamAsync("RRN", cancellationToken).ConfigureAwaitFalse();
        var amount = await httpRequest.TryGetParamAsync("Amount", cancellationToken).ConfigureAwaitFalse();
        var wage = await httpRequest.TryGetParamAsync("Wage", cancellationToken).ConfigureAwaitFalse();

        return new SamanCallbackResponse
               {
                   MID = mid.Value,
                   State = state.Value,
                   Status = status.Value,
                   Rrn = rrn.Value,
                   RefNum = refNum.Value,
                   ResNum = resNum.Value,
                   TerminalId = terminalId.Value,
                   TraceNo = traceNo.Value,
                   Amount = amount.Value,
                   Wage = wage.Value,
                   SecurePan = securePan.Value,
                   HashedCardNumber = hashedCardNumber.Value,
               };

        // var status = await httpRequest.TryGetParamAsync("status", cancellationToken).ConfigureAwaitFalse();
        //
        // if (!status.Exists || status.Value.IsNullOrEmpty())
        // {
        //     message = messagesOptions.InvalidDataReceivedFromGateway;
        // }
        // else
        // {
        //     var referenceIdResult = await httpRequest.TryGetParamAsync("ResNum", cancellationToken).ConfigureAwaitFalse();
        //     if (referenceIdResult.Exists) referenceId = referenceIdResult.Value;
        //
        //     var transactionIdResult = await httpRequest.TryGetParamAsync("RefNum", cancellationToken).ConfigureAwaitFalse();
        //     if (transactionIdResult.Exists) transactionId = transactionIdResult.Value;
        //
        //     isSuccess = status.Value.Equals("OK", StringComparison.OrdinalIgnoreCase);
        //
        //     if (!isSuccess)
        //     {
        //         message = SamanStateTranslator.Translate(status.Value, messagesOptions);
        //     }
        // }
        //
        // return new SamanCallbackResult
        //        {
        //            IsSucceed = isSuccess,
        //            ReferenceId = referenceId,
        //            TransactionId = transactionId,
        //            SecurePan = securePan.Value,
        //            Cid = cid.Value,
        //            TraceNo = traceNo.Value,
        //            Rrn = rrn.Value,
        //            Message = message
        //        };
    }

    public static IPaymentFetchResult CreateFetchResult(SamanCallbackResponse callbackResponse,
                                                        InvoiceContext invoiceContext,
                                                        SamanGatewayAccount gatewayAccount,
                                                        MessagesOptions messagesOptions)
    {
        var isCallbackResponseValid = ValidateCallbackResponse(callbackResponse, invoiceContext, gatewayAccount, out var message);

        var isReceivedSuccessFromGateway = callbackResponse.Status == "2";

        var isSucceed = isCallbackResponseValid && isReceivedSuccessFromGateway;

        if (!isReceivedSuccessFromGateway)
        {
            message = SamanResultTranslator.Translate(callbackResponse.Status, messagesOptions);
        }
        else if (isSucceed)
        {
            message = messagesOptions.PaymentSucceed;
        }

        var result = new PaymentFetchResult
                     {
                         Status = isSucceed ? PaymentFetchResultStatus.ReadyForVerifying : PaymentFetchResultStatus.Failed,
                         GatewayResponseCode = callbackResponse.Status,
                         Message = message
                     };

        result.AddSamanAdditionalData(MapToAdditionalData(callbackResponse));

        return result;
    }

    public static SamanVerificationRequest CreateVerificationRequest(SamanCallbackResponse callbackResponse,
                                                                     SamanGatewayAccount gatewayAccount)
    {
        return new()
               {
                   RefNum = callbackResponse.RefNum!,
                   TerminalNumber = gatewayAccount.TerminalId
               };
    }

    public static PaymentVerifyResult CreateVerifyResult(SamanGatewayAccount gatewayAccount,
                                                         SamanCallbackResponse callbackResponse,
                                                         SamanVerificationAndRefundResponse verificationResponse,
                                                         InvoiceContext invoiceContext,
                                                         MessagesOptions messagesOptions)
    {
        var isSuccess = verificationResponse.TransactionDetail.TerminalNumber.ToString() == gatewayAccount.TerminalId &&
                        verificationResponse.TransactionDetail.AffectiveAmount == (long)invoiceContext.Payment.Amount &&
                        verificationResponse.TransactionDetail.RefNum == callbackResponse.RefNum;


        var message = isSuccess
            ? messagesOptions.PaymentSucceed
            : SamanResultTranslator.Translate(verificationResponse.ResultCode.ToString(), messagesOptions);

        var result = new PaymentVerifyResult
                     {
                         Status = isSuccess ? PaymentVerifyResultStatus.Succeed : PaymentVerifyResultStatus.Failed,
                         TransactionCode = verificationResponse.TransactionDetail.Rrn,
                         GatewayResponseCode = verificationResponse.ResultCode.ToString(),
                         Message = message
                     };

        result.DatabaseAdditionalData.Add(RefNumKey, callbackResponse.RefNum);

        result.AddSamanAdditionalData(MapToAdditionalData(callbackResponse));

        return result;
    }

    public static SamanReverseRequest CreateReverseRequest(InvoiceContext context, SamanGatewayAccount account)
    {
        var verificationTransaction = context.Transactions.SingleOrDefault(transaction => transaction.Type == TransactionType.Verify);

        if (string.IsNullOrEmpty(verificationTransaction?.AdditionalData) ||
            !verificationTransaction.ToDictionary().ContainsKey(RefNumKey))
        {
            throw new InvalidOperationException($"No Transaction of type Verification or additional data found for reversing the invoice {context.Payment.TrackingNumber}.");
        }

        return new()
               {
                   TerminalNumber = account.TerminalId,
                   RefNum = verificationTransaction.ToDictionary()[RefNumKey]
               };
    }

    public static PaymentRefundResult CreateRefundResult(SamanVerificationAndRefundResponse response)
    {
        return new PaymentRefundResult
               {
                   Status = response.Success ? PaymentRefundResultStatus.Succeed : PaymentRefundResultStatus.Failed,
                   GatewayResponseCode = response.ResultCode.ToString()
               };
    }

    private static bool ValidateCallbackResponse(SamanCallbackResponse callbackResponse,
                                                 InvoiceContext invoiceContext,
                                                 SamanGatewayAccount gatewayAccount,
                                                 out string failures)
    {
        const string nullOrEmptyString = "IsReceivedFromTheGatewayAsNullOrEmpty";

        var validationFailures = new StringBuilder();
        var isValid = true;

        if (string.IsNullOrWhiteSpace(callbackResponse.Status))
        {
            isValid = false;

            validationFailures.AppendLine($"{nameof(SamanCallbackResponse.Status)}{nullOrEmptyString}");
        }

        if (string.IsNullOrWhiteSpace(callbackResponse.TerminalId))
        {
            isValid = false;

            validationFailures.AppendLine($"{nameof(SamanCallbackResponse.TerminalId)}{nullOrEmptyString}");
        }
        else if (callbackResponse.TerminalId != gatewayAccount.TerminalId)
        {
            isValid = false;

            validationFailures.AppendLine($"ReceivedInvalidTerminalId: {callbackResponse.TerminalId}");
        }

        if (string.IsNullOrWhiteSpace(callbackResponse.RefNum))
        {
            isValid = false;

            validationFailures.AppendLine($"{nameof(SamanCallbackResponse.RefNum)}{nullOrEmptyString}");
        }
        else if (callbackResponse.RefNum != invoiceContext.Payment.TrackingNumber.ToString())
        {
            isValid = false;

            validationFailures.AppendLine($"ReceivedInvalidTrackingNumber: {callbackResponse.RefNum}");
        }

        if (string.IsNullOrWhiteSpace(callbackResponse.ResNum))
        {
            isValid = false;

            validationFailures.AppendLine($"{nameof(SamanCallbackResponse.ResNum)}{nullOrEmptyString}");
        }

        if (string.IsNullOrWhiteSpace(callbackResponse.Amount))
        {
            isValid = false;

            validationFailures.AppendLine($"{nameof(SamanCallbackResponse.Amount)}{nullOrEmptyString}");
        }
        else if (callbackResponse.Amount != ((long)invoiceContext.Payment.Amount).ToString())
        {
            isValid = false;

            validationFailures.AppendLine($"ReceivedInvalidAmount: {callbackResponse.Amount}");
        }

        if (string.IsNullOrWhiteSpace(callbackResponse.Rrn))
        {
            isValid = false;

            validationFailures.AppendLine($"{nameof(SamanCallbackResponse.Rrn)}{nullOrEmptyString}");
        }

        failures = validationFailures.ToString();

        return isValid;
    }

    private static SamanVerificationAdditionalData MapToAdditionalData(SamanCallbackResponse callbackResponse)
    {
        return new SamanVerificationAdditionalData
               {
                   Rrn = callbackResponse.Rrn,
                   TraceNo = callbackResponse.TraceNo,
                   RefNum = callbackResponse.RefNum,
                   ResNum = callbackResponse.ResNum,
                   SecurePan = callbackResponse.SecurePan,
                   HashedCardNumber = callbackResponse.HashedCardNumber
               };
    }
}
