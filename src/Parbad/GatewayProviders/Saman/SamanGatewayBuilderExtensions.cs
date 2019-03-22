// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parbad.GatewayBuilders;
using Parbad.GatewayProviders.Saman;
using Parbad.Options;

namespace Parbad.Builder
{
    public static class SamanGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds Saman gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<SamanGateway> AddSaman(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<SamanGateway>(new Uri(SamanHelper.BaseServiceUrl));
        }

        /// <summary>
        /// Configures Saman gateway options.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        public static IGatewayConfigurationBuilder<SamanGateway> WithOptions(
            this IGatewayConfigurationBuilder<SamanGateway> builder,
            Action<SamanGatewayOptions> configureOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptions(configureOptions);
        }

        /// <summary>
        /// Adds the given <typeparamref name="TOptionsProvider"/> to services.
        /// It will be used for configuring the <see cref="SamanGatewayOptions"/>.
        /// </summary>
        /// <typeparam name="TOptionsProvider"></typeparam>
        /// <param name="builder"></param>
        /// <param name="serviceLifetime">Lifetime of <typeparamref name="TOptionsProvider"/>.</param>
        public static IGatewayConfigurationBuilder<SamanGateway> WithOptionsProvider<TOptionsProvider>(
            this IGatewayConfigurationBuilder<SamanGateway> builder,
            ServiceLifetime serviceLifetime)
            where TOptionsProvider : class, IParbadOptionsProvider<SamanGatewayOptions>
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptionsProvider<SamanGateway, SamanGatewayOptions, TOptionsProvider>(serviceLifetime);
        }

        /// <summary>
        /// Adds the given <paramref name="factory"/> to services.
        /// It will be used for configuring the <see cref="SamanGatewayOptions"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="factory"></param>
        /// <param name="serviceLifetime">Lifetime of <paramref name="factory"/>.</param>
        public static IGatewayConfigurationBuilder<SamanGateway> WithOptionsProvider(
            this IGatewayConfigurationBuilder<SamanGateway> builder,
            Func<IServiceProvider, IParbadOptionsProvider<SamanGatewayOptions>> factory,
            ServiceLifetime serviceLifetime)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptionsProvider<SamanGateway, SamanGatewayOptions>(factory, serviceLifetime);
        }

        /// <summary>
        /// Configures Saman gateway by using an <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">The <see cref="IConfiguration"/> section.</param>
        public static IGatewayConfigurationBuilder<SamanGateway> WithConfiguration(
            this IGatewayConfigurationBuilder<SamanGateway> builder,
            IConfiguration configuration)
        {
            return builder.WithConfiguration<SamanGateway, SamanGatewayOptions>(configuration);
        }
    }
}
