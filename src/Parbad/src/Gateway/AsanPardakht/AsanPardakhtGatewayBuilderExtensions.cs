// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Parbad.Gateway.AsanPardakht.Internal;
using Parbad.GatewayBuilders;
using System;

namespace Parbad.Gateway.AsanPardakht
{
    public static class AsanPardakhtGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds Asan Pardakht gateway to Parbad services.
        /// </summary>
        public static IGatewayConfigurationBuilder<AsanPardakhtGateway> AddAsanPardakht(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSingleton<IAsanPardakhtCrypto, AsanPardakhtCrypto>();

            return builder
                .AddGateway<AsanPardakhtGateway>()
                .WithOptions(options => { })
                .WithHttpClient(clientBuilder => clientBuilder.ConfigureHttpClient(client => { }));
        }

        /// <summary>
        /// Configures the accounts for Asan Pardakht gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<AsanPardakhtGateway> WithAccounts(
            this IGatewayConfigurationBuilder<AsanPardakhtGateway> builder,
            Action<IGatewayAccountBuilder<AsanPardakhtGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }

        /// <summary>
        /// Configures the options for Asan Pardakht Gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions">Configuration</param>
        public static IGatewayConfigurationBuilder<AsanPardakhtGateway> WithOptions(
            this IGatewayConfigurationBuilder<AsanPardakhtGateway> builder,
            Action<AsanPardakhtGatewayOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);

            return builder;
        }
    }
}
