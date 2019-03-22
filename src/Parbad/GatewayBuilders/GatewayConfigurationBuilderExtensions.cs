// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Abstraction;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Options;

namespace Parbad.Builder
{
    public static class GatewayConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds the given <see cref="IParbadOptionsProvider{TGatewayOptions}"/> to <see cref="IServiceCollection"/>.
        /// It will be used for configuring the specified option.
        /// </summary>
        /// <typeparam name="TGateway"></typeparam>
        /// <typeparam name="TGatewayOptions">An option that should be configured to be used by the specified gateway.</typeparam>
        /// <typeparam name="TOptionsProvider">An <see cref="IParbadOptionsProvider{TOptions}"/> that provides and
        /// configures the <typeparamref name="TGatewayOptions"/></typeparam>
        /// <param name="builder"></param>
        /// <param name="serviceLifetime"></param>
        public static IGatewayConfigurationBuilder<TGateway> WithOptionsProvider<TGateway, TGatewayOptions, TOptionsProvider>(
            this IGatewayConfigurationBuilder<TGateway> builder,
            ServiceLifetime serviceLifetime)
            where TGateway : class, IGateway
            where TGatewayOptions : class, new()
            where TOptionsProvider : class, IParbadOptionsProvider<TGatewayOptions>
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            new ParbadOptionsBuilder(builder.Services)
                .AddOptionsUsingProvider<TGatewayOptions, TOptionsProvider>(serviceLifetime);

            AddDataAnnotationsValidator<TGatewayOptions>(builder.Services);

            return builder;
        }

        /// <summary>
        /// Adds the given <see cref="IParbadOptionsProvider{TGatewayOptions}"/> to <see cref="IServiceCollection"/>.
        /// It will be used for configuring the specified option.
        /// </summary>
        /// <typeparam name="TGateway"></typeparam>
        /// <typeparam name="TGatewayOptions">An option that should be configured to be used by the specified gateway.</typeparam>
        /// <param name="builder"></param>
        /// <param name="factory"></param>
        /// <param name="serviceLifetime"></param>
        public static IGatewayConfigurationBuilder<TGateway> WithOptionsProvider<TGateway, TGatewayOptions>(
            this IGatewayConfigurationBuilder<TGateway> builder,
            Func<IServiceProvider, IParbadOptionsProvider<TGatewayOptions>> factory,
            ServiceLifetime serviceLifetime)
            where TGateway : class, IGateway
            where TGatewayOptions : class, new()
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            new ParbadOptionsBuilder(builder.Services).AddOptionsUsingProvider(factory, serviceLifetime);

            AddDataAnnotationsValidator<TGatewayOptions>(builder.Services);

            return builder;
        }

        /// <summary>
        /// Configures the specified gateway by using an <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">The <see cref="IConfiguration"/> section.</param>
        internal static IGatewayConfigurationBuilder<TGateway> WithConfiguration<TGateway, TGatewayOptions>(
            this IGatewayConfigurationBuilder<TGateway> builder,
            IConfiguration configuration)
            where TGateway : class, IGateway
            where TGatewayOptions : class, new()
        {
            builder.Services.Configure<TGatewayOptions>(configuration);

            AddDataAnnotationsValidator<TGatewayOptions>(builder.Services);

            return builder;
        }

        internal static void AddDataAnnotationsValidator<TGatewayOptions>(IServiceCollection services)
            where TGatewayOptions : class, new()
        {
            new ParbadOptionsBuilder(services).AddDataAnnotationsValidator<TGatewayOptions>();
        }
    }
}
