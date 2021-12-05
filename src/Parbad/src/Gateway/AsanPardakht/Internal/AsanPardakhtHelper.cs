// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Parbad.Abstraction;
using Parbad.Gateway.AsanPardakht.Internal.Models;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.AsanPardakht.Internal
{
    internal static class AsanPardakhtHelper
    {
        private static JsonSerializerSettings JsonSettings => new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static async Task<(string Token, bool IsSucceed, AsanPardakhtApiErrorModel ErrorModel)> GetToken(
            HttpClient httpClient,
            Invoice invoice,
            AsanPardakhtGatewayAccount account,
            AsanPardakhtGatewayOptions gatewayOptions,
            CancellationToken cancellationToken)
        {
            AppendAuthenticationHeaders(httpClient, account);

            var serverDateTime = await httpClient.GetStringAsync(gatewayOptions.ApiServerTimeUrl);
            serverDateTime = serverDateTime.Replace("\"", "");

            var requestData = invoice.GetAsanPardakhtData();

            var hasSettlementPortions = requestData?.SettlementPortions != null && requestData.SettlementPortions.Count > 0;

            var data = new AsanPardakhtTokenModel
            {
                MerchantConfigurationId = account.MerchantConfigurationId,
                LocalInvoiceId = invoice.TrackingNumber,
                AmountInRials = invoice.Amount,
                CallbackURL = invoice.CallbackUrl,
                LocalDate = serverDateTime,
                ServiceTypeId = 1,
                AdditionalData = requestData?.AdditionalData,
                PaymentId = requestData?.PaymentId ?? "0",
                UseDefaultSharing = hasSettlementPortions ? true : null,
                SettlementPortions = hasSettlementPortions ? requestData.SettlementPortions : null
            };

            var responseMessage = await httpClient.PostJsonAsync(gatewayOptions.ApiGetTokenUrl, data, JsonSettings, cancellationToken);

            var response = await responseMessage.Content.ReadAsStringAsync();

            if (responseMessage.IsSuccessStatusCode)
            {
                return (response, true, null);
            }

            var errorModel = JsonConvert.DeserializeObject<AsanPardakhtApiErrorModel>(response);

            return (null, false, errorModel);
        }

        public static PaymentRequestResult CreateRequestResult(
            string token,
            bool isSucceed,
            AsanPardakhtApiErrorModel errorModel,
            HttpContext httpContext,
            Invoice invoice,
            AsanPardakhtGatewayAccount account,
            AsanPardakhtGatewayOptions gatewayOptions,
            MessagesOptions messagesOptions)
        {
            if (!isSucceed)
            {
                return PaymentRequestResult.Failed(errorModel.Title ?? messagesOptions.PaymentFailed, account.Name, errorModel.Status.ToString());
            }

            var formData = new Dictionary<string, string>
            {
                {"RefId", token}
            };

            var requestData = invoice.GetAsanPardakhtData();

            if (!string.IsNullOrWhiteSpace(requestData?.MobileNumber))
            {
                formData.Add("mobileap", requestData.MobileNumber);
            }

            return PaymentRequestResult.SucceedWithPost(
                account.Name,
                httpContext,
                gatewayOptions.PaymentPageUrl,
                formData);
        }

        public static async Task<(bool IsSucceed, AsanPardakhtPaymentResultModel TransModel, string FailedMessage)> GetTransResult(
            InvoiceContext context,
            HttpClient httpClient,
            AsanPardakhtGatewayAccount account,
            AsanPardakhtGatewayOptions gatewayOptions,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            var apiUrl = new CallbackUrl(gatewayOptions.ApiGetGetTransUrl);
            apiUrl.AddQueryString("LocalInvoiceId", context.Payment.TrackingNumber.ToString());
            apiUrl.AddQueryString("MerchantConfigurationId", account.MerchantConfigurationId.ToString());

            AppendAuthenticationHeaders(httpClient, account);

            var responseMessage = await httpClient.GetAsync(apiUrl, cancellationToken);

            var response = await responseMessage.Content.ReadAsStringAsync();

            if (!responseMessage.IsSuccessStatusCode)
            {
                var errorModel = JsonConvert.DeserializeObject<AsanPardakhtApiErrorModel>(response);

                var failedMessage = errorModel.Title ?? messagesOptions.PaymentFailed;

                return (false, null, failedMessage);
            }

            var transModel = JsonConvert.DeserializeObject<AsanPardakhtPaymentResultModel>(response);

            return (true, transModel, null);
        }

        /// <summary>
        /// This method is used for Verification, Settlement, Cancelling and Reversing a payment.
        /// </summary>
        public static async Task<(bool IsSucceed, string FailedMessage)> CompletionMethod(
            HttpClient httpClient,
            string apiUrl,
            long payGateTranId,
            AsanPardakhtGatewayAccount account,
            AsanPardakhtGatewayOptions gatewayOptions,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            AppendAuthenticationHeaders(httpClient, account);

            var data = new AsanPardakhtPaymentCompletionModel
            {
                MerchantConfigurationId = account.MerchantConfigurationId,
                PayGateTranId = payGateTranId
            };

            var responseMessage = await httpClient.PostJsonAsync(apiUrl, data, JsonSettings, cancellationToken);

            var response = await responseMessage.Content.ReadAsStringAsync();

            if (!responseMessage.IsSuccessStatusCode)
            {
                var errorModel = JsonConvert.DeserializeObject<AsanPardakhtApiErrorModel>(response);

                var failedMessage = errorModel.Title ?? messagesOptions.PaymentFailed;

                return (false, failedMessage);
            }

            return (true, null);
        }

        private static void AppendAuthenticationHeaders(HttpClient httpClient, AsanPardakhtGatewayAccount account)
        {
            httpClient.DefaultRequestHeaders.AddOrUpdate("usr", account.UserName);

            httpClient.DefaultRequestHeaders.AddOrUpdate("pwd", account.Password);
        }
    }
}
