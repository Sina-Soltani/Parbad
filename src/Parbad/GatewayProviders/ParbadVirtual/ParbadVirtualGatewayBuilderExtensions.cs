// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Parbad.GatewayBuilders;
using Parbad.GatewayProviders.ParbadVirtual;
using Parbad.GatewayProviders.ParbadVirtual.MiddlewareInvoker;
using Parbad.Internal;
using Parbad.Options;

namespace Parbad.Builder
{
    public static class ParbadVirtualGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds ParbadVirtual gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<ParbadVirtualGateway> AddParbadVirtual(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddTransient<ParbadVirtualGateway>();
            builder.Services.TryAddTransient<IParbadVirtualGatewayMiddlewareInvoker, ParbadVirtualGatewayMiddlewareInvoker>();

            return new GatewayConfigurationBuilder<ParbadVirtualGateway>(builder.Services);
        }

        /// <summary>
        /// Configures Parbad Virtual gateway options.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        public static IGatewayConfigurationBuilder<ParbadVirtualGateway> WithOptions(
            this IGatewayConfigurationBuilder<ParbadVirtualGateway> builder,
            Action<ParbadVirtualGatewayOptions> configureOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptions(configureOptions);
        }

        /// <summary>
        /// Adds the given <typeparamref name="TOptionsProvider"/> to services.
        /// It will be used for configuring the <see cref="ParbadVirtualGatewayOptions"/>.
        /// </summary>
        /// <typeparam name="TOptionsProvider"></typeparam>
        /// <param name="builder"></param>
        /// <param name="serviceLifetime">Lifetime of <typeparamref name="TOptionsProvider"/>.</param>
        public static IGatewayConfigurationBuilder<ParbadVirtualGateway> WithOptionsProvider<TOptionsProvider>(
            this IGatewayConfigurationBuilder<ParbadVirtualGateway> builder,
            ServiceLifetime serviceLifetime)
            where TOptionsProvider : class, IParbadOptionsProvider<ParbadVirtualGatewayOptions>
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptionsProvider<ParbadVirtualGateway, ParbadVirtualGatewayOptions, TOptionsProvider>(serviceLifetime);
        }

        /// <summary>
        /// Adds the given <paramref name="factory"/> to services.
        /// It will be used for configuring the <see cref="ParbadVirtualGatewayOptions"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="factory"></param>
        /// <param name="serviceLifetime">Lifetime of <paramref name="factory"/>.</param>
        public static IGatewayConfigurationBuilder<ParbadVirtualGateway> WithOptionsProvider(
            this IGatewayConfigurationBuilder<ParbadVirtualGateway> builder,
            Func<IServiceProvider, IParbadOptionsProvider<ParbadVirtualGatewayOptions>> factory,
            ServiceLifetime serviceLifetime)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptionsProvider<ParbadVirtualGateway, ParbadVirtualGatewayOptions>(factory, serviceLifetime);
        }

        /// <summary>
        /// Configures Parbad Virtual gateway by using an <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">The <see cref="IConfiguration"/> section.</param>
        public static IGatewayConfigurationBuilder<ParbadVirtualGateway> WithConfiguration(
            this IGatewayConfigurationBuilder<ParbadVirtualGateway> builder,
            IConfiguration configuration)
        {
            return builder.WithConfiguration<ParbadVirtualGateway, ParbadVirtualGatewayOptions>(configuration);
        }
    }
}
