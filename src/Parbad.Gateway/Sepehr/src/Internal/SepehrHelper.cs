// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using Parbad.Storage.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.Sepehr.Internal;

internal static class SepehrHelper
{
    public const string VerificationAdditionalDataKey = "SepehrVerificationAdditionalData";

    public static object CreateRequestData(Invoice invoice, SepehrGatewayAccount account)
    {
        return new TokenRequestModel
               {
                   InvoiceId = invoice.TrackingNumber.ToString(),
                   Amount = (long)invoice.Amount,
                   CallbackUrl = invoice.CallbackUrl,
                   TerminalId = account.TerminalId
               };
    }

    public static async Task<PaymentRequestResult> CreateRequestResult(
        HttpResponseMessage responseMessage,
        HttpContext httpContext,
        SepehrGatewayAccount account,
        SepehrGatewayOptions gatewayOptions,
        MessagesOptions messages)
    {
        if (!responseMessage.IsSuccessStatusCode)
        {
            return PaymentRequestResult.Failed($"Operation failed. Http Status: {responseMessage.StatusCode}", account.Name);
        }

        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

        var result = JsonConvert.DeserializeObject<TokenResponseModel>(response);

        if (result == null)
        {
            return PaymentRequestResult.Failed(messages.InvalidDataReceivedFromGateway, account.Name);
        }

        if (result.Status != 0)
        {
            var message = SepehrGatewayResultTranslator.Translate(result.Status.ToString(), messages);

            return PaymentRequestResult.Failed(message, account.Name);
        }

        if (result.AccessToken.IsNullOrWhiteSpace())
        {
            return PaymentRequestResult.Failed(messages.InvalidDataReceivedFromGateway, account.Name);
        }

        return PaymentRequestResult.SucceedWithPost(account.Name,
                                                    httpContext,
                                                    gatewayOptions.PaymentPageUrl,
                                                    new Dictionary<string, string>
                                                    {
                                                        {"TerminalID", account.TerminalId.ToString()},
                                                        {"token", result.AccessToken}
                                                    });
    }

    public static async Task<CallbackResultModel> CreateCallbackResultAsync(
        InvoiceContext context,
        HttpRequest httpRequest,
        SepehrGatewayAccount account,
        MessagesOptions messages,
        CancellationToken cancellationToken)
    {
        var respCodeParam = await httpRequest.TryGetParamAsync("respcode", cancellationToken).ConfigureAwaitFalse();
        var respMsgParam = await httpRequest.TryGetParamAsync("respmsg", cancellationToken).ConfigureAwaitFalse();
        var invoiceIdParam = await httpRequest.TryGetParamAsync("invoiceid", cancellationToken).ConfigureAwaitFalse();
        var terminalIdParam = await httpRequest.TryGetParamAsync("terminalid", cancellationToken).ConfigureAwaitFalse();
        var amountParam = await httpRequest.TryGetParamAsync("amount", cancellationToken).ConfigureAwaitFalse();
        var traceNumberParam = await httpRequest.TryGetParamAsync("tracenumber", cancellationToken).ConfigureAwaitFalse();
        var rrnParam = await httpRequest.TryGetParamAsync("rrn", cancellationToken).ConfigureAwaitFalse();
        var digitalReceiptParam = await httpRequest.TryGetParamAsync("digitalreceipt", cancellationToken).ConfigureAwaitFalse();
        var cardNumberParam = await httpRequest.TryGetParamAsync("cardnumber", cancellationToken).ConfigureAwaitFalse();

        bool isSucceed;
        long traceNumber = 0;
        long rrn = 0;
        string message;

        if (!respCodeParam.Exists)
        {
            message = messages.InvalidDataReceivedFromGateway;

            isSucceed = false;
        }
        else
        {
            if (!int.TryParse(respCodeParam.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var responseCode))
            {
                message = messages.InvalidDataReceivedFromGateway;

                isSucceed = false;
            }
            else if (responseCode != 0)
            {
                message = respMsgParam.Exists ? respMsgParam.Value.ToString() : messages.PaymentFailed;

                isSucceed = false;
            }
            else
            {
                var isValid = true;
                message = messages.InvalidDataReceivedFromGateway;

                if (!invoiceIdParam.Exists || !string.Equals(invoiceIdParam.Value, context.Payment.TrackingNumber.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    isValid = false;
                    message += " InvoiceID is not valid.";
                }

                if (!terminalIdParam.Exists || !string.Equals(terminalIdParam.Value, account.TerminalId.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    isValid = false;
                    message += " TerminalID is not valid.";
                }

                if (!amountParam.Exists || !string.Equals(amountParam.Value, ((long)context.Payment.Amount).ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    isValid = false;
                    message += " Amount is not valid.";
                }

                if (!traceNumberParam.Exists)
                {
                    isValid = false;
                    message += " TraceNumber doesn't exist.";
                }
                else
                {
                    traceNumber = long.Parse(traceNumberParam.Value.ToString(), CultureInfo.InvariantCulture);
                }

                if (!rrnParam.Exists)
                {
                    isValid = false;
                    message += " RRN doesn't exist.";
                }
                else
                {
                    rrn = long.Parse(rrnParam.Value.ToString(), CultureInfo.InvariantCulture);
                }

                if (!digitalReceiptParam.Exists)
                {
                    isValid = false;
                    message += " DigitalReceipt doesn't exist.";
                }

                isSucceed = isValid;
            }
        }

        return new CallbackResultModel
               {
                   IsSucceed = isSucceed,
                   TraceNumber = traceNumber,
                   Rrn = rrn,
                   DigitalReceipt = digitalReceiptParam.Value,
                   CardNumber = cardNumberParam.Value,
                   Message = message
               };
    }

    public static VerificationRollbackRequestModel CreateVerifyData(CallbackResultModel callbackResult, SepehrGatewayAccount account)
    {
        return new VerificationRollbackRequestModel
               {
                   DigitalReceipt = callbackResult.DigitalReceipt,
                   Tid = account.TerminalId
               };
    }

    public static async Task<PaymentVerifyResult> CreateVerifyResult(InvoiceContext context, HttpResponseMessage responseMessage, CallbackResultModel callbackResult, MessagesOptions messagesOptions)
    {
        if (!responseMessage.IsSuccessStatusCode)
        {
            // A JSON error body is the bank's answer; anything else (e.g. a proxy's error page) throws, so the
            // payment stays open for a retry.
            await responseMessage.ReadJsonStringAsync().ConfigureAwaitFalse();

            return PaymentVerifyResult.Failed($"Operation failed. Http Status: {responseMessage.StatusCode}");
        }

        var result = await responseMessage.ReadJsonAsync<VerificationRollbackResponseModel>().ConfigureAwaitFalse();

        if (result.Status.IsNullOrWhiteSpace())
        {
            return PaymentVerifyResult.Failed(messagesOptions.InvalidDataReceivedFromGateway);
        }

        PaymentVerifyResultStatus status;
        string message;
        var isAlreadyVerifiedAtBank = false;

        switch (result.Status.ToLowerInvariant())
        {
            case "ok":
                if (!long.TryParse(result.ReturnId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var amount))
                {
                    message = $"{messagesOptions.InvalidDataReceivedFromGateway} Cannot parse the amount. Amount: {result.ReturnId}";
                    status = PaymentVerifyResultStatus.Failed;
                }
                else if (amount != (long)context.Payment.Amount)
                {
                    status = PaymentVerifyResultStatus.Failed;
                    message = $"{messagesOptions.PaymentFailed} The amount that received from the gateway and the actual invoice amount are not equal. Invoice amount: {(long)context.Payment.Amount}. Received amount: {result.ReturnId}";
                }
                else
                {
                    status = PaymentVerifyResultStatus.Succeed;
                    message = messagesOptions.PaymentSucceed;
                }

                break;

            case "nok":
                status = PaymentVerifyResultStatus.Failed;
                message = SepehrGatewayResultTranslator.Translate(result.ReturnId, messagesOptions);
                if (message.IsNullOrEmpty())
                {
                    message = messagesOptions.PaymentFailed;
                }
                break;

            case "duplicate":
                // Verified before (e.g. the first response was lost). The digital receipt comes from the browser, so
                // Parbad records the payment as paid only if the receipt has not settled another payment.
                status = PaymentVerifyResultStatus.Failed;
                isAlreadyVerifiedAtBank = true;
                message = !result.Message.IsNullOrWhiteSpace() ? result.Message : messagesOptions.PaymentIsAlreadyProcessedBefore;
                break;

            default:
                status = PaymentVerifyResultStatus.Failed;
                message = $"{messagesOptions.InvalidDataReceivedFromGateway} Verification failed. Status value is not valid.";
                break;
        }

        var verificationResult = new PaymentVerifyResult
                                 {
                                     Status = status,
                                     Message = message,
                                     // The RRN is shown to the customer; the bank verifies the digital receipt.
                                     TransactionCode = callbackResult.Rrn.ToString(),
                                     BankReference = callbackResult.DigitalReceipt,
                                     IsAlreadyVerifiedAtBank = isAlreadyVerifiedAtBank
                                 };

        var verificationAdditionalData = new SepehrGatewayVerificationAdditionalData
                                         {
                                             TraceNumber = callbackResult.TraceNumber,
                                             DigitalReceipt = callbackResult.DigitalReceipt,
                                             Rrn = callbackResult.Rrn,
                                             CardNumber = callbackResult.CardNumber
                                         };
        verificationResult.SetSepehrAdditionalData(verificationAdditionalData);
        var serializedAdditionalData = JsonConvert.SerializeObject(verificationAdditionalData);
        verificationResult.DatabaseAdditionalData.Add(VerificationAdditionalDataKey, serializedAdditionalData);

        return verificationResult;
    }

    public static VerificationRollbackRequestModel CreateRefundData(InvoiceContext context, SepehrGatewayAccount account)
    {
        var verificationRecord = context.Transactions.LastOrDefault(transaction => transaction.Type == TransactionType.Verify && transaction.IsSucceed);

        if (verificationRecord == null)
        {
            throw new Exception("No verification record found in transactions.");
        }

        var additionalData = JsonConvert.DeserializeObject<IDictionary<string, string>>(verificationRecord.AdditionalData ?? "{}");

        if (additionalData == null || !additionalData.ContainsKey(VerificationAdditionalDataKey))
        {
            throw new Exception("No Additional Data found in transactions record.");
        }

        var verificationAdditionalData = JsonConvert.DeserializeObject<SepehrGatewayVerificationAdditionalData>(additionalData[VerificationAdditionalDataKey]);

        if (verificationAdditionalData == null)
        {
            throw new Exception("No Additional Data found in transactions record.");
        }

        return new VerificationRollbackRequestModel
               {
                   Tid = account.TerminalId,
                   DigitalReceipt = verificationAdditionalData.DigitalReceipt
               };
    }

    public static async Task<IPaymentRefundResult> CreateRefundResult(InvoiceContext context, HttpResponseMessage responseMessage, MessagesOptions messagesOptions)
    {
        if (!responseMessage.IsSuccessStatusCode)
        {
            await responseMessage.ReadJsonStringAsync().ConfigureAwaitFalse();

            return PaymentRefundResult.Failed($"Operation failed. Http Status: {responseMessage.StatusCode}");
        }

        var result = await responseMessage.ReadJsonAsync<VerificationRollbackResponseModel>().ConfigureAwaitFalse();

        if (result.Status.IsNullOrWhiteSpace())
        {
            return PaymentRefundResult.Failed(messagesOptions.InvalidDataReceivedFromGateway);
        }

        PaymentRefundResultStatus status;
        string message;

        switch (result.Status.ToLowerInvariant())
        {
            case "ok":
                if (!long.TryParse(result.ReturnId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var amount))
                {
                    message = $"{messagesOptions.InvalidDataReceivedFromGateway} Cannot parse the amount. Amount: {result.ReturnId}";
                    status = PaymentRefundResultStatus.Failed;
                }
                else if (amount != (long)context.Payment.Amount)
                {
                    status = PaymentRefundResultStatus.Failed;
                    message = $"{messagesOptions.PaymentFailed} The amount that received from the gateway and the actual invoice amount are not equal. Invoice amount: {(long)context.Payment.Amount}. Received amount: {result.ReturnId}";
                }
                else
                {
                    status = PaymentRefundResultStatus.Succeed;
                    message = messagesOptions.PaymentSucceed;
                }

                break;

            case "nok":
                status = PaymentRefundResultStatus.Failed;
                message = SepehrGatewayResultTranslator.Translate(result.ReturnId, messagesOptions);
                if (message.IsNullOrEmpty())
                {
                    message = messagesOptions.PaymentFailed;
                }
                break;

            case "duplicate":
                status = PaymentRefundResultStatus.AlreadyRefunded;
                message = !result.Message.IsNullOrWhiteSpace() ? result.Message : "The invoice has been already refunded before.";
                break;

            default:
                status = PaymentRefundResultStatus.Failed;
                message = $"{messagesOptions.InvalidDataReceivedFromGateway} Operation failed. Status value is not valid.";
                break;
        }

        return new PaymentRefundResult
               {
                   Status = status,
                   Message = message
               };
    }
}