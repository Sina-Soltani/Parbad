// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Parbad.Http
{
    public static class HttpRequestExtensions
    {
        public static bool TryGetParam(this HttpRequest httpRequest, string key, out StringValues value)
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));
            if (key == null) throw new ArgumentNullException(nameof(key));

            var hasForm = (HttpMethods.IsPost(httpRequest.Method) ||
                           HttpMethods.IsPut(httpRequest.Method) ||
                           HttpMethods.IsDelete(httpRequest.Method)) &&
                          httpRequest.HasFormContentType;

            if (!hasForm) return httpRequest.Query.TryGetValue(key, out value);

            return
                httpRequest.Form.TryGetValue(key, out value) ||
                httpRequest.Query.TryGetValue(key, out value);
        }

        public static bool TryGetParamAs<T>(this HttpRequest httpRequest, string key, out T value)
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (httpRequest.TryGetParam(key, out var stringValue))
            {
                value = (T)Convert.ChangeType((string)stringValue, typeof(T));

                return true;
            }

            value = default;

            return false;
        }
    }
}
