// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Gateway.Tara;
using Parbad.Gateway.Tara.Internal.Models;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.Tara.Internal
{
    internal static class TaraHelper
    {
        public const string AuthorizationHeader = "Authorization";
        public const string SuccessResult = "0";

        #region Authentication

        public static TaraAuthenticateRequest CreateAuthenticateRequest(TaraGatewayAccount account)
        {
            return new TaraAuthenticateRequest
            {
                username = account.Username,
                password = account.Password
            };
        }

        public static async Task<string> AuthenticateAsync(
            HttpClient httpClient,
            TaraGatewayOptions options,
            TaraGatewayAccount account,
            CancellationToken cancellationToken = default)
        {
            var request = CreateAuthenticateRequest(account);

            // Use staging URL if account is in test mode
            var authUrl = TaraGatewayOptions.GetEnvironmentUrl(options.AuthenticationUrl, account.IsTest);

            TaraGatewayDiagnostics.LogRequest("Authentication", authUrl, JsonConvert.SerializeObject(request));

            // Clear any existing headers and set fresh ones
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Use Parbad's standard PostJsonAsync extension method
            var response = await httpClient.PostJsonAsync(authUrl, request, cancellationToken)
                .ConfigureAwaitFalse();

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            TaraGatewayDiagnostics.LogResponse("Authentication", (int)response.StatusCode, responseContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Authentication HTTP error {response.StatusCode}: {responseContent}");
            }

            var authResponse = JsonConvert.DeserializeObject<TaraAuthenticateResponse>(responseContent);

            if (authResponse == null)
            {
                throw new Exception($"Authentication failed: Unable to parse response: {responseContent}");
            }

            if (authResponse.result != SuccessResult)
            {
                throw new Exception($"Authentication failed (code: {authResponse.result}): {authResponse.description}");
            }

            if (string.IsNullOrEmpty(authResponse.accessToken))
            {
                throw new Exception("Authentication succeeded but no access token was returned");
            }

            return authResponse.accessToken;
        }

        #endregion

        #region Request

        public static TaraGetTokenRequest CreateGetTokenRequest(TaraGatewayAccount account, Invoice invoice)
        {
            var additionalData = invoice.GetTaraRequestAdditionalData();

            // If no additional data is provided, use default service amount with the invoice amount
            var serviceAmountList = additionalData?.ServiceAmountList?.Count > 0
                ? additionalData.ServiceAmountList
                : new List<TaraServiceAmount>
                {
                    new TaraServiceAmount { serviceId = 1, amount = (long)invoice.Amount }
                };

            var invoiceItemList = additionalData?.InvoiceItemList ?? new List<TaraInvoiceItem>();
            var mobile = additionalData?.Mobile ?? "";
            var additionalDataString = additionalData?.AdditionalData ?? "";
            var vat = additionalData?.Vat ?? 0;

            return new TaraGetTokenRequest
            {
                ip = account.Ip,
                serviceAmountList = serviceAmountList,
                taraInvoiceItemList = invoiceItemList,
                additionalData = additionalDataString,
                callBackUrl = invoice.CallbackUrl,
                amount = invoice.Amount.ToString(),
                mobile = mobile,
                orderId = invoice.TrackingNumber,
                vat = vat
            };
        }

        public static async Task<PaymentRequestResult> CreateRequestResult(
            HttpClient httpClient,
            HttpContext httpContext,
            TaraGatewayOptions options,
            TaraGatewayAccount account,
            Invoice invoice,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Step 1: Authenticate to get access token
                var accessToken = await AuthenticateAsync(httpClient, options, account, cancellationToken)
                    .ConfigureAwaitFalse();

                // Step 2: Get payment token
                var getTokenRequest = CreateGetTokenRequest(account, invoice);

                // Clear headers and set authorization
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Use staging URL if account is in test mode
                var getTokenUrl = TaraGatewayOptions.GetEnvironmentUrl(options.GetTokenUrl, account.IsTest);

                TaraGatewayDiagnostics.LogRequest("GetToken", getTokenUrl, JsonConvert.SerializeObject(getTokenRequest));

                var response = await httpClient.PostJsonAsync(getTokenUrl, getTokenRequest, cancellationToken)
                    .ConfigureAwaitFalse();

                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwaitFalse();

                TaraGatewayDiagnostics.LogResponse("GetToken", (int)response.StatusCode, responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    return PaymentRequestResult.Failed(responseContent, account.Name);
                }

                var tokenResponse = JsonConvert.DeserializeObject<TaraGetTokenResponse>(responseContent);

                if (tokenResponse.result != SuccessResult)
                {
                    var message = TaraGatewayResultTranslator.Translate(tokenResponse.result, messagesOptions);
                    return PaymentRequestResult.Failed(message, account.Name);
                }

                // Step 3: Create form data to redirect to payment page
                var form = new Dictionary<string, string>
                {
                    { "username", account.Username },
                    { "token", tokenResponse.token }
                };

                // Use staging payment URL if account is in test mode
                var paymentUrl = TaraGatewayOptions.GetEnvironmentUrl(options.PaymentUrl, account.IsTest);

                return PaymentRequestResult.SucceedWithPost(account.Name, httpContext, paymentUrl, form);
            }
            catch (Exception exception)
            {
                return PaymentRequestResult.Failed(exception.Message, account.Name);
            }
        }

        #endregion

        #region Callback

        public static async Task<TaraCallbackResult> CreateCallbackResultAsync(
            HttpRequest httpRequest,
            InvoiceContext context,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken = default)
        {
            var resultParam = await httpRequest.TryGetParamAsync("result", cancellationToken).ConfigureAwaitFalse();
            var descParam = await httpRequest.TryGetParamAsync("desc", cancellationToken).ConfigureAwaitFalse();
            var tokenParam = await httpRequest.TryGetParamAsync("token", cancellationToken).ConfigureAwaitFalse();
            var channelRefNumberParam = await httpRequest.TryGetParamAsync("channelRefNumber", cancellationToken).ConfigureAwaitFalse();
            var additionalDataParam = await httpRequest.TryGetParamAsync("additionalData", cancellationToken).ConfigureAwaitFalse();
            var orderIdParam = await httpRequest.TryGetParamAsync("orderId", cancellationToken).ConfigureAwaitFalse();

            var result = resultParam.Value.ToString();
            var desc = descParam.Value.ToString();
            var token = tokenParam.Value.ToString();
            var channelRefNumber = channelRefNumberParam.Value.ToString();
            var additionalData = additionalDataParam.Value.ToString();
            var orderId = orderIdParam.Value.ToString();

            var isSucceed = !result.IsNullOrEmpty() && result.Equals(SuccessResult, StringComparison.OrdinalIgnoreCase);

            var message = isSucceed 
                ? messagesOptions.PaymentSucceed 
                : TaraGatewayResultTranslator.Translate(result, messagesOptions);

            return new TaraCallbackResult
            {
                IsSucceed = isSucceed,
                Token = token,
                Result = result,
                Description = desc,
                ChannelRefNumber = channelRefNumber,
                AdditionalData = additionalData,
                OrderId = orderId.IsNullOrEmpty() ? 0 : long.Parse(orderId),
                Message = message
            };
        }

        #endregion

        #region Verify

        public static TaraVerifyRequest CreateVerifyRequest(string ip, string token)
        {
            return new TaraVerifyRequest
            {
                ip = ip,
                token = token
            };
        }

        public static async Task<PaymentVerifyResult> CreateVerifyResult(
            HttpClient httpClient,
            TaraGatewayOptions options,
            TaraGatewayAccount account,
            TaraCallbackResult callbackResult,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Authenticate to get access token
                var accessToken = await AuthenticateAsync(httpClient, options, account, cancellationToken)
                    .ConfigureAwaitFalse();

                // Create verify request
                var verifyRequest = CreateVerifyRequest(account.Ip, callbackResult.Token);

                // Clear headers and set authorization
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Use staging URL if account is in test mode
                var verifyUrl = TaraGatewayOptions.GetEnvironmentUrl(options.VerifyUrl, account.IsTest);

                TaraGatewayDiagnostics.LogRequest("Verify", verifyUrl, JsonConvert.SerializeObject(verifyRequest));

                var response = await httpClient.PostJsonAsync(verifyUrl, verifyRequest, cancellationToken)
                    .ConfigureAwaitFalse();

                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwaitFalse();

                TaraGatewayDiagnostics.LogResponse("Verify", (int)response.StatusCode, responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    return PaymentVerifyResult.Failed(responseContent);
                }

                var verifyResponse = JsonConvert.DeserializeObject<TaraVerifyResponse>(responseContent);

                var isSucceed = !verifyResponse.result.IsNullOrEmpty() &&
                               verifyResponse.result.Equals(SuccessResult, StringComparison.OrdinalIgnoreCase);

                var message = isSucceed 
                    ? messagesOptions.PaymentSucceed 
                    : TaraGatewayResultTranslator.Translate(verifyResponse.result, messagesOptions);

                var result = new PaymentVerifyResult
                {
                    Status = isSucceed ? PaymentVerifyResultStatus.Succeed : PaymentVerifyResultStatus.Failed,
                    TransactionCode = verifyResponse.rrn,
                    Message = message
                };

                result.DatabaseAdditionalData.Add("token", verifyResponse.token);
                result.DatabaseAdditionalData.Add("type", verifyResponse.type);

                return result;
            }
            catch (Exception exception)
            {
                return PaymentVerifyResult.Failed(exception.Message);
            }
        }

        #endregion
    }
}
