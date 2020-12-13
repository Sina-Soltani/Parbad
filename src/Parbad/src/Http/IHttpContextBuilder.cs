// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Parbad.Http
{
    public interface IHttpContextBuilder
    {
        IServiceCollection Services { get; }

        void AddHttpContext<THttpContext>(ServiceLifetime serviceLifetime) where THttpContext : class, IHttpContextAccessor;

        void AddHttpContext(Func<IServiceProvider, IHttpContextAccessor> factory, ServiceLifetime serviceLifetime);
    }
}
