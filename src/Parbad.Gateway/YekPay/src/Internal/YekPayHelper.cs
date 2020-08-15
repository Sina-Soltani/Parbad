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
using Parbad.Storage.Abstractions;

namespace Parbad.Gateway.YekPay.Internal
{
    internal static class YekPayHelper
    {
        private const string AuthorityAdditionalDataKey = "Authority";
        private const int SuccessCode = 100;

        public const string ApiBaseUrl = "https://gate.yekpay.com/api/";
        public const string ApiRequestUrl = "payment/request";
        public const string ApiVerifyUrl = "payment/verify";
        public static string PaymentPageUrl => $"{ApiBaseUrl}payment/start/";

        public static YekPayRequestModel CreateRequestData(Invoice invoice, YekPayGatewayAccount account)
        {
            var yekPayRequest = invoice.GetYekPayRequest();

            if (yekPayRequest == null) throw new Exception("YekPayRequest object not found. Make sure that you are using the UseYekPay method.");

            return new YekPayRequestModel
            {
                MerchantId = account.MerchantId,
                Amount = invoice.Amount,
                FromCurrencyCode = yekPayRequest.FromCurrencyCode,
                ToCurrencyCode = yekPayRequest.ToCurrencyCode,
                OrderNumber = invoice.TrackingNumber,
                Callback = invoice.CallbackUrl,
                FirstName = yekPayRequest.FirstName,
                LastName = yekPayRequest.LastName,
                Email = yekPayRequest.Email,
                Mobile = yekPayRequest.Mobile,
                Address = yekPayRequest.Address,
                PostalCode = yekPayRequest.PostalCode,
                Country = yekPayRequest.Country,
                City = yekPayRequest.City,
                Description = yekPayRequest.Description
            };
        }

        public static async Task<PaymentRequestResult> CreateRequestResult(HttpResponseMessage responseMessage, HttpContext httpContext, YekPayGatewayAccount account, MessagesOptions optionsMessages)
        {
            var message = await responseMessage.Content.ReadAsStringAsync();

            var response = JsonConvert.DeserializeObject<YekPayResponseModel>(message);

            if (response == null)
            {
                return PaymentRequestResult.Failed(optionsMessages.InvalidDataReceivedFromGateway, account.Name);
            }

            if (response.Code != SuccessCode)
            {
                var failureMessage = response.Description ?? optionsMessages.UnexpectedErrorText;

                return PaymentRequestResult.Failed(failureMessage, account.Name);
            }

            var paymentPageUrl = PaymentPageUrl + response.Authority;
            var descriptor = GatewayTransporterDescriptor.CreateRedirect(paymentPageUrl);
            var transporter = new DefaultGatewayTransporter(httpContext, descriptor);

            var result = PaymentRequestResult.Succeed(transporter, account.Name);
            result.DatabaseAdditionalData.Add(AuthorityAdditionalDataKey, response.Authority);

            return result;
        }

        public static YekPayVerificationRequestModel CreateVerifyData(
            IEnumerable<Transaction> transactions,
            YekPayGatewayAccount account)
        {
            var transactionRecord = transactions.SingleOrDefault(transaction => transaction.Type == TransactionType.Request);

            if (transactionRecord == null) throw new Exception("Cannot find any transaction with the type \"Request\"");

            var additionalData = JsonConvert.DeserializeObject<IDictionary<string, string>>(transactionRecord.AdditionalData);

            if (!additionalData.ContainsKey(AuthorityAdditionalDataKey))
            {
                throw new Exception("Cannot find the Authority.");
            }

            return new YekPayVerificationRequestModel
            {
                MerchantId = account.MerchantId,
                Authority = additionalData[AuthorityAdditionalDataKey]
            };
        }

        public static async Task<PaymentVerifyResult> CreateVerifyResult(HttpResponseMessage responseMessage, MessagesOptions optionsMessages)
        {
            var message = await responseMessage.Content.ReadAsStringAsync();

            var response = JsonConvert.DeserializeObject<YekPayVerificationResponseModel>(message);

            if (response == null)
            {
                return PaymentVerifyResult.Failed(optionsMessages.InvalidDataReceivedFromGateway);
            }

            if (response.Code != SuccessCode)
            {
                var failureMessage = response.Description ?? optionsMessages.PaymentFailed;

                return PaymentVerifyResult.Failed(failureMessage);
            }

            return PaymentVerifyResult.Succeed(response.Reference, optionsMessages.PaymentSucceed);
        }
    }
}
