// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parbad.GatewayBuilders;
using Parbad.GatewayProviders.IranKish;
using Parbad.Options;

namespace Parbad.Builder
{
    public static class IranKishGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds IranKish gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<IranKishGateway> AddIranKish(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<IranKishGateway>(new Uri(IranKishHelper.BaseServiceUrl));
        }

        /// <summary>
        /// Configures IranKish gateway options.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        public static IGatewayConfigurationBuilder<IranKishGateway> WithOptions(
            this IGatewayConfigurationBuilder<IranKishGateway> builder,
            Action<IranKishGatewayOptions> configureOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptions(configureOptions);
        }

        /// <summary>
        /// Adds the given <typeparamref name="TOptionsProvider"/> to services.
        /// It will be used for configuring the <see cref="IranKishGatewayOptions"/>.
        /// </summary>
        /// <typeparam name="TOptionsProvider"></typeparam>
        /// <param name="builder"></param>
        /// <param name="serviceLifetime">Lifetime of <typeparamref name="TOptionsProvider"/>.</param>
        public static IGatewayConfigurationBuilder<IranKishGateway> WithOptionsProvider<TOptionsProvider>(
            this IGatewayConfigurationBuilder<IranKishGateway> builder,
            ServiceLifetime serviceLifetime)
            where TOptionsProvider : class, IParbadOptionsProvider<IranKishGatewayOptions>
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptionsProvider<IranKishGateway, IranKishGatewayOptions, TOptionsProvider>(serviceLifetime);
        }

        /// <summary>
        /// Adds the given <paramref name="factory"/> to services.
        /// It will be used for configuring the <see cref="IranKishGatewayOptions"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="factory"></param>
        /// <param name="serviceLifetime">Lifetime of <paramref name="factory"/>.</param>
        public static IGatewayConfigurationBuilder<IranKishGateway> WithOptionsProvider(
            this IGatewayConfigurationBuilder<IranKishGateway> builder,
            Func<IServiceProvider, IParbadOptionsProvider<IranKishGatewayOptions>> factory,
            ServiceLifetime serviceLifetime)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithOptionsProvider<IranKishGateway, IranKishGatewayOptions>(factory, serviceLifetime);
        }

        /// <summary>
        /// Configures IranKish gateway by using an <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">The <see cref="IConfiguration"/> section.</param>
        public static IGatewayConfigurationBuilder<IranKishGateway> WithConfiguration(
            this IGatewayConfigurationBuilder<IranKishGateway> builder,
            IConfiguration configuration)
        {
            return builder.WithConfiguration<IranKishGateway, IranKishGatewayOptions>(configuration);
        }
    }
}
