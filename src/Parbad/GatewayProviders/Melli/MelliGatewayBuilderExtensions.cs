// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parbad.GatewayBuilders;
using Parbad.GatewayProviders.Melli;
using Parbad.Options;

namespace Parbad.Builder
{
    public static class MelliGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds Melli gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<MelliGateway> AddMelli(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<MelliGateway>(new Uri(MelliHelper.BaseServiceUrl));
        }

        /// <summary>
        /// Configures Melli gateway options.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        public static IGatewayConfigurationBuilder<MelliGateway> WithOptions(
            this IGatewayConfigurationBuilder<MelliGateway> builder,
            Action<MelliGatewayOptions> configureOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptions(configureOptions);
        }

        /// <summary>
        /// Adds the given <typeparamref name="TOptionsProvider"/> to services.
        /// It will be used for configuring the <see cref="MelliGatewayOptions"/>.
        /// </summary>
        /// <typeparam name="TOptionsProvider"></typeparam>
        /// <param name="builder"></param>
        /// <param name="serviceLifetime">Lifetime of <typeparamref name="TOptionsProvider"/>.</param>
        public static IGatewayConfigurationBuilder<MelliGateway> WithOptionsProvider<TOptionsProvider>(
            this IGatewayConfigurationBuilder<MelliGateway> builder,
            ServiceLifetime serviceLifetime)
            where TOptionsProvider : class, IParbadOptionsProvider<MelliGatewayOptions>
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptionsProvider<MelliGateway, MelliGatewayOptions, TOptionsProvider>(serviceLifetime);
        }

        /// <summary>
        /// Adds the given <paramref name="factory"/> to services.
        /// It will be used for configuring the <see cref="MelliGatewayOptions"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="factory"></param>
        /// <param name="serviceLifetime">Lifetime of <paramref name="factory"/>.</param>
        public static IGatewayConfigurationBuilder<MelliGateway> WithOptionsProvider(
            this IGatewayConfigurationBuilder<MelliGateway> builder,
            Func<IServiceProvider, IParbadOptionsProvider<MelliGatewayOptions>> factory,
            ServiceLifetime serviceLifetime)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptionsProvider<MelliGateway, MelliGatewayOptions>(factory, serviceLifetime);
        }

        /// <summary>
        /// Configures Melli gateway by using an <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">The <see cref="IConfiguration"/> section.</param>
        public static IGatewayConfigurationBuilder<MelliGateway> WithConfiguration(
            this IGatewayConfigurationBuilder<MelliGateway> builder,
            IConfiguration configuration)
        {
            return builder.WithConfiguration<MelliGateway, MelliGatewayOptions>(configuration);
        }
    }
}
