// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Parbad.Gateway.ParbadVirtual.MiddlewareInvoker;

namespace Parbad.AspNetCore.VirtualGateway;

public class ParbadVirtualGatewayMiddleware
{
    public ParbadVirtualGatewayMiddleware(RequestDelegate next)
    {
    }

    public Task Invoke(HttpContext httpContext, IParbadVirtualGatewayMiddlewareInvoker invoker)
    {
        return invoker.InvokeAsync();
    }
}
