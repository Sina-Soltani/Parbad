﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Storage.Abstractions.Models;
using Transaction = Parbad.Storage.Abstractions.Models.Transaction;

namespace Parbad.Gateway.Zibal.Internal
{
    internal static class ZibalHelper
    {
        public static string ZibalRequestAdditionalKeyName => "ZibalRequest";
        public static string TrackIdAdditionalDataKey => "TrackId";
        private const int SuccessCode = 100;

        public static ZibalRequestModel CreateRequestData(Invoice invoice, ZibalGatewayAccount account)
        {
            var request = invoice.GetZibalRequest();

            if (request == null) throw new Exception("ZibalRequest object not found. Make sure that you are using the UseZibal method.");

            return new ZibalRequestModel()
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
            MessagesOptions optionsMessages)
        {
            var message = await responseMessage.Content.ReadAsStringAsync();

            var response = JsonConvert.DeserializeObject<ZibalResponseModel>(message);

            if (response == null)
            {
                return PaymentRequestResult.Failed(optionsMessages.InvalidDataReceivedFromGateway, account.Name);
            }

            if (response.Result != SuccessCode)
            {
                var failureMessage = ZibalTranslator.TranslateResult(response.Result) ?? response.Message;
                return PaymentRequestResult.Failed(failureMessage, account.Name);
            }

            var paymentPageUrl = response.PayLink 
                                 ?? GetPaymentUrl(gatewayOptions.PaymentUrl, response.TrackId);

            var result = PaymentRequestResult.SucceedWithRedirect(account.Name, httpContext, paymentPageUrl);
            result.DatabaseAdditionalData.Add(TrackIdAdditionalDataKey, response.TrackId.ToString());

            return result;
        }

        private static string GetPaymentUrl(string paymentUrl, long trackId)
        {
            if (paymentUrl.EndsWith('/'))
                paymentUrl = paymentUrl.Substring(0, paymentUrl.Length - 1);

            return $"{paymentUrl}/{trackId}";
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

        public static async Task<PaymentVerifyResult> CreateVerifyResult(HttpResponseMessage responseMessage, MessagesOptions optionsMessages)
        {
            var message = await responseMessage.Content.ReadAsStringAsync();

            var response = JsonConvert.DeserializeObject<ZibalVerifyResponseModel>(message);
            if (response == null)
            {
                return PaymentVerifyResult.Failed(optionsMessages.InvalidDataReceivedFromGateway);
            }

            if (response.Result != SuccessCode)
            {
                var failureMessage = ZibalTranslator.TranslateResult(response.Result) ?? optionsMessages.PaymentFailed;

                if (response.Status != null)
                    failureMessage = ZibalTranslator.TranslateStatus((int)response.Status)
                                     ?? optionsMessages.PaymentFailed;

                return PaymentVerifyResult.Failed(failureMessage);
            }

            var successMessage = $"{optionsMessages.PaymentSucceed}-status is {response.Result}";
            return PaymentVerifyResult.Succeed(response.RefNumber.ToString(), successMessage);
        }
    }
}