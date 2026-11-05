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
using Parbad.Exceptions;
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

    public static PaymentRequestResult CreateRequestResult(ZarinPalRequestResultModelData data,
                                                           HttpContext httpContext,
                                                           ZarinPalGatewayAccount account,
                                                           ZarinPalGatewayOptions gatewayOptions,
                                                           MessagesOptions messagesOptions)
    {
        if (!IsSucceedCode(data.Code))
        {
            var message = ZarinPalCodeTranslator.Translate(data.Code, messagesOptions);

            return PaymentRequestResult.Failed(message, account.Name, data.Code.ToString());
        }

        var paymentPageUrl = GetPaymentPageUrl(account.IsSandbox, gatewayOptions) + data.Authority;

        var result = PaymentRequestResult.SucceedWithRedirect(account.Name, httpContext, paymentPageUrl);

        result.GatewayResponseCode = data.Code.ToString();
        result.DatabaseAdditionalData.Add(AuthorityDatabaseKey, data.Authority);

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

    public static PaymentVerifyResult CreateVerifyResult(ZarinPalOriginalVerificationResult verificationResult,
                                                         ZarinPalGatewayAccount account,
                                                         MessagesOptions messagesOptions)
    {
        PaymentVerifyResult result;

        // 101: this payment was verified before (e.g. the first response was lost). The Authority comes from
        // the database, so it is this invoice's own payment and it counts as paid.
        if (IsSucceedCode(verificationResult.Code) || verificationResult.Code == NumericAlreadyOkResult)
        {
            result = PaymentVerifyResult.Succeed(verificationResult.RefId, messagesOptions.PaymentSucceed);
        }
        else
        {
            var message = ZarinPalCodeTranslator.Translate(verificationResult.Code, messagesOptions);

            result = new PaymentVerifyResult
                     {
                         Status = PaymentVerifyResultStatus.Failed,
                         Message = message
                     };
        }

        result.GatewayAccountName = account.Name;
        result.GatewayResponseCode = verificationResult.Code.ToString();
        result.SetZarinPalOriginalVerificationResult(verificationResult);

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

    public static PaymentRefundResult CreateRefundResult(ZarinPalRefundResultModelData data, ZarinPalGatewayAccount account, MessagesOptions messagesOptions)
    {
        var result = new PaymentRefundResult
                     {
                         GatewayResponseCode = data.Code.ToString(),
                         GatewayAccountName = account.Name
                     };

        if (IsSucceedCode(data.Code))
        {
            result.Status = PaymentRefundResultStatus.Succeed;

            result.Message = messagesOptions.PaymentSucceed;
        }
        else
        {
            result.Status = data.Code == NumericAlreadyOkResult ? PaymentRefundResultStatus.AlreadyRefunded : PaymentRefundResultStatus.Failed;

            result.Message = ZarinPalCodeTranslator.Translate(data.Code, messagesOptions);
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

    private static bool IsSucceedCode(int status) => status == NumericOkResult;

    private static bool IsSucceedResult(string status) => string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase);
}
