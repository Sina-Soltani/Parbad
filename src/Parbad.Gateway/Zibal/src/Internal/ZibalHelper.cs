// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Storage.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.Zibal.Internal
{
    internal static class ZibalHelper
    {
        private const int SuccessCode = 100;
        private const int FetchSuccessCode = 2;
        private const int PaymentAlreadyVerifiedCode = 201;

        public static string ZibalRequestAdditionalKeyName => "ZibalRequest";
        public static string TrackIdAdditionalDataKey => "TrackId";

        public static object CreateRequestData(Invoice invoice, ZibalGatewayAccount account)
        {
            var requestAdditionalData = invoice.GetZibalRequestData();

            return new ZibalPaymentRequestModel
            {
                //if Merchant value is 'zibal' , Gateway mode is sandBox
                Merchant = account.IsSandbox ? "zibal" : account.Merchant,
                Amount = invoice.Amount,
                Mobile = requestAdditionalData?.MobileNumber,
                OrderId = invoice.TrackingNumber.ToString(),
                CallbackUrl = invoice.CallbackUrl,
                Description = requestAdditionalData?.Description,
                FeeMode = requestAdditionalData?.FeeMode,
                Sms = requestAdditionalData?.SendPaymentLinkViaSms,
                AllowedCards = requestAdditionalData?.AllowedCards
            };
        }

        public static async Task<PaymentRequestResult> CreateRequestResult(
            HttpResponseMessage responseMessage,
            HttpContext httpContext,
            ZibalGatewayAccount account,
            ZibalGatewayOptions gatewayOptions,
            MessagesOptions messagesOptions)
        {
            var message = await responseMessage.Content.ReadAsStringAsync();

            var response = JsonConvert.DeserializeObject<ZibalResponseModel>(message);

            if (response == null)
            {
                return PaymentRequestResult.Failed(messagesOptions.InvalidDataReceivedFromGateway, account.Name);
            }

            if (response.Result != SuccessCode)
            {
                var failureMessage = ZibalTranslator.TranslateResult(response.Result) ?? response.Message ?? messagesOptions.PaymentFailed;

                return PaymentRequestResult.Failed(failureMessage, account.Name, response.Result.ToString());
            }

            var paymentPageUrl = string.IsNullOrEmpty(response.PayLink)
                                ? GetPaymentPageUrl(gatewayOptions.PaymentPageUrl, response.TrackId)
                                : response.PayLink;

            var result = PaymentRequestResult.SucceedWithRedirect(account.Name, httpContext, paymentPageUrl);

            result.DatabaseAdditionalData.Add(TrackIdAdditionalDataKey, response.TrackId.ToString());

            return result;
        }

        public static async Task<IPaymentFetchResult> CreateFetchResult(HttpRequest httpRequest, InvoiceContext invoiceContext, MessagesOptions messagesOptions, CancellationToken cancellationToken)
        {
            var trackId = await httpRequest.TryGetParamAsync("trackId", cancellationToken).ConfigureAwaitFalse();
            var status = await httpRequest.TryGetParamAsAsync<int>("status", cancellationToken).ConfigureAwaitFalse();

            var trackIdInDatabase = GetTrackId(invoiceContext.Transactions);

            var isSucceed = status.Exists && status.Value == FetchSuccessCode &&
                            trackId.Exists && trackId.Value == trackIdInDatabase;

            string? message;

            if (isSucceed)
            {
                message = messagesOptions.PaymentSucceed;
            }
            else
            {
                message = ZibalTranslator.TranslateStatus(status.Value) ?? messagesOptions.PaymentFailed;
            }

            return new PaymentFetchResult
            {
                Status = isSucceed ? PaymentFetchResultStatus.ReadyForVerifying : PaymentFetchResultStatus.Failed,
                GatewayResponseCode = status.Value.ToString(),
                Message = message,
            };
        }

        public static ZibalPaymentVerifyModel CreateVerifyData(
            IEnumerable<Transaction> transactions,
            ZibalGatewayAccount account)
        {
            var trackId = GetTrackId(transactions);

            return new ZibalPaymentVerifyModel
            {
                Merchant = account.Merchant,
                TrackId = trackId
            };
        }

        public static async Task<PaymentVerifyResult> CreateVerifyResult(HttpResponseMessage responseMessage, MessagesOptions messagesOptions)
        {
            var message = await responseMessage.Content.ReadAsStringAsync();

            var response = JsonConvert.DeserializeObject<ZibalPaymentVerifyResponseModel>(message);

            if (response == null)
            {
                return PaymentVerifyResult.Failed(messagesOptions.InvalidDataReceivedFromGateway);
            }

            if (response.Result != SuccessCode)
            {
                var verifyResult = PaymentVerifyResult.Failed(response.Message);
                verifyResult.GatewayResponseCode = response.Result.ToString();

                if (response.Result == PaymentAlreadyVerifiedCode)
                {
                    verifyResult.Status = PaymentVerifyResultStatus.AlreadyVerified;
                }

                return verifyResult;
            }

            return PaymentVerifyResult.Succeed(response.RefNumber.ToString(), messagesOptions.PaymentSucceed);
        }

        private static string GetPaymentPageUrl(string paymentUrl, long trackId)
        {
            if (string.IsNullOrEmpty(paymentUrl)) throw new ArgumentNullException(nameof(paymentUrl), $"'{nameof(paymentUrl)}' cannot be null or empty.");

            paymentUrl = paymentUrl.ToggleStringAtEnd("/", false);

            return $"{paymentUrl}/{trackId}";
        }

        private static string GetTrackId(IEnumerable<Transaction> transactions)
        {
            var transactionRecord = transactions.SingleOrDefault(transaction => transaction.Type == TransactionType.Request);

            if (transactionRecord == null) throw new Exception("Cannot find any transaction with the type \"Request\"");

            var additionalData = JsonConvert.DeserializeObject<IDictionary<string, string>>(transactionRecord.AdditionalData);

            if (!additionalData.ContainsKey(TrackIdAdditionalDataKey))
            {
                throw new Exception("Cannot find the TrackId.");
            }

            return additionalData[TrackIdAdditionalDataKey];
        }
    }
}
