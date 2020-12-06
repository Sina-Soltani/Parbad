// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.GatewayBuilders;

namespace Parbad.Gateway.IranKish
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

            return builder
                .AddGateway<IranKishGateway>()
                .WithHttpClient(clientBuilder => { })
                .WithOptions(options => { });
        }

        /// <summary>
        /// Configures the accounts for <see cref="IranKishGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<IranKishGateway> WithAccounts(
            this IGatewayConfigurationBuilder<IranKishGateway> builder,
            Action<IGatewayAccountBuilder<IranKishGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }

        /// <summary>
        /// Configures the options for IranKish Gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions">Configuration</param>
        public static IGatewayConfigurationBuilder<IranKishGateway> WithOptions(
            this IGatewayConfigurationBuilder<IranKishGateway> builder,
            Action<IranKishGatewayOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);

            return builder;
        }
    }
}
