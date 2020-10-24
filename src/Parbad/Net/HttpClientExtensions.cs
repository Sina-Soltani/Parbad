// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Net
{/// <summary>
/// 
/// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="requestUri"></param>
        /// <param name="xml"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> PostXmlAsync(this HttpClient httpClient, string requestUri, string xml, CancellationToken cancellationToken = default)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

            return httpClient.PostAsync(requestUri, new StringContent(xml, Encoding.UTF8, "text/xml"), cancellationToken);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="requestUri"></param>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static  Task<HttpResponseMessage> PostJsonAsync(this HttpClient httpClient, string requestUri, object data, CancellationToken cancellationToken = default)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            httpClient.DefaultRequestHeaders.Accept.Clear();

            var json = JsonConvert.SerializeObject(data, Formatting.None);

            return  httpClient.PostAsync(requestUri, new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="requestUri"></param>
        /// <param name="data"></param>
        /// <param name="serializerSettings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient httpClient, string requestUri, object data, JsonSerializerSettings serializerSettings, CancellationToken cancellationToken = default)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

            var json = JsonConvert.SerializeObject(data, serializerSettings);

            return httpClient.PostAsync(requestUri, new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="requestUri"></param>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> PostFormAsync(this HttpClient httpClient, string requestUri, IEnumerable<KeyValuePair<string, string>> data, CancellationToken cancellationToken = default)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

            return httpClient.PostAsync(requestUri, new FormUrlEncodedContent(data), cancellationToken);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
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
