// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Gateway.AsanPardakht.Internal.Models;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using Parbad.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.AsanPardakht.Internal
{
    internal class AsanPardakhtHelper
    {
        private static async Task<(string Token, bool IsSucceed, AsanPardakhtApiErrorModel ErrorModel)> Token(HttpClient httpClient,
            Invoice invoice,
            AsanPardakhtGatewayAccount account,
            AsanPardakhtGatewayOptions gatewayOptions,
            CancellationToken cancellationToken)
        {
            var requestData = invoice.GetAsanPardakhtData();
            var hasSettlementPortions = requestData?.SettlementPortions != null && requestData.SettlementPortions.Count > 0;

            var time = (await httpClient.GetStringAsync(gatewayOptions.TimeUrl)).Replace("\"", "");

            var data = new AsanPardakhtRequestToken
            {
                MerchantConfigurationId = account.MerchantConfigurationId,
                LocalInvoiceId = invoice.TrackingNumber,
                AmountInRials = invoice.Amount,
                LocalDate = time,
                CallbackURL = $"{invoice.CallbackUrl}&invoiceid={invoice.TrackingNumber}",
                PaymentId = requestData?.PaymentId ?? "0",
                AdditionalData = requestData?.AdditionalData,
                SettlementPortions = hasSettlementPortions ? requestData.SettlementPortions : null
            };

            AppendAuthenticationHeaders(httpClient, account);

            var httpResponseMessage = await httpClient.PostJsonAsync(gatewayOptions.GetTokenUrl, data, cancellationToken);
            var response = await httpResponseMessage.Content.ReadAsStringAsync();
            if (httpResponseMessage.IsSuccessStatusCode)
                return (response.Replace("\"", ""), true, null);

            var error = JsonConvert.DeserializeObject<AsanPardakhtApiErrorModel>(response);
            return (null, false, error);
        }

        public static async Task<PaymentRequestResult> CreateRequestResult(
            HttpClient httpClient,
            Invoice invoice,
            AsanPardakhtGatewayAccount account,
            HttpContext httpContext,
            AsanPardakhtGatewayOptions gatewayOptions,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            var tokenResult = await Token(httpClient, invoice, account, gatewayOptions, cancellationToken);

            if (!tokenResult.IsSucceed)
            {
                var message = AsanPardakhtResultTranslator.TranslateRequest(tokenResult.ErrorModel.Status.ToString(), messagesOptions);

                return PaymentRequestResult.Failed(message, account.Name);
            }

            var requestData = invoice.GetAsanPardakhtData();
            var formData = new Dictionary<string, string>()
            {
                { "RefId", tokenResult.Token },
                { "mobileap", requestData?.MobileNumber }
            };

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
            try
            {
                var apiUrl = new CallbackUrl(gatewayOptions.GetTransUrl);
                apiUrl = apiUrl.AddQueryString("LocalInvoiceId", context.Payment.TrackingNumber.ToString());
                apiUrl = apiUrl.AddQueryString("MerchantConfigurationId", account.MerchantConfigurationId.ToString());

                // AppendAuthenticationHeaders(httpClient, account);
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
                request.Headers.Add("usr", account.UserName);
                request.Headers.Add("pwd", account.Password);

                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var res = await reader.ReadToEndAsync();
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        var errorModel = JsonConvert.DeserializeObject<AsanPardakhtApiErrorModel>(res);

                        var failedMessage = errorModel.Title ?? messagesOptions.PaymentFailed;

                        return (false, null, failedMessage);
                    }

                    var transModel = JsonConvert.DeserializeObject<AsanPardakhtPaymentResultModel>(res);
                    // return await reader.ReadToEndAsync();
                    return (true, transModel, null);
                }


                // var responseMessage = httpClient.GetAsync(apiUrl.ToString()).Result;

                // // var response = await responseMessage.Content.ReadAsStringAsync();

                // if (!responseMessage.IsSuccessStatusCode)
                // {
                //     var errorModel = JsonConvert.DeserializeObject<AsanPardakhtApiErrorModel>(response);

                //     var failedMessage = errorModel.Title ?? messagesOptions.PaymentFailed;

                //     return (false, null, failedMessage);
                // }

                // var transModel = JsonConvert.DeserializeObject<AsanPardakhtPaymentResultModel>(response);

            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
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

            var responseMessage = await httpClient.PostJsonAsync(apiUrl, data, cancellationToken);

            var response = await responseMessage.Content.ReadAsStringAsync();

            if (!responseMessage.IsSuccessStatusCode)
            {
                var errorModel = JsonConvert.DeserializeObject<AsanPardakhtApiErrorModel>(response);

                var failedMessage = errorModel.Title ?? messagesOptions.PaymentFailed;

                return (false, failedMessage);
            }

            return (true, null);
        }
        //public static async Task<string> CreateVerifyData(
        //    AsanPardakhtCallbackResult callbackResult,
        //    AsanPardakhtGatewayAccount account,
        //    IAsanPardakhtCrypto crypto)
        //{
        //    var requestToEncrypt = account.UserName + "," + account.Password;
        //    var encryptedRequest = await crypto.Encrypt(requestToEncrypt, account.Key, account.IV);

        //    return
        //        "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
        //        "<soapenv:Header/>" +
        //        "<soapenv:Body>" +
        //        "<tem:RequestVerification>" +
        //        $"<tem:merchantConfigurationID>{account.MerchantConfigurationId}</tem:merchantConfigurationID>" +
        //        "<!--Optional:-->" +
        //        $"<tem:encryptedCredentials>{XmlHelper.EncodeXmlValue(encryptedRequest)}</tem:encryptedCredentials>" +
        //        $"<tem:payGateTranID>{callbackResult.PayGateTranId}</tem:payGateTranID>" +
        //        "</tem:RequestVerification>" +
        //        "</soapenv:Body>" +
        //        "</soapenv:Envelope>";
        //}

        //public static AsanPardakhtVerifyResult CheckVerifyResult(
        //    string response,
        //    AsanPardakhtCallbackResult callbackResult,
        //    MessagesOptions messagesOptions)
        //{
        //    if (!callbackResult.IsSucceed)
        //    {
        //        return new AsanPardakhtVerifyResult
        //        {
        //            IsSucceed = false,
        //            Result = PaymentVerifyResult.Failed(callbackResult.Message)
        //        };
        //    }

        //    var result = XmlHelper.GetNodeValueFromXml(response, "RequestVerificationResult", "http://tempuri.org/");

        //    var isSucceed = result == "500";

        //    PaymentVerifyResult verifyResult = null;

        //    if (!isSucceed)
        //    {
        //        var message = AsanPardakhtResultTranslator.TranslateVerification(result, messagesOptions);

        //        verifyResult = PaymentVerifyResult.Failed(message);
        //        verifyResult.Message = message;
        //    }

        //    return new AsanPardakhtVerifyResult
        //    {
        //        IsSucceed = isSucceed,
        //        Result = verifyResult
        //    };
        //}

        //public static async Task<string> CreateSettleData(
        //    AsanPardakhtCallbackResult callbackResult,
        //    AsanPardakhtGatewayAccount account,
        //    IAsanPardakhtCrypto crypto)
        //{
        //    var requestToEncrypt = $"{account.UserName},{account.Password}";
        //    var encryptedRequest = await crypto.Encrypt(requestToEncrypt, account.Key, account.IV);

        //    return
        //        "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
        //        "<soapenv:Header/>" +
        //        "<soapenv:Body>" +
        //        "<tem:RequestReconciliation>" +
        //        $"<tem:merchantConfigurationID>{account.MerchantConfigurationId}</tem:merchantConfigurationID>" +
        //        "<!--Optional:-->" +
        //        $"<tem:encryptedCredentials>{XmlHelper.EncodeXmlValue(encryptedRequest)}</tem:encryptedCredentials>" +
        //        $"<tem:payGateTranID>{callbackResult.PayGateTranId}</tem:payGateTranID>" +
        //        "</tem:RequestReconciliation>" +
        //        "</soapenv:Body>" +
        //        "</soapenv:Envelope>";
        //}

        //public static PaymentVerifyResult CreateSettleResult(
        //    string response,
        //    AsanPardakhtCallbackResult callbackResult,
        //    MessagesOptions messagesOptions)
        //{
        //    var result = XmlHelper.GetNodeValueFromXml(response, "RequestReconciliationResult", "http://tempuri.org/");

        //    var isSucceed = result == "600";
        //    string message;

        //    if (isSucceed)
        //    {
        //        message = messagesOptions.PaymentSucceed;
        //    }
        //    else
        //    {
        //        message = AsanPardakhtResultTranslator.TranslateReconcilation(result, messagesOptions) ??
        //                  messagesOptions.PaymentFailed;
        //    }

        //    var verifyResult = new PaymentVerifyResult
        //    {
        //        Status = isSucceed ? PaymentVerifyResultStatus.Succeed : PaymentVerifyResultStatus.Failed,
        //        TransactionCode = callbackResult.Rrn,
        //        Message = message
        //    };

        //    verifyResult.DatabaseAdditionalData.Add("PayGateTranId", callbackResult.PayGateTranId);
        //    verifyResult.DatabaseAdditionalData.Add("LastFourDigitOfPAN", callbackResult.LastFourDigitOfPAN);

        //    return verifyResult;
        //}

        private static void AppendAuthenticationHeaders(HttpClient httpClient, AsanPardakhtGatewayAccount account)
        {
            httpClient.DefaultRequestHeaders.Clear();

            httpClient.DefaultRequestHeaders.AddOrUpdate("usr", account.UserName);
            httpClient.DefaultRequestHeaders.AddOrUpdate("pwd", account.Password);
        }

        private async Task<string> GetAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
