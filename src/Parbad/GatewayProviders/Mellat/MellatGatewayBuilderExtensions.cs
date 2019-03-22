// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parbad.GatewayBuilders;
using Parbad.GatewayProviders.Mellat;
using Parbad.Options;

namespace Parbad.Builder
{
    public static class MellatGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds Mellat gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<MellatGateway> AddMellat(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<MellatGateway>(new Uri(MellatHelper.BaseServiceUrl));
        }

        /// <summary>
        /// Configures Mellat gateway options.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        public static IGatewayConfigurationBuilder<MellatGateway> WithOptions(
            this IGatewayConfigurationBuilder<MellatGateway> builder,
            Action<MellatGatewayOptions> configureOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptions(configureOptions);
        }

        /// <summary>
        /// Adds the given <typeparamref name="TOptionsProvider"/> to services.
        /// It will be used for configuring the <see cref="MellatGatewayOptions"/>.
        /// </summary>
        /// <typeparam name="TOptionsProvider"></typeparam>
        /// <param name="builder"></param>
        /// <param name="serviceLifetime">Lifetime of <typeparamref name="TOptionsProvider"/>.</param>
        public static IGatewayConfigurationBuilder<MellatGateway> WithOptionsProvider<TOptionsProvider>(
            this IGatewayConfigurationBuilder<MellatGateway> builder,
            ServiceLifetime serviceLifetime)
            where TOptionsProvider : class, IParbadOptionsProvider<MellatGatewayOptions>
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptionsProvider<MellatGateway, MellatGatewayOptions, TOptionsProvider>(serviceLifetime);
        }

        /// <summary>
        /// Adds the given <paramref name="factory"/> to services.
        /// It will be used for configuring the <see cref="MellatGatewayOptions"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="factory"></param>
        /// <param name="serviceLifetime">Lifetime of <paramref name="factory"/>.</param>
        public static IGatewayConfigurationBuilder<MellatGateway> WithOptionsProvider(
            this IGatewayConfigurationBuilder<MellatGateway> builder,
            Func<IServiceProvider, IParbadOptionsProvider<MellatGatewayOptions>> factory,
            ServiceLifetime serviceLifetime)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptionsProvider<MellatGateway, MellatGatewayOptions>(factory, serviceLifetime);
        }

        /// <summary>
        /// Configures Mellat gateway by using an <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">The <see cref="IConfiguration"/> section.</param>
        public static IGatewayConfigurationBuilder<MellatGateway> WithConfiguration(
            this IGatewayConfigurationBuilder<MellatGateway> builder,
            IConfiguration configuration)
        {
            return builder.WithConfiguration<MellatGateway, MellatGatewayOptions>(configuration);
        }
    }
}
