// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Parbad.Internal;

namespace Parbad.Http
{
    public static class HttpRequestExtensions
    {
        public static async Task<(bool Exists, StringValues Value)> TryGetParamAsync(this HttpRequest httpRequest, string key, CancellationToken cancellationToken)
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));
            if (key == null) throw new ArgumentNullException(nameof(key));

            var hasForm = (HttpMethods.IsPost(httpRequest.Method) ||
                           HttpMethods.IsPut(httpRequest.Method) ||
                           HttpMethods.IsDelete(httpRequest.Method) ||
                           HttpMethods.IsPatch(httpRequest.Method)) &&
                          httpRequest.HasFormContentType;

            var exists = httpRequest.Query.TryGetValue(key, out var value);

            if (!exists && hasForm)
            {
                var form = await httpRequest.ReadFormAsync(cancellationToken).ConfigureAwaitFalse();

                exists = form.TryGetValue(key, out value);
            }

            return (exists, value);
        }

        //public static bool TryGetParam(this HttpRequest httpRequest, string key, out StringValues value)
        //{
        //    if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));
        //    if (key == null) throw new ArgumentNullException(nameof(key));

        //    var hasForm = (HttpMethods.IsPost(httpRequest.Method) ||
        //                   HttpMethods.IsPut(httpRequest.Method) ||
        //                   HttpMethods.IsDelete(httpRequest.Method) ||
        //                   HttpMethods.IsPatch(httpRequest.Method)) &&
        //                  httpRequest.HasFormContentType;

        //    if (!hasForm) return httpRequest.Query.TryGetValue(key, out value);

        //    return
        //        httpRequest.Form.TryGetValue(key, out value) ||
        //        httpRequest.Query.TryGetValue(key, out value);
        //}

        public static async Task<(bool Exists, T Value)> TryGetParamAsAsync<T>(this HttpRequest httpRequest, string key, CancellationToken cancellationToken)
        {
            var result = await TryGetParamAsync(httpRequest, key, cancellationToken).ConfigureAwaitFalse();

            T value = default;

            if (result.Exists)
            {
                value = (T)Convert.ChangeType((string)result.Value, typeof(T));
            }

            return (result.Exists, value);
        }
    }
}
