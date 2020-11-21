// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Parbad.Gateway.ParbadVirtual.MiddlewareInvoker;

namespace Parbad.Owin.VirtualGateway
{
    public class ParbadVirtualGatewayMiddleware : OwinMiddleware
    {
        private readonly IServiceProvider _services;

        public ParbadVirtualGatewayMiddleware(OwinMiddleware next, IServiceProvider services)
        : base(next)
        {
            _services = services;
        }

        public override Task Invoke(IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var invoker = _services.GetRequiredService<IParbadVirtualGatewayMiddlewareInvoker>();

            return invoker.InvokeAsync();
        }
    }
}
