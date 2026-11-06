// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Parbad.Http;

public static class HttpContextAccessorExtensions
{
    /// <summary>
    /// Gets the current HTTP request's context. Requesting, fetching and verifying a payment read and answer the
    /// current request (e.g. the bank's callback), so they cannot run outside one, such as in a background service.
    /// </summary>
    /// <exception cref="InvalidOperationException">There is no current HTTP request.</exception>
    public static HttpContext GetRequiredHttpContext(this IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor == null) throw new ArgumentNullException(nameof(httpContextAccessor));

        return httpContextAccessor.HttpContext ??
               throw new InvalidOperationException("Parbad needs the current HTTP request, e.g. the bank's callback request, but there is none. " +
                                                   "Request, fetch and verify a payment while handling an HTTP request.");
    }
}
