// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using System;

namespace Parbad.Builder
{
    public static class ParbadBuilderExtensions
    {
        /// <summary>
        /// Adds Parbad pre-configured services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        public static IParbadBuilder AddParbad(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return ParbadBuilder.CreateDefaultBuilder(services);
        }
    }
}
