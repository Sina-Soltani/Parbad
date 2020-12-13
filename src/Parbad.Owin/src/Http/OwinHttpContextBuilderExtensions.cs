// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Owin;
using Parbad.Http;
using Parbad.Owin.Http;

namespace Parbad.Builder
{
    public static class OwinHttpContextBuilderExtensions
    {
        public static IHttpContextBuilder UseOwinFromCurrentHttpContext(this IHttpContextBuilder builder)
        {
            return builder.UseOwinFromHttpContext(provider => new HttpContextWrapper(HttpContext.Current));
        }

        public static IHttpContextBuilder UseOwinFromHttpContext(
            this IHttpContextBuilder builder,
            Func<IServiceProvider, HttpContextBase> httpContextFactory,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            return builder.UseOwin(provider => httpContextFactory(provider).GetOwinContext(), serviceLifetime);
        }

        public static IHttpContextBuilder UseOwinEnvironment(
            this IHttpContextBuilder builder,
            Func<IServiceProvider, IDictionary<string, object>> environmentFactory,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            return builder.UseOwin(provider => new OwinContext(environmentFactory(provider)), serviceLifetime);
        }

        public static IHttpContextBuilder UseOwin(
            this IHttpContextBuilder builder,
            Func<IServiceProvider, IOwinContext> owinContextFactory,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(IOwinContext), owinContextFactory, serviceLifetime));

            builder.AddHttpContext<OwinHttpContextAccessor>(serviceLifetime);

            return builder;
        }
    }
}
