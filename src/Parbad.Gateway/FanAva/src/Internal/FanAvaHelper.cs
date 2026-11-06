// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Gateway.FanAva.Internal.Models;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using Parbad.Storage.Abstractions.Models;
using Parbad.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.FanAva.Internal;

internal static class FanAvaHelper
{
    private const string SucceedResult = "erSucceed";
    private const string TokenDatabaseKey = "FanAvaToken";
    private const string RefNumDatabaseKey = "FanAvaRefNum";

    internal static FanAvaRequestModel CreateRequestModel(Invoice invoice, FanAvaGatewayAccount account)
    {
        var additionalData = invoice.GetFanAvaAdditionalData();

        string transType;

        if (additionalData == null)
        {
            transType = "EN_GOODS";
        }
        else
        {
            transType = additionalData.Type == FanAvaGatewayAdditionalDataRequestType.Goods
                ? "EN_GOODS"
                : "EN_BILL_PAY";
        }

        return new FanAvaRequestModel
               {
                   WSContext = new FanAvaRequestModel.WSContextModel
                               {
                                   UserId = account.UserId,
                                   Password = account.Password
                               },
                   TransType = transType,
                   ReserveNum = invoice.TrackingNumber.ToString(),
                   Amount = (long)invoice.Amount,
                   RedirectUrl = invoice.CallbackUrl,
                   Email = additionalData?.Email,
                   MobileNo = additionalData?.MobileNumber,
                   GoodsReferenceId = additionalData?.GoodsReferenceId,
                   MerchantGoodReferenceId = additionalData?.MerchantGoodReferenceId,
                   ApportionmentAccountList = additionalData?.ApportionmentAccountList
               };
    }

    internal static async Task<PaymentRequestResult> CreateRequestResult(
        HttpResponseMessage responseMessage,
        HttpContext httpContext,
        FanAvaGatewayAccount account,
        FanAvaGatewayOptions gatewayOptions)
    {
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

        if (!responseMessage.IsSuccessStatusCode)
        {
            return PaymentRequestResult.Failed(response, account.Name);
        }

        var result = JsonConvert.DeserializeObject<FanAvaRequestResultModel>(response);

        if (result is not { IsSucceed: true } || string.IsNullOrEmpty(result.Token))
        {
            return PaymentRequestResult.Failed($"The gateway did not issue a payment token. Result: {result?.Result}", account.Name, result?.Result);
        }

        var paymentPageUrl = QueryHelper.AddQueryString(gatewayOptions.PaymentPageUrl, new Dictionary<string, string>
                                                                                       {
                                                                                           {"token", result.Token},
                                                                                           {"lang", "fa"}
                                                                                       });

        var requestResult = PaymentRequestResult.SucceedWithRedirect(account.Name, httpContext, paymentPageUrl);

        // Stored so that the callback can be bound to this invoice (see CreateCallbackResult).
        requestResult.DatabaseAdditionalData.Add(TokenDatabaseKey, result.Token);

        return requestResult;
    }

    internal static async Task<FanAvaCallbackResultModel> CreateCallbackResult(
        HttpRequest httpRequest,
        InvoiceContext context,
        MessagesOptions messagesOptions,
        FanAvaGatewayAccount gatewayAccount,
        CancellationToken cancellationToken)
    {
        var state = await httpRequest.TryGetParamAsAsync<string>("State", cancellationToken).ConfigureAwaitFalse();

        if (!state.Exists || state.Value != "ok")
        {
            return new FanAvaCallbackResultModel
                   {
                       IsSucceed = false,
                       Message = messagesOptions.InvalidDataReceivedFromGateway
                   };
        }

        var invoiceNumber = await httpRequest.TryGetParamAsAsync<string>("RefNum", cancellationToken).ConfigureAwaitFalse();
        var token = await httpRequest.TryGetParamAsAsync<string>("token", cancellationToken).ConfigureAwaitFalse();

        if (!IsTokenIssuedForThisInvoice(token.Value, context))
        {
            // The callback carries the token of another invoice (e.g. a cheaper one that was paid).
            return new FanAvaCallbackResultModel
                   {
                       IsSucceed = false,
                       Message = messagesOptions.InvalidDataReceivedFromGateway
                   };
        }

        var data = new FanAvaCheckRequestModel
                   {
                       WSContext = new FanAvaRequestModel.WSContextModel
                                   {
                                       UserId = gatewayAccount.UserId,
                                       Password = gatewayAccount.Password
                                   },
                       Token = token.Value
                   };

        return new FanAvaCallbackResultModel
               {
                   IsSucceed = true,
                   InvoiceNumber = invoiceNumber.Value,
                   CallbackCheckData = data
               };
    }

    internal static async Task<FanAvaCheckResultModel> CreateCheckResult(HttpResponseMessage responseMessage, InvoiceContext context, FanAvaCallbackResultModel callbackResult, MessagesOptions messagesOptions)
    {
        var result = await responseMessage.ReadJsonAsync<FanAvaCheckResultModel>().ConfigureAwaitFalse();

        var invoiceAmount = (long)context.Payment.Amount;

        if (!IsSucceedResult(result.Result))
        {
            result.VerifyResult = PaymentVerifyResult.Failed(messagesOptions.PaymentFailed, result.Result);
        }
        else if (result.InvoiceNumber != callbackResult.InvoiceNumber)
        {
            result.VerifyResult = PaymentVerifyResult.Failed(messagesOptions.PaymentFailed);
        }
        else if (result.Amount > 0 && result.Amount != invoiceAmount)
        {
            result.VerifyResult = PaymentVerifyResult.Failed($"{messagesOptions.PaymentFailed} The amount reported by the gateway ({result.Amount}) does not match the invoice amount ({invoiceAmount}).");
        }

        result.IsSucceed = result.VerifyResult == null;

        return result;
    }

    internal static FanAvaVerifyRequestModel CreateVerifyRequest(InvoiceContext context, FanAvaCallbackResultModel callbackResult)
    {
        return new FanAvaVerifyRequestModel
               {
                   WSContext = callbackResult.CallbackCheckData.WSContext,
                   Token = callbackResult.CallbackCheckData.Token,
                   Amount = (long)context.Payment.Amount,
                   InvoiceNumber = callbackResult.InvoiceNumber
               };
    }

    internal static async Task<IPaymentVerifyResult> CreateVerifyResult(HttpResponseMessage responseMessage, FanAvaCallbackResultModel callbackResult, MessagesOptions messagesOptions)
    {
        var verifyResult = await responseMessage.ReadJsonAsync<FanAvaVerifyResultModel>().ConfigureAwaitFalse();

        if (verifyResult is not { IsSucceed: true })
        {
            return PaymentVerifyResult.Failed(messagesOptions.PaymentFailed, verifyResult?.Result);
        }

        var result = PaymentVerifyResult.Paid(callbackResult.InvoiceNumber, callbackResult.InvoiceNumber, messagesOptions.PaymentSucceed, verifyResult.Result);

        // A refund may run long after the callback (e.g. from an admin action), so it reads them from here.
        result.DatabaseAdditionalData.Add(TokenDatabaseKey, callbackResult.CallbackCheckData.Token);
        result.DatabaseAdditionalData.Add(RefNumDatabaseKey, callbackResult.InvoiceNumber);

        return result;
    }

    /// <summary>
    /// Creates the refund request from the data stored by <see cref="CreateVerifyResult"/>, or returns null if
    /// the payment has no successful verification with that data.
    /// </summary>
    internal static FanAvaVerifyRequestModel CreateRefundRequest(InvoiceContext context, FanAvaGatewayAccount account)
    {
        var verifyTransaction = context.Transactions?.LastOrDefault(transaction => transaction.Type == TransactionType.Verify && transaction.IsSucceed);

        if (string.IsNullOrEmpty(verifyTransaction?.AdditionalData))
        {
            return null;
        }

        var additionalData = verifyTransaction.ToDictionary();

        if (additionalData == null ||
            !additionalData.TryGetValue(TokenDatabaseKey, out var token) ||
            !additionalData.TryGetValue(RefNumDatabaseKey, out var refNum))
        {
            return null;
        }

        return new FanAvaVerifyRequestModel
               {
                   WSContext = new FanAvaRequestModel.WSContextModel
                               {
                                   UserId = account.UserId,
                                   Password = account.Password
                               },
                   Token = token,
                   Amount = (long)context.Payment.Amount,
                   InvoiceNumber = refNum
               };
    }

    internal static async Task<IPaymentRefundResult> CreateRefundResult(HttpResponseMessage responseMessage)
    {
        var refundResult = await responseMessage.ReadJsonAsync<FanAvaVerifyResultModel>().ConfigureAwaitFalse();

        if (refundResult is not { IsSucceed: true })
        {
            return PaymentRefundResult.Failed($"The gateway rejected the refund. Result: {refundResult?.Result}", refundResult?.Result);
        }

        return PaymentRefundResult.Succeed(gatewayResponseCode: refundResult.Result);
    }

    private static bool IsSucceedResult(string result) =>
        string.Equals(result, SucceedResult, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks that the callback's token is the one FanAva issued for this invoice. Invoices requested before
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