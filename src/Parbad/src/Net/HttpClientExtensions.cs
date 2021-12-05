// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Parbad.Net
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PostXmlAsync(this HttpClient httpClient, string requestUri, string xml, CancellationToken cancellationToken = default)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

            return httpClient.PostAsync(requestUri, new StringContent(xml, Encoding.UTF8, "text/xml"), cancellationToken);
        }

        public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient httpClient, string requestUri, object data, CancellationToken cancellationToken = default)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

            var json = JsonConvert.SerializeObject(data, Formatting.None);

            return httpClient.PostAsync(requestUri, new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);
        }

        public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient httpClient, string requestUri, object data, JsonSerializerSettings serializerSettings, CancellationToken cancellationToken = default)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

            var json = JsonConvert.SerializeObject(data, serializerSettings);

            return httpClient.PostAsync(requestUri, new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);
        }

        public static Task<HttpResponseMessage> PostFormAsync(this HttpClient httpClient, string requestUri, IEnumerable<KeyValuePair<string, string>> data, CancellationToken cancellationToken = default)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

            return httpClient.PostAsync(requestUri, new FormUrlEncodedContent(data), cancellationToken);
        }

        public static async Task<TResult> PostJsonAsync<TResult>(this HttpClient httpClient, string requestUri, object data, CancellationToken cancellationToken = default)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

            var json = JsonConvert.SerializeObject(data, Formatting.None);

            var responseMessage = await httpClient.PostAsync(requestUri, new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);

            var response = await responseMessage.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TResult>(response);
        }

        public static async Task<TResult> GetJsonAsync<TResult>(this HttpClient httpClient, string requestUri)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

            var response = await httpClient.GetStringAsync(requestUri);

            return JsonConvert.DeserializeObject<TResult>(response);
        }

        public static void AddOrUpdate(this HttpRequestHeaders headers, string name, string value)
        {
            if (headers == null) throw new ArgumentNullException(nameof(headers));

            if (headers.Contains(name))
            {
                headers.Remove(name);
            }

            headers.Add(name, value);
        }
    }
}
