// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Parbad.Internal
{
    public class GatewayRedirect : IGatewayTransporter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _url;

        public GatewayRedirect(IHttpContextAccessor httpContextAccessor, string url)
        {
            _httpContextAccessor = httpContextAccessor;
            _url = url;
        }

        public Task TransportAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseUtilities.AddNecessaryContents(_httpContextAccessor.HttpContext);

            _httpContextAccessor.HttpContext.Response.Redirect(_url);

            return Task.CompletedTask;
        }
    }
}
