// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parbad.Abstraction;
using Parbad.Gateway.ZarinPal.Models;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.Gateway.ZarinPal.Internal;

internal static class ZarinPalHelper
{
    private const int NumericOkResult = 100;
    private const int NumericAlreadyOkResult = 101;
    private const string AuthorityDatabaseKey = "Authority";

    public static string ZarinPalRequestAdditionalKeyName => "ZarinPalRequest";

    public static ZarinPalRequestModel CreateRequestModel(ZarinPalGatewayAccount account, Invoice invoice)
    {
        if (!invoice.Properties.ContainsKey(ZarinPalRequestAdditionalKeyName) ||
            invoice.Properties[ZarinPalRequestAdditionalKeyName] is not ZarinPalInvoice zarinPalInvoice)
        {
            throw new InvalidOperationException("Request failed. ZarinPal Gateway needs invoice information. Please use the SetZarinPalData method of Invoice Builder to add the information.");
        }

        return new()
               {
                   MerchantId = account.MerchantId,
                   Amount = invoice.Amount,
                   CallbackUrl = invoice.CallbackUrl,
                   Description = zarinPalInvoice.Description,
                   Email = zarinPalInvoice.Email,
                   Mobile = zarinPalInvoice.Mobile
               };
    }

    public static PaymentRequestResult CreateRequestResult(ZarinPalRequestResultModel resultModel,
                                                           HttpContext httpContext,
                                                           ZarinPalGatewayAccount account,
                                                           ZarinPalGatewayOptions gatewayOptions,
                                                           MessagesOptions messagesOptions)
    {
        if (!IsSucceedCode(resultModel.Data.Code))
        {
            var message = ZarinPalCodeTranslator.Translate(resultModel.Data.Code, messagesOptions);

            return PaymentRequestResult.Failed(message, account.Name);
        }

        var paymentPageUrl = GetPaymentPageUrl(account.IsSandbox, gatewayOptions) + resultModel.Data.Authority;

        var result = PaymentRequestResult.SucceedWithRedirect(account.Name, httpContext, paymentPageUrl);

        result.GatewayResponseCode = resultModel.Data.Code.ToString();
        result.DatabaseAdditionalData.Add(AuthorityDatabaseKey, resultModel.Data.Authority);

        return result;
    }

    public static async Task<ZarinPalCallbackResult> CreateCallbackResultAsync(
        HttpRequest httpRequest,
        MessagesOptions messagesOptions,
        CancellationToken cancellationToken)
    {
        var authority = await httpRequest.TryGetParamAsync("Authority", cancellationToken).ConfigureAwaitFalse();
        var status = await httpRequest.TryGetParamAsync("Status", cancellationToken).ConfigureAwaitFalse();
        string message = null;

        var isSucceed = status.Exists && IsSucceedResult(status.Value);

        if (!isSucceed)
        {
            message = messagesOptions.PaymentFailed;
        }

        return new ZarinPalCallbackResult
               {
                   Authority = authority.Value,
                   Status = status.Value,
                   IsSucceed = isSucceed,
                   Message = message
               };
    }

    public static ZarinPalVerificationModel CreateVerificationModel(ZarinPalGatewayAccount account, InvoiceContext context, Money amount)
    {
        var authority = GetAuthorityFromAdditionalData(context);

        return new()
               {
                   MerchantId = account.MerchantId,
                   Amount = amount,
                   Authority = authority
               };
    }

    public static PaymentVerifyResult CreateVerifyResult(ZarinPalResultModel<ZarinPalOriginalVerificationResult> verificationResult,
                                                         ZarinPalGatewayAccount account,
                                                         MessagesOptions messagesOptions)
    {
        PaymentVerifyResult result;

        if (IsSucceedCode(verificationResult.Data.Code))
        {
            result = PaymentVerifyResult.Succeed(verificationResult.Data.RefId, messagesOptions.PaymentSucceed);
        }
        else
        {
            var message = ZarinPalCodeTranslator.Translate(verificationResult.Data.Code, messagesOptions);

            var verifyResultStatus = verificationResult.Data.Code == NumericAlreadyOkResult
                ? PaymentVerifyResultStatus.AlreadyVerified
                : PaymentVerifyResultStatus.Failed;

            result = new PaymentVerifyResult
                     {
                         Status = verifyResultStatus,
                         Message = message
                     };
        }

        result.GatewayAccountName = account.Name;
        result.GatewayResponseCode = verificationResult.Data.Code.ToString();
        result.SetZarinPalOriginalVerificationResult(verificationResult.Data);

        return result;
    }

    public static ZarinPalRefundModel CreateRefundModel(string authority, ZarinPalGatewayAccount account)
    {
        return new()
               {
                   MerchantId = account.MerchantId,
                   Authority = authority
               };
    }

    public static PaymentRefundResult CreateRefundResult(ZarinPalRefundResultModel resultModel, ZarinPalGatewayAccount account, MessagesOptions messagesOptions)
    {
        var result = new PaymentRefundResult
                     {
                         GatewayResponseCode = resultModel.Data.Code.ToString(),
                         GatewayAccountName = account.Name
                     };

        if (IsSucceedCode(resultModel.Data.Code))
        {
            result.Status = PaymentRefundResultStatus.Succeed;

            result.Message = messagesOptions.PaymentSucceed;
        }
        else
        {
            result.Status = resultModel.Data.Code == NumericAlreadyOkResult ? PaymentRefundResultStatus.AlreadyRefunded : PaymentRefundResultStatus.Failed;

            result.Message = ZarinPalCodeTranslator.Translate(resultModel.Data.Code, messagesOptions);
        }

        return result;
    }

    public static string GetAuthorityFromAdditionalData(InvoiceContext context)
    {
        var transaction = context.Transactions.SingleOrDefault(_ => _.Type == TransactionType.Request);

        if (transaction == null)
        {
            throw new InvalidOperationException($"No transaction of type '{TransactionType.Request}' found for the invoice #{context.Payment.TrackingNumber}.");
        }

        var additionalData = JsonConvert.DeserializeObject<IDictionary<string, string>>(transaction.AdditionalData);

        string authority = null;

        foreach (var key in additionalData.Keys)
        {
            if (string.Equals(key, AuthorityDatabaseKey, StringComparison.OrdinalIgnoreCase))
            {
                authority = additionalData[key];

                break;
            }
        }

        if (string.IsNullOrEmpty(authority))
        {
            throw new InvalidOperationException($"Authority not found in the Transaction record of invoice #{context.Payment.TrackingNumber}.");
        }

        return authority;
    }

    public static string GetApiRequestUrl(bool isSandbox, ZarinPalGatewayOptions gatewayOptions)
    {
        return isSandbox
            ? gatewayOptions.SandboxApiRequestUrl
            : gatewayOptions.ApiRequestUrl;
    }

    public static string GetApiVerificationUrl(bool isSandbox, ZarinPalGatewayOptions gatewayOptions)
    {
        return isSandbox
            ? gatewayOptions.SandboxApiVerificationUrl
            : gatewayOptions.ApiVerificationUrl;
    }

    public static string GetApiRefundUrl(bool isSandbox, ZarinPalGatewayOptions gatewayOptions)
    {
        return isSandbox
            ? gatewayOptions.SandboxApiRefundUrl
            : gatewayOptions.ApiRefundUrl;
    }

    private static string GetPaymentPageUrl(bool isSandbox, ZarinPalGatewayOptions gatewayOptions)
    {
        return isSandbox
            ? gatewayOptions.SandboxPaymentPageUrl
            : gatewayOptions.PaymentPageUrl;
    }

    public static async Task<(int? Code, string Message)> TryGetError(HttpResponseMessage httpResponseMessage, MessagesOptions messagesOptions)
    {
        if (httpResponseMessage.StatusCode == HttpStatusCode.InternalServerError)
        {
            return (null, messagesOptions.UnexpectedErrorText);
        }

        var jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync();

        JObject jsonObject = JObject.Parse(jsonResponse);
        var errorsToken = jsonObject["errors"];
        if (errorsToken != null && errorsToken.Type == JTokenType.Object)
        {
            jsonObject["errors"] = new JArray(errorsToken);
        }

        var failedResult = JsonConvert.DeserializeObject<ZarinPalFailedResult>(jsonObject.ToString());

        string message;
        int? errorCode = null;

        if (failedResult?.Errors != null && failedResult.Errors.Any())
        {
            errorCode = failedResult.Errors.First().Code;

            message = ZarinPalCodeTranslator.Translate(errorCode.Value, messagesOptions);
        }
        else
        {
            message = $"ZarinPal HTTP request failed. Status code {httpResponseMessage.StatusCode}";
        }

        return (errorCode, message);
    }

    private static bool IsSucceedCode(int status) => status == NumericOkResult;

    private static bool IsSucceedResult(string status) => string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase);
}
