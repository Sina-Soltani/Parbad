// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parbad.GatewayBuilders;
using Parbad.GatewayProviders.AsanPardakht;
using Parbad.Options;

namespace Parbad.Builder
{
    public static class AsanPardakhtGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds AsanPardakht gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<AsanPardakhtGateway> AddAsanPardakht(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<AsanPardakhtGateway>(new Uri(AsanPardakhtHelper.BaseServiceUrl));
        }

        /// <summary>
        /// Configures AsanPardakht gateway options.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        public static IGatewayConfigurationBuilder<AsanPardakhtGateway> WithOptions(
            this IGatewayConfigurationBuilder<AsanPardakhtGateway> builder,
            Action<AsanPardakhtGatewayOptions> configureOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptions(configureOptions);
        }

        /// <summary>
        /// Adds the given <typeparamref name="TOptionsProvider"/> to services.
        /// It will be used for configuring the <see cref="AsanPardakhtGatewayOptions"/>.
        /// </summary>
        /// <typeparam name="TOptionsProvider"></typeparam>
        /// <param name="builder"></param>
        /// <param name="serviceLifetime">Lifetime of <typeparamref name="TOptionsProvider"/>.</param>
        public static IGatewayConfigurationBuilder<AsanPardakhtGateway> WithOptionsProvider<TOptionsProvider>(
            this IGatewayConfigurationBuilder<AsanPardakhtGateway> builder,
            ServiceLifetime serviceLifetime)
            where TOptionsProvider : class, IParbadOptionsProvider<AsanPardakhtGatewayOptions>
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptionsProvider<AsanPardakhtGateway, AsanPardakhtGatewayOptions, TOptionsProvider>(serviceLifetime);
        }

        /// <summary>
        /// Adds the given <paramref name="factory"/> to services.
        /// It will be used for configuring the <see cref="AsanPardakhtGatewayOptions"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="factory"></param>
        /// <param name="serviceLifetime">Lifetime of <paramref name="factory"/>.</param>
        public static IGatewayConfigurationBuilder<AsanPardakhtGateway> WithOptionsProvider(
            this IGatewayConfigurationBuilder<AsanPardakhtGateway> builder,
            Func<IServiceProvider, IParbadOptionsProvider<AsanPardakhtGatewayOptions>> factory,
            ServiceLifetime serviceLifetime)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptionsProvider<AsanPardakhtGateway, AsanPardakhtGatewayOptions>(factory, serviceLifetime);
        }

        /// <summary>
        /// Configures AsanPardakht gateway by using an <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">The <see cref="IConfiguration"/> section.</param>
        public static IGatewayConfigurationBuilder<AsanPardakhtGateway> WithConfiguration(
            this IGatewayConfigurationBuilder<AsanPardakhtGateway> builder,
            IConfiguration configuration)
        {
            return builder.WithConfiguration<AsanPardakhtGateway, AsanPardakhtGatewayOptions>(configuration);
        }
    }
}
