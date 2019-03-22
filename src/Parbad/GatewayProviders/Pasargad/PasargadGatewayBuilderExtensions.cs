// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parbad.GatewayBuilders;
using Parbad.GatewayProviders.Pasargad;
using Parbad.Options;

namespace Parbad.Builder
{
    public static class PasargadGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds Pasargad gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<PasargadGateway> AddPasargad(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<PasargadGateway>(new Uri(PasargadHelper.BaseServiceUrl));
        }

        /// <summary>
        /// Configures Pasargad gateway options.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        public static IGatewayConfigurationBuilder<PasargadGateway> WithOptions(
            this IGatewayConfigurationBuilder<PasargadGateway> builder,
            Action<PasargadGatewayOptions> configureOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptions(configureOptions);
        }

        /// <summary>
        /// Adds the given <typeparamref name="TOptionsProvider"/> to services.
        /// It will be used for configuring the <see cref="PasargadGatewayOptions"/>.
        /// </summary>
        /// <typeparam name="TOptionsProvider"></typeparam>
        /// <param name="builder"></param>
        /// <param name="serviceLifetime">Lifetime of <typeparamref name="TOptionsProvider"/>.</param>
        public static IGatewayConfigurationBuilder<PasargadGateway> WithOptionsProvider<TOptionsProvider>(
            this IGatewayConfigurationBuilder<PasargadGateway> builder,
            ServiceLifetime serviceLifetime)
            where TOptionsProvider : class, IParbadOptionsProvider<PasargadGatewayOptions>
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptionsProvider<PasargadGateway, PasargadGatewayOptions, TOptionsProvider>(serviceLifetime);
        }

        /// <summary>
        /// Adds the given <paramref name="factory"/> to services.
        /// It will be used for configuring the <see cref="PasargadGatewayOptions"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="factory"></param>
        /// <param name="serviceLifetime">Lifetime of <paramref name="factory"/>.</param>
        public static IGatewayConfigurationBuilder<PasargadGateway> WithOptionsProvider(
            this IGatewayConfigurationBuilder<PasargadGateway> builder,
            Func<IServiceProvider, IParbadOptionsProvider<PasargadGatewayOptions>> factory,
            ServiceLifetime serviceLifetime)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptionsProvider<PasargadGateway, PasargadGatewayOptions>(factory, serviceLifetime);
        }

        /// <summary>
        /// Configures Pasargad gateway by using an <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">The <see cref="IConfiguration"/> section.</param>
        public static IGatewayConfigurationBuilder<PasargadGateway> WithConfiguration(
            this IGatewayConfigurationBuilder<PasargadGateway> builder,
            IConfiguration configuration)
        {
            return builder.WithConfiguration<PasargadGateway, PasargadGatewayOptions>(configuration);
        }
    }
}
