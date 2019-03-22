// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Parbad.GatewayProviders.ParbadVirtual.MiddlewareInvoker
{
    public interface IParbadVirtualGatewayMiddlewareInvoker
    {
        Task Invoke(HttpContext httpContext);
        Task InvokeAsync();
        void Invoke2(HttpContext httpContext);
    }
}
