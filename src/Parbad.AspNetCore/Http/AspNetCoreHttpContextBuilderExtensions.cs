// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Http;

namespace Parbad.Builder
{
    public static class AspNetCoreHttpContextBuilderExtensions
    {
        /// <summary>
        /// Uses the default ASP.NET CORE <see cref="IHttpContextAccessor"/>. If it exists, no action is
        /// taken. If it does not exist then a default implementation for the <see cref="IHttpContextAccessor"/> is added.
        /// </summary>
        /// <param name="builder"></param>
        public static IHttpContextBuilder UseDefaultAspNetCore(this IHttpContextBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();

            return builder;
        }
    }
}
