// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.Gateway.Zibal.Internal
{
    internal static class ZibalHelper
    {
        private const int SuccessCode = 100;

        public static string ZibalRequestAdditionalKeyName => "ZibalRequest";
        public static string TrackIdAdditionalDataKey => "TrackId";

        public static ZibalRequestModel CreateRequestData(Invoice invoice, ZibalGatewayAccount account)
        {
            var request = invoice.GetZibalRequest();

            if (request == null) throw new Exception("ZibalRequest object not found. Make sure that you are using the SetZibalData method when creating an invoice.");

            return new ZibalRequestModel
            {
                //if Merchant value is 'zibal' , Gateway mode is sandBox
                Merchant = account.IsSandBox ? "zibal" : account.Merchant,
                Amount = invoice.Amount,
                CustomerMobile = request.CustomerMobile,
                OrderId = invoice.TrackingNumber.ToString(),
                CallBackUrl = invoice.CallbackUrl,
                Description = request.Description,
                FeeMode = request.FeeMode,
                SendSms = request.SendSms,
                AllowedCards = request.AllowedCards
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

                return PaymentRequestResult.Failed(failureMessage, account.Name);
            }

            var paymentPageUrl = string.IsNullOrEmpty(response.PayLink)
                                ? GetPaymentPageUrl(gatewayOptions.PaymentUrl, response.TrackId)
                                : response.PayLink;

            var result = PaymentRequestResult.SucceedWithRedirect(account.Name, httpContext, paymentPageUrl);

            result.DatabaseAdditionalData.Add(TrackIdAdditionalDataKey, response.TrackId.ToString());

            return result;
        }

        public static ZibalVerifyRequestModel CreateVerifyData(
            IEnumerable<Transaction> transactions,
            ZibalGatewayAccount account)
        {
            var transactionRecord = transactions.SingleOrDefault(transaction => transaction.Type == TransactionType.Request);

            if (transactionRecord == null) throw new Exception("Cannot find any transaction with the type \"Request\"");

            var additionalData = JsonConvert.DeserializeObject<IDictionary<string, string>>(transactionRecord.AdditionalData);

            if (!additionalData.ContainsKey(TrackIdAdditionalDataKey))
            {
                throw new Exception("Cannot find the TrackId.");
            }

            return new ZibalVerifyRequestModel
            {
                Merchant = account.Merchant,
                TrackId = additionalData[TrackIdAdditionalDataKey]
            };
        }

        public static async Task<PaymentVerifyResult> CreateVerifyResult(HttpResponseMessage responseMessage, MessagesOptions messagesOptions)
        {
            var message = await responseMessage.Content.ReadAsStringAsync();

            var response = JsonConvert.DeserializeObject<ZibalVerifyResponseModel>(message);

            if (response == null)
            {
                return PaymentVerifyResult.Failed(messagesOptions.InvalidDataReceivedFromGateway);
            }

            if (response.Result != SuccessCode)
            {
                string failureMessage;

                if (response.Status == null)
                {
                    failureMessage = ZibalTranslator.TranslateResult(response.Result) ?? messagesOptions.PaymentFailed;
                }
                else
                {
                    failureMessage = ZibalTranslator.TranslateStatus((int)response.Status) ?? messagesOptions.PaymentFailed;
                }

                return PaymentVerifyResult.Failed(failureMessage);
            }

            return PaymentVerifyResult.Succeed(response.RefNumber.ToString(), messagesOptions.PaymentSucceed);
        }

        private static string GetPaymentPageUrl(string paymentUrl, long trackId)
        {
            paymentUrl = paymentUrl.ToggleStringAtEnd("/", false);

            return $"{paymentUrl}/{trackId}";
        }
    }
}
