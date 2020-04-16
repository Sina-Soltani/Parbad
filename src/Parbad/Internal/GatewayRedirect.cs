// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Parbad.Internal
{
    public class GatewayRedirect : IGatewayTransporter
    {
        private readonly HttpContext _httpContext;
        private readonly string _url;

        public GatewayRedirect(HttpContext httpContext, string url)
        {
            if (httpContext != null) _httpContext = httpContext;
            _url = url ?? throw new ArgumentNullException(nameof(url));
        }

        public Task TransportAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseUtilities.AddNecessaryContents(_httpContext);

            _httpContext.Response.Redirect(_url);

            return Task.CompletedTask;
        }
    }
}
