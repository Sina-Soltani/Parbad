// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parbad.GatewayBuilders;
using Parbad.GatewayProviders.Parsian;
using Parbad.Options;

namespace Parbad.Builder
{
    public static class ParsianGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds Parsian gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<ParsianGateway> AddParsian(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<ParsianGateway>(new Uri(ParsianHelper.BaseServiceUrl));
        }

        /// <summary>
        /// Configures Parsian gateway options.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        public static IGatewayConfigurationBuilder<ParsianGateway> WithOptions(
            this IGatewayConfigurationBuilder<ParsianGateway> builder,
            Action<ParsianGatewayOptions> configureOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptions(configureOptions);
        }

        /// <summary>
        /// Adds the given <typeparamref name="TOptionsProvider"/> to services.
        /// It will be used for configuring the <see cref="ParsianGatewayOptions"/>.
        /// </summary>
        /// <typeparam name="TOptionsProvider"></typeparam>
        /// <param name="builder"></param>
        /// <param name="serviceLifetime">Lifetime of <typeparamref name="TOptionsProvider"/>.</param>
        public static IGatewayConfigurationBuilder<ParsianGateway> WithOptionsProvider<TOptionsProvider>(
            this IGatewayConfigurationBuilder<ParsianGateway> builder,
            ServiceLifetime serviceLifetime)
            where TOptionsProvider : class, IParbadOptionsProvider<ParsianGatewayOptions>
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptionsProvider<ParsianGateway, ParsianGatewayOptions, TOptionsProvider>(serviceLifetime);
        }

        /// <summary>
        /// Adds the given <paramref name="factory"/> to services.
        /// It will be used for configuring the <see cref="ParsianGatewayOptions"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="factory"></param>
        /// <param name="serviceLifetime">Lifetime of <paramref name="factory"/>.</param>
        public static IGatewayConfigurationBuilder<ParsianGateway> WithOptionsProvider(
            this IGatewayConfigurationBuilder<ParsianGateway> builder,
            Func<IServiceProvider, IParbadOptionsProvider<ParsianGatewayOptions>> factory,
            ServiceLifetime serviceLifetime)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptionsProvider<ParsianGateway, ParsianGatewayOptions>(factory, serviceLifetime);
        }

        /// <summary>
        /// Configures Parsian gateway by using an <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">The <see cref="IConfiguration"/> section.</param>
        public static IGatewayConfigurationBuilder<ParsianGateway> WithConfiguration(
            this IGatewayConfigurationBuilder<ParsianGateway> builder,
            IConfiguration configuration)
        {
            return builder.WithConfiguration<ParsianGateway, ParsianGatewayOptions>(configuration);
        }
    }
}
