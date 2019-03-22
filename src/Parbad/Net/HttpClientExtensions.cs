// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Parbad.Net
{
    internal static class HttpClientExtensions
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

        public static Task<HttpResponseMessage> PostFormAsync(this HttpClient httpClient, string requestUri, IEnumerable<KeyValuePair<string, string>> data, CancellationToken cancellationToken = default)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

            return httpClient.PostAsync(requestUri, new FormUrlEncodedContent(data), cancellationToken);
        }
    }
}
