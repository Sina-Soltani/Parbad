// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Gateway.AsanPardakht.Internal.Models;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Storage.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Parbad.Gateway.AsanPardakht.Internal;

internal static class AsanPardakhtHelper
{
    public const string RefIdDatabaseKey = "RefId";
    public const string PayGateTranIdDatabaseKey = "PayGateTranId";
    public const string CardNumberDatabaseKey = "CardNumber";
    public const string SettlementResultDatabaseKey = "SettlementResult";

    public static bool TryGetMerchantConfigurationId(AsanPardakhtGatewayAccount account, out long merchantConfigurationId) =>
        long.TryParse(account.MerchantConfigurationId, NumberStyles.Integer, CultureInfo.InvariantCulture, out merchantConfigurationId);

    public static string InvalidMerchantConfigurationIdMessage(AsanPardakhtGatewayAccount account) =>
        $"The AsanPardakht account \"{account.Name}\" has no valid MerchantConfigurationId (a number is expected).";

    public static object CreateTokenRequest(Invoice invoice, long merchantConfigurationId, string localDate) => new
                                                                                                                {
                                                                                                                    serviceTypeId = 1, // sale
                                                                                                                    merchantConfigurationId,
                                                                                                                    localInvoiceId = invoice.TrackingNumber,
                                                                                                                    amountInRials = (long)invoice.Amount,
                                                                                                                    localDate,
                                                                                                                    callbackURL = invoice.CallbackUrl.Url,
                                                                                                                    paymentId = "0",
                                                                                                                    additionalData = ""
                                                                                                                };

    public static PaymentRequestResult CreateRequestResult(AsanPardakhtResponse response,
                                                           AsanPardakhtGatewayAccount account,
                                                           HttpContext httpContext,
                                                           AsanPardakhtGatewayOptions gatewayOptions,
                                                           MessagesOptions messagesOptions)
    {
        var refId = response.IsSucceed ? response.ReadString() : null;

        if (string.IsNullOrWhiteSpace(refId))
        {
            return PaymentRequestResult.Failed(AsanPardakhtResultTranslator.Translate(AsanPardakhtOperation.Token, response, messagesOptions),
                                               account.Name,
                                               response.Code);
        }

        var result = PaymentRequestResult.SucceedWithPost(account.Name,
                                                          httpContext,
                                                          gatewayOptions.PaymentPageUrl,
                                                          new Dictionary<string, string> { { "RefId", refId } });

        // Stored to check later that the bank's record of the payment belongs to this request.
        result.DatabaseAdditionalData.Add(RefIdDatabaseKey, refId);

        return result;
    }

    /// <summary>
    /// Checks the bank's record of the payment (TranResult). It is looked up by this invoice's own id, so it
    /// cannot belong to another invoice; the RefId and the amount are compared as well.
    /// </summary>
    public static (bool IsSucceed, string Message, AsanPardakhtTransactionResult Result) CheckTransactionResult(AsanPardakhtResponse response,
                                                                                                                InvoiceContext context,
                                                                                                                MessagesOptions messagesOptions)
    {
        if (!response.IsSucceed)
        {
            // E.g. no record: the payment was cancelled or failed at the bank.
            return (false, AsanPardakhtResultTranslator.Translate(AsanPardakhtOperation.TranResult, response, messagesOptions), null);
        }

        var result = response.ReadJson<AsanPardakhtTransactionResult>();

        if (!long.TryParse(result.PayGateTranId, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
        {
            return (false, messagesOptions.InvalidDataReceivedFromGateway, null);
        }

        if (!long.TryParse(result.Amount, NumberStyles.Integer, CultureInfo.InvariantCulture, out var amount) ||
            amount != (long)context.Payment.Amount)
        {
            return (false, $"{messagesOptions.PaymentFailed} The amount reported by the gateway ({result.Amount}) does not match the invoice amount ({(long)context.Payment.Amount}).", null);
        }

        var issuedRefId = GetIssuedRefId(context);

        if (issuedRefId != null && !string.Equals(issuedRefId, result.RefId, StringComparison.Ordinal))
        {
            return (false, $"{messagesOptions.PaymentFailed} The payment reported by the gateway was not started by this invoice's request.", null);
        }

        return (true, null, result);
    }

    public static PaymentVerifyResult CreateVerifyResult(AsanPardakhtTransactionResult transaction, MessagesOptions messagesOptions)
    {
        var result = PaymentVerifyResult.Succeed(transaction.Rrn, messagesOptions.PaymentSucceed);

        result.BankReportedAmount = long.Parse(transaction.Amount, NumberStyles.Integer, CultureInfo.InvariantCulture);
        result.DatabaseAdditionalData.Add(PayGateTranIdDatabaseKey, transaction.PayGateTranId);

        if (!transaction.CardNumber.IsNullOrEmpty())
        {
            result.DatabaseAdditionalData.Add(CardNumberDatabaseKey, transaction.CardNumber);
        }

        return result;
    }

    /// <summary>
    /// The RefId stored when this invoice was requested. Invoices requested before 3.12 (SOAP) stored none.
    /// </summary>
    private static string GetIssuedRefId(InvoiceContext context)
    {
        var requestTransaction = context.Transactions?.SingleOrDefault(transaction => transaction.Type == TransactionType.Request);

        if (string.IsNullOrEmpty(requestTransaction?.AdditionalData)) return null;

        return requestTransaction.ToDictionary().TryGetValue(RefIdDatabaseKey, out var refId) ? refId : null;
    }
}