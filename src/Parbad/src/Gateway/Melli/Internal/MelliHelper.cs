// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Gateway.Melli.Internal.Models;
using Parbad.Gateway.Melli.Internal.ResultTranslator;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Storage.Abstractions.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.Melli.Internal;

internal static class MelliHelper
{
    private const int SuccessCode = 0;
    private const int DuplicateTrackingNumberCode = 1011;
    private const string TokenDatabaseKey = "MelliToken";

    public static object CreateRequestData(Invoice invoice, MelliGatewayAccount account, IMelliGatewayCrypto crypto)
    {
        var data = $"{account.TerminalId};{invoice.TrackingNumber};{(long)invoice.Amount}";

        var signedData = crypto.Encrypt(account.TerminalKey, data);

        return CreateRequestObject(
                                   account.TerminalId,
                                   account.MerchantId,
                                   (long)invoice.Amount,
                                   signedData,
                                   invoice.CallbackUrl,
                                   invoice.TrackingNumber);
    }

    public static PaymentRequestResult CreateRequestResult(
        MelliApiRequestResult result,
        HttpContext httpContext,
        MelliGatewayAccount account,
        MelliGatewayOptions gatewayOptions,
        MessagesOptions messagesOptions)
    {
        if (result == null)
        {
            return PaymentRequestResult.Failed(messagesOptions.UnexpectedErrorText);
        }

        var isSucceed = result.ResCode == SuccessCode;

        if (!isSucceed)
        {
            string message;

            if (result.ResCode == DuplicateTrackingNumberCode)
            {
                message = messagesOptions.DuplicateTrackingNumber;
            }
            else
            {
                message = !result.Description.IsNullOrEmpty()
                    ? result.Description
                    : MelliRequestResultTranslator.Translate(result.ResCode, messagesOptions);
            }

            return PaymentRequestResult.Failed(message, account.Name);
        }

        var paymentPageUrl = $"{gatewayOptions.PaymentPageUrl}?token={result.Token}";

        var requestResult = PaymentRequestResult.SucceedWithRedirect(account.Name, httpContext, paymentPageUrl);

        // Stored so that the callback can be bound to this invoice (see CreateCallbackResultAsync).
        requestResult.DatabaseAdditionalData.Add(TokenDatabaseKey, result.Token);

        return requestResult;
    }

    public static async Task<MelliCallbackResult> CreateCallbackResultAsync(
        InvoiceContext context,
        HttpRequest httpRequest,
        MelliGatewayAccount account,
        IMelliGatewayCrypto crypto,
        MessagesOptions messagesOptions,
        CancellationToken cancellationToken)
    {
        var apiResponseCode = await httpRequest.TryGetParamAsAsync<int>("ResCode", cancellationToken).ConfigureAwaitFalse();

        if (!apiResponseCode.Exists || apiResponseCode.Value != SuccessCode)
        {
            return new MelliCallbackResult
                   {
                       IsSucceed = false,
                       Message = messagesOptions.PaymentFailed
                   };
        }

        var apiToken = await httpRequest.TryGetParamAsync("Token", cancellationToken).ConfigureAwaitFalse();
        var apiOrderId = await httpRequest.TryGetParamAsAsync<long>("OrderId", cancellationToken).ConfigureAwaitFalse();

        // The order id comes from the browser, so it proves nothing on its own; the token is what the bank
        // verifies, so it must be the one issued for this invoice.
        if (!apiOrderId.Exists || apiOrderId.Value != context.Payment.TrackingNumber ||
            !IsTokenIssuedForThisInvoice(apiToken.Value, context))
        {
            return new MelliCallbackResult
                   {
                       IsSucceed = false,
                       Token = apiToken.Value,
                       Message = messagesOptions.InvalidDataReceivedFromGateway
                   };
        }

        var signedData = crypto.Encrypt(account.TerminalKey, apiToken.Value);

        var dataToVerify = CreateVerifyObject(apiToken.Value, signedData);

        return new MelliCallbackResult
               {
                   IsSucceed = true,
                   Token = apiToken.Value,
                   JsonDataToVerify = dataToVerify
               };
    }

    public static PaymentVerifyResult CreateVerifyResult(MelliApiVerifyResult result, InvoiceContext context, MessagesOptions messagesOptions)
    {
        if (result == null)
        {
            return PaymentVerifyResult.Failed(messagesOptions.UnexpectedErrorText);
        }

        // Compare what the bank reports with the invoice. Values the bank does not send (0) are not compared.
        if (result.ResCode == SuccessCode &&
            ((result.Amount > 0 && result.Amount != (long)context.Payment.Amount) ||
             (result.OrderId > 0 && result.OrderId != context.Payment.TrackingNumber)))
        {
            return PaymentVerifyResult.Failed($"{messagesOptions.PaymentFailed} The amount or order id reported by the gateway does not match the invoice.");
        }

        string message;

        if (!result.Description.IsNullOrEmpty())
        {
            message = result.Description;
        }
        else
        {
            message = MelliVerifyResultTranslator.Translate(result.ResCode, messagesOptions);
        }

        var status = result.ResCode == SuccessCode
            ? PaymentVerifyResultStatus.Succeed
            : PaymentVerifyResultStatus.Failed;

        return new PaymentVerifyResult
               {
                   Status = status,
                   TransactionCode = result.RetrivalRefNo,
                   Message = message
               };
    }

    private static object CreateRequestObject(string terminalId, string merchantId, long amount, string signedData, string callbackUrl, long orderId)
    {
        return new
               {
                   TerminalId = terminalId,
                   MerchantId = merchantId,
                   Amount = amount,
                   SignData = signedData,
                   ReturnUrl = callbackUrl,
                   LocalDateTime = DateTime.Now,
                   OrderId = orderId.ToString()
               };
    }

    private static object CreateVerifyObject(string apiToken, string signedData)
    {
        return new
               {
                   token = apiToken,
                   SignData = signedData
               };
    }

    /// <summary>
    /// Checks that the callback's token is the one Sadad issued for this invoice. Invoices requested before
    /// the token was stored have nothing to compare with and are accepted as before.
    /// </summary>
    private static bool IsTokenIssuedForThisInvoice(string callbackToken, InvoiceContext context)
    {
        var requestTransaction = context.Transactions?.SingleOrDefault(transaction => transaction.Type == TransactionType.Request);

        if (string.IsNullOrEmpty(requestTransaction?.AdditionalData) ||
            !requestTransaction.ToDictionary().TryGetValue(TokenDatabaseKey, out var issuedToken))
        {
            return true;
        }

        return string.Equals(issuedToken, callbackToken, StringComparison.Ordinal);
    }
}