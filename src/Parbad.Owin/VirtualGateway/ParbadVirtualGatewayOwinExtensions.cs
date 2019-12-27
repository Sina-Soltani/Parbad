// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Owin;
using Parbad.Gateway.ParbadVirtual;
using Parbad.Owin.VirtualGateway;

namespace Parbad.Builder
{
    public static class ParbadVirtualGatewayOwinExtensions
    {
        /// <summary>
        /// Adds the Parbad Virtual Gateway middleware to the pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IAppBuilder UseParbadVirtualGateway(this IAppBuilder app, IServiceProvider serviceProvider)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            var options = serviceProvider.GetRequiredService<IOptions<ParbadVirtualGatewayOptions>>();

            return app.Map(options.Value.GatewayPath,
                builder => builder.Use<ParbadVirtualGatewayMiddleware>(serviceProvider));
        }
    }
}
