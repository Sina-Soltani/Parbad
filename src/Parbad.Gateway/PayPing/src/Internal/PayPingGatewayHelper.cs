// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parbad.Abstraction;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.Gateway.PayPing.Internal
{
    internal static class PayPingGatewayHelper
    {
        public const string PaymentCodeKey = "PaymentCode";

        /// <summary>"Already verified" in the metaData of an HTTP 409 answer to /v3/pay/verify.</summary>
        public const int AlreadyVerifiedCode = 110;

        private const string CallbackItemKey = "Parbad.PayPing.Callback";

        /// <summary>
        /// Reads PayPing's callback: a form POST with "status" (1 = paid), "errorCode" and "data", a JSON string with
        /// clientRefId, paymentCode, paymentRefId and amount. Some callbacks arrive as a JSON body instead of a form.
        /// </summary>
        public static async Task<PayPingCallbackResult> GetCallbackResult(
            HttpRequest request,
            InvoiceContext context,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            var callback = await ReadCallbackAsync(request, cancellationToken).ConfigureAwaitFalse();

            if (callback.Status != "1")
            {
                var errorCode = string.IsNullOrEmpty(callback.ErrorCode) ? null : $" Error code: {callback.ErrorCode}";

                return new PayPingCallbackResult
                {
                    IsSucceed = false,
                    Message = $"{messagesOptions.PaymentFailed}{errorCode}"
                };
            }

            var data = callback.Data;
            var message = "";
            long paymentRefId = 0;
            string paymentCode = null;

            if (data == null)
            {
                message += "Data isn't received.";
            }
            else
            {
                if (ToString(data["clientRefId"]) != context.Payment.TrackingNumber.ToString(CultureInfo.InvariantCulture))
                {
                    message += "ClientRefId isn't valid.";
                }

                // The payment code PayPing issued for this invoice: a payment of another invoice has another one.
                paymentCode = GetStoredPaymentCode(context);

                if (paymentCode == null || ToString(data["paymentCode"]) != paymentCode)
                {
                    message += "PaymentCode isn't valid.";
                }

                if (!long.TryParse(ToString(data["paymentRefId"]), NumberStyles.Integer, CultureInfo.InvariantCulture, out paymentRefId))
                {
                    message += "PaymentRefId isn't received.";
                }

                var amount = ToString(data["amount"]);

                if (amount != null &&
                    (!long.TryParse(amount, NumberStyles.Integer, CultureInfo.InvariantCulture, out var paidAmount) || paidAmount != (long)context.Payment.Amount))
                {
                    message += "Amount isn't valid.";
                }
            }

            var isSucceed = message.Length == 0;

            return new PayPingCallbackResult
            {
                IsSucceed = isSucceed,
                Message = isSucceed ? null : $"{messagesOptions.InvalidDataReceivedFromGateway}{message}",
                PaymentRefId = paymentRefId,
                PaymentCode = paymentCode
            };
        }

        public static PayPingVerifyRequestModel CreateVerificationModel(InvoiceContext context, PayPingCallbackResult callbackResult)
        {
            return new PayPingVerifyRequestModel
            {
                PaymentRefId = callbackResult.PaymentRefId,
                PaymentCode = callbackResult.PaymentCode,
                Amount = (long)context.Payment.Amount
            };
        }

        /// <summary>
        /// A successful or "already verified" answer. The core compares the reported amount and order with the invoice.
        /// </summary>
        public static PaymentVerifyResult CreateVerifyResult(VerifyPayResponseModel responseModel,
                                                             PayPingCallbackResult callbackResult,
                                                             MessagesOptions messagesOptions)
        {
            if (!string.IsNullOrEmpty(responseModel.Code) && responseModel.Code != callbackResult.PaymentCode)
            {
                return PaymentVerifyResult.Failed($"{messagesOptions.InvalidDataReceivedFromGateway}PaymentCode isn't valid.");
            }

            var paymentRefId = responseModel.PaymentRefId ?? callbackResult.PaymentRefId;

            var result = PaymentVerifyResult.Succeed(paymentRefId.ToString(CultureInfo.InvariantCulture), messagesOptions.PaymentSucceed);

            if (responseModel.Amount != null) result.BankReportedAmount = responseModel.Amount.Value;

            if (!string.IsNullOrEmpty(responseModel.ClientRefId)) result.BankReportedOrderId = responseModel.ClientRefId;

            return result;
        }

        /// <summary>
        /// Reads a ProblemDetails answer: { title, detail, metaData: { code, message, errors: [ { message } ] } }.
        /// </summary>
        /// <exception cref="Exceptions.GatewayResponseException">The body is not JSON.</exception>
        public static async Task<PayPingProblem> ReadProblemAsync(HttpResponseMessage responseMessage, MessagesOptions messagesOptions)
        {
            var body = await responseMessage.ReadJsonStringAsync().ConfigureAwaitFalse();

            var json = Parse(body) as JObject;
            var metaData = json?["metaData"] as JObject;

            var code = int.TryParse(ToString(metaData?["code"]), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
                ? number
                : (int?)null;

            var message = new[]
                          {
                              (metaData?["errors"] as JArray)?.OfType<JObject>().Select(error => ToString(error["message"])).FirstOrDefault(),
                              ToString(json?["detail"]),
                              ToString(json?["title"]),
                              ToString(json?["Error"] ?? json?["error"])
                          }
                          .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));

            return new PayPingProblem
            {
                Code = code,
                Message = message == null ? messagesOptions.PaymentFailed : $"{messagesOptions.PaymentFailed} {message}",
                Details = ToString(metaData?["message"])
            };
        }

        public static VerifyPayResponseModel TryParseVerifyResponse(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                return JsonConvert.DeserializeObject<VerifyPayResponseModel>(json);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string GetStoredPaymentCode(InvoiceContext context)
        {
            var transaction = context.Transactions.FirstOrDefault(item => item.Type == TransactionType.Request);

            if (string.IsNullOrEmpty(transaction?.AdditionalData)) return null;

            IDictionary<string, string> additionalData;

            try
            {
                additionalData = JsonConvert.DeserializeObject<IDictionary<string, string>>(transaction.AdditionalData);
            }
            catch (JsonException)
            {
                return null;
            }

            return additionalData?
                   .FirstOrDefault(pair => string.Equals(pair.Key, PaymentCodeKey, StringComparison.OrdinalIgnoreCase))
                   .Value;
        }

        /// <summary>
        /// Fetch and Verify both read the callback within the same request, so it is read once and kept with the request.
        /// </summary>
        private static async Task<PayPingCallback> ReadCallbackAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            if (request.HttpContext.Items.TryGetValue(CallbackItemKey, out var item) && item is PayPingCallback cached)
            {
                return cached;
            }

            PayPingCallback callback;

            if (request.ContentType?.IndexOf("json", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string body;

                using (var reader = new StreamReader(request.Body))
                {
                    body = await reader.ReadToEndAsync().ConfigureAwaitFalse();
                }

                var json = Parse(body) as JObject;

                callback = new PayPingCallback
                {
                    Status = ToString(json?["status"]),
                    ErrorCode = ToString(json?["errorCode"]),
                    Data = json?["data"] as JObject ?? Parse(ToString(json?["data"])) as JObject
                };
            }
            else
            {
                var status = await request.TryGetParamAsync("status", cancellationToken).ConfigureAwaitFalse();
                var errorCode = await request.TryGetParamAsync("errorCode", cancellationToken).ConfigureAwaitFalse();
                var data = await request.TryGetParamAsync("data", cancellationToken).ConfigureAwaitFalse();

                callback = new PayPingCallback
                {
                    Status = status.Exists ? (string)status.Value : null,
                    ErrorCode = errorCode.Exists ? (string)errorCode.Value : null,
                    Data = data.Exists ? Parse(data.Value) as JObject : null
                };
            }

            request.HttpContext.Items[CallbackItemKey] = callback;

            return callback;
        }

        /// <summary>Parses JSON, keeping dates as strings; returns null for anything that is not JSON.</summary>
        private static JToken Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                using var reader = new JsonTextReader(new StringReader(json)) { DateParseHandling = DateParseHandling.None };

                return JToken.ReadFrom(reader);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string ToString(JToken token) =>
            token == null || token.Type == JTokenType.Null ? null :
            token is JValue value ? Convert.ToString(value.Value, CultureInfo.InvariantCulture) :
            token.ToString(Formatting.None);

        private class PayPingCallback
        {
            public string Status { get; set; }

            public string ErrorCode { get; set; }

            public JObject Data { get; set; }
        }
    }

    internal class PayPingProblem
    {
        public int? Code { get; set; }

        public string Message { get; set; }

        /// <summary>metaData.message: for "already verified", the verified transaction as a JSON string.</summary>
        public string Details { get; set; }
    }
}
