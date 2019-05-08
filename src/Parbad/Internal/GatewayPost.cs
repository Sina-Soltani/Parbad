// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Parbad.Internal
{
    public class GatewayPost : IGatewayTransporter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _url;
        private readonly IDictionary<string, string> _formData;

        /// <inheritdoc />
        public GatewayPost(IHttpContextAccessor httpContextAccessor, string url, IDictionary<string, string> formData)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _url = url ?? throw new ArgumentNullException(nameof(url));
            _formData = formData ?? throw new ArgumentNullException(nameof(formData));
        }

        /// <inheritdoc />
        public Task TransportAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseUtilities.AddNecessaryContents(_httpContextAccessor.HttpContext, "text/html");

            var form = HtmlFormBuilder.CreateForm(_url, _formData);

            return _httpContextAccessor.HttpContext.Response.WriteAsync(form, cancellationToken);
        }
    }
}
