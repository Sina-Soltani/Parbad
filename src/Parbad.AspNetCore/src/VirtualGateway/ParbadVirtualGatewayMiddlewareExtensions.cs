// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Parbad.AspNetCore.VirtualGateway;
using Parbad.Gateway.ParbadVirtual;

namespace Microsoft.AspNetCore.Builder;

public static class ParbadVirtualGatewayMiddlewareExtensions
{
    /// <summary>
    /// Adds the Parbad Virtual Gateway middleware to the pipeline if the current
    /// hosting environment name is <see cref="Hosting.EnvironmentName.Development"/>.
    /// </summary>
    /// <param name="builder"></param>
    public static IApplicationBuilder UseParbadVirtualGatewayWhenDeveloping(this IApplicationBuilder builder)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

#if NETCOREAPP3_0 || Net_5 || Net_6 || Net_7 || Net_8
            var hostEnvironment = builder.ApplicationServices.GetRequiredService<Hosting.IWebHostEnvironment>();
            var isDevelopment = Microsoft.Extensions.Hosting.HostEnvironmentEnvExtensions.IsDevelopment(hostEnvironment);
#else
        var hostEnvironment = builder.ApplicationServices.GetRequiredService<Hosting.IHostingEnvironment>();
        var isDevelopment = Hosting.HostingEnvironmentExtensions.IsDevelopment(hostEnvironment);
#endif
        return builder.UseParbadVirtualGatewayWhen(context => isDevelopment);
    }

    /// <summary>
    /// Adds the Parbad Virtual Gateway middleware to the pipeline.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="predicate"></param>
    public static IApplicationBuilder UseParbadVirtualGatewayWhen(this IApplicationBuilder builder,
                                                                  Func<HttpContext, bool> predicate)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        return builder.MapWhen(predicate,
                               applicationBuilder => applicationBuilder.UseParbadVirtualGateway());
    }

    /// <summary>
    /// Adds the Parbad Virtual Gateway middleware to the pipeline.
    /// </summary>
    /// <param name="builder"></param>
    public static IApplicationBuilder UseParbadVirtualGateway(this IApplicationBuilder builder)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        var options = builder
                     .ApplicationServices
                     .GetRequiredService<IOptions<ParbadVirtualGatewayOptions>>();

        if (options.Value == null ||
            options.Value.GatewayPath == null ||
            !options.Value.GatewayPath.HasValue)
        {
            throw new InvalidOperationException("Cannot get Parbad Virtual gateway path value. " +
                                                "Make sure that you have already configured it.");
        }

        return builder.Map(options.Value.GatewayPath,
                           applicationBuilder => applicationBuilder.UseMiddleware<ParbadVirtualGatewayMiddleware>());
    }
}