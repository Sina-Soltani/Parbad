// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Parbad.Internal
{
    /// <inheritdoc />
    public class DefaultGatewayTransporter : IGatewayTransporter
    {
        private readonly HttpContext _httpContext;

        /// <summary>
        /// Initializes an instance of <see cref="DefaultGatewayTransporter"/>.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="descriptor"></param>
        public DefaultGatewayTransporter(HttpContext httpContext, GatewayTransporterDescriptor descriptor)
        {
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        }

        /// <inheritdoc />
        public GatewayTransporterDescriptor Descriptor { get; }

        /// <inheritdoc />
        public Task TransportAsync(CancellationToken cancellationToken = default)
        {
            if (Descriptor.Type == GatewayTransporterDescriptor.TransportType.Post)
            {
                HttpResponseUtilities.AddNecessaryContents(_httpContext, "text/html");

                var form = HtmlFormBuilder.CreateForm(Descriptor.Url, Descriptor.Form);

                return _httpContext.Response.WriteAsync(form, cancellationToken);
            }

            HttpResponseUtilities.AddNecessaryContents(_httpContext);

            _httpContext.Response.Redirect(Descriptor.Url);

            return Task.CompletedTask;
        }
    }
}
