// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Parbad.Internal
{
    internal static class HttpResponseUtilities
    {
        public static void AddNecessaryContents(HttpContext httpContext, string contentType = null)
        {
            var response = httpContext.Response;

            response.Headers["Cache-Control"] = "no-store, no-cache";
            response.Headers["Pragma"] = "no-cache";
            response.Headers["Expires"] = "Thu, 01 Jan 1970 00:00:00 GMT";
            response.Headers["Access-Control-Allow-Origin"] = "*";
            response.Headers["Access-Control-Allow-Methods"] = "*";
            response.Headers["Access-Control-Allow-Headers"] = "*";

            if (!contentType.IsNullOrEmpty())
            {
                response.ContentType = contentType;
            }
        }
    }
}
