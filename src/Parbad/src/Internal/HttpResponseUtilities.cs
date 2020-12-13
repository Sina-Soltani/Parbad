// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Parbad.Internal
{
    internal static class HttpResponseUtilities
    {
        public static void AddNecessaryContents(HttpContext httpContext, string contentType = null)
        {
            var response = httpContext.Response;

            response.Headers[HeaderNames.CacheControl] = "no-store, no-cache";
            response.Headers[HeaderNames.Pragma] = "no-cache";
            response.Headers[HeaderNames.Expires] = "Thu, 01 Jan 1970 00:00:00 GMT";

            if (!contentType.IsNullOrEmpty())
            {
                response.ContentType = contentType;
            }
        }
    }
}
