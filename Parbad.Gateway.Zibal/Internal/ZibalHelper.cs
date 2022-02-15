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
        public static string ZibalRequestAdditionalKeyName => "ZibalRequest";
        public static string TrackIdAdditionalDataKey => "TrackId";
        private const int SuccessCode = 100;

        public static ZibalRequestModel CreateRequestData(Invoice invoice, ZibalGatewayAccount account)
        {
            var request = invoice.GetZibalRequest();

            if (request == null) throw new Exception("ZibalRequest object not found. Make sure that you are using the UseYekPay method.");

            return new ZibalRequestModel()
            {
                MerchantId = account.Merchant,
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
                var failureMessage = response.Message;

                return PaymentRequestResult.Failed(failureMessage, account.Name);
            }

            var paymentPageUrl = response.PayLink;

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
                var failureMessage = response.Description ?? optionsMessages.PaymentFailed;

                return PaymentVerifyResult.Failed(failureMessage);
            }

            return PaymentVerifyResult.Succeed(response.RefNumber.ToString(), optionsMessages.PaymentSucceed);
        }
    }
}