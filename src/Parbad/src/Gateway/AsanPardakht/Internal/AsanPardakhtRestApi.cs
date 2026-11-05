// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parbad.Exceptions;
using Parbad.Internal;

namespace Parbad.Gateway.AsanPardakht.Internal
{
    /// <summary>
    /// The calls of AsanPardakht's REST API (https://ipgrest.asanpardakht.ir/v1/). Every call authenticates with the
    /// <c>usr</c> and <c>pwd</c> headers; nothing is encrypted by the merchant.
    /// </summary>
    internal sealed class AsanPardakhtRestApi
    {
        private readonly HttpClient _httpClient;
        private readonly AsanPardakhtGatewayOptions _options;

        public AsanPardakhtRestApi(HttpClient httpClient, AsanPardakhtGatewayOptions options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        /// <summary>POST Token: registers the payment and returns the RefId for the payment page.</summary>
        public Task<AsanPardakhtResponse> GetTokenAsync(AsanPardakhtGatewayAccount account, object request, CancellationToken cancellationToken) =>
            SendAsync(HttpMethod.Post, "Token", account, request, cancellationToken);

        /// <summary>GET TranResult: the result of the payment of an invoice, looked up by the merchant's own invoice id.</summary>
        public Task<AsanPardakhtResponse> GetTransactionResultAsync(AsanPardakhtGatewayAccount account,
                                                                    long merchantConfigurationId,
                                                                    long invoiceId,
                                                                    CancellationToken cancellationToken)
        {
            var query = $"TranResult?merchantConfigurationId={merchantConfigurationId.ToString(CultureInfo.InvariantCulture)}" +
                        $"&localInvoiceId={invoiceId.ToString(CultureInfo.InvariantCulture)}";

            return SendAsync(HttpMethod.Get, query, account, body: null, cancellationToken);
        }

        /// <summary>POST Verify.</summary>
        public Task<AsanPardakhtResponse> VerifyAsync(AsanPardakhtGatewayAccount account, long merchantConfigurationId, long payGateTranId, CancellationToken cancellationToken) =>
            SendAsync(HttpMethod.Post, "Verify", account, new { merchantConfigurationId, payGateTranId }, cancellationToken);

        /// <summary>POST Settlement.</summary>
        public Task<AsanPardakhtResponse> SettleAsync(AsanPardakhtGatewayAccount account, long merchantConfigurationId, long payGateTranId, CancellationToken cancellationToken) =>
            SendAsync(HttpMethod.Post, "Settlement", account, new { merchantConfigurationId, payGateTranId }, cancellationToken);

        private async Task<AsanPardakhtResponse> SendAsync(HttpMethod method,
                                                           string path,
                                                           AsanPardakhtGatewayAccount account,
                                                           object body,
                                                           CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(method, _options.ApiBaseUrl.TrimEnd('/') + "/" + path);

            request.Headers.Add("usr", account.UserName);
            request.Headers.Add("pwd", account.Password);

            if (body != null)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwaitFalse();

            var content = response.Content == null ? string.Empty : await response.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            // AsanPardakht answers with the HTTP status code itself. A 4xx is the bank's answer about this request.
            // A 5xx (e.g. 571 "not yet processed", 572 "status undetermined", or a proxy's error page) says nothing
            // about whether the bank processed the call, so the payment must stay open (G-H9).
            if ((int)response.StatusCode >= 500)
            {
                throw new GatewayResponseException(response.StatusCode, response.Content?.Headers.ContentType?.MediaType, "server error");
            }

            return new AsanPardakhtResponse(response.StatusCode, content);
        }
    }

    /// <summary>
    /// An answer of AsanPardakht's REST API: its HTTP status code, which is the result code, and its body.
    /// </summary>
    internal sealed class AsanPardakhtResponse
    {
        public AsanPardakhtResponse(HttpStatusCode statusCode, string body)
        {
            StatusCode = (int)statusCode;
            Body = body ?? string.Empty;
        }

        public int StatusCode { get; }

        public string Body { get; }

        public bool IsSucceed => StatusCode == (int)HttpStatusCode.OK;

        public string Code => StatusCode.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// The body as a string. It may be a JSON string (e.g. "\"ABC123\"") or plain text.
        /// </summary>
        public string ReadString()
        {
            var text = Body.Trim();

            if (text.StartsWith("\"", StringComparison.Ordinal))
            {
                try
                {
                    return JToken.Parse(text).Value<string>();
                }
                catch (JsonException)
                {
                }
            }

            return text;
        }

        /// <summary>The body as JSON.</summary>
        /// <exception cref="GatewayResponseException">The body is not valid JSON.</exception>
        public T ReadJson<T>() where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(Body) ??
                       throw new GatewayResponseException((HttpStatusCode)StatusCode, null, "empty body");
            }
            catch (JsonException)
            {
                throw new GatewayResponseException((HttpStatusCode)StatusCode, null, "not valid JSON");
            }
        }
    }
}
