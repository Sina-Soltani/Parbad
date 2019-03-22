// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Parbad.Http;

namespace Parbad.Internal
{
    public class HttpContextBuilder : IHttpContextBuilder
    {
        public HttpContextBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public void AddHttpContext<THttpContext>(ServiceLifetime serviceLifetime) where THttpContext : class, IHttpContextAccessor
        {
            Services.TryAdd(ServiceDescriptor.Describe(typeof(IHttpContextAccessor), typeof(THttpContext), serviceLifetime));
        }

        public void AddHttpContext(Func<IServiceProvider, IHttpContextAccessor> factory, ServiceLifetime serviceLifetime)
        {
            Services.TryAdd(ServiceDescriptor.Describe(typeof(IHttpContextAccessor), factory, serviceLifetime));
        }
    }
}
