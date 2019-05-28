// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Parbad.GatewayBuilders;
using Parbad.GatewayProviders.ParbadVirtual;
using Parbad.GatewayProviders.ParbadVirtual.MiddlewareInvoker;
using Parbad.Internal;

namespace Parbad.Builder
{
    public static class ParbadVirtualGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds the Parbad Virtual Gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<ParbadVirtualGateway> AddParbadVirtual(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddTransient<ParbadVirtualGateway>();
            builder.Services.TryAddTransient<IParbadVirtualGatewayMiddlewareInvoker, ParbadVirtualGatewayMiddlewareInvoker>();
            builder.AddGatewayAccountProvider<ParbadVirtualGatewayAccount>();

            var gatewayConfigurationBuilder = new GatewayConfigurationBuilder<ParbadVirtualGateway>(builder.Services);
            gatewayConfigurationBuilder
                .WithAccounts(accounts =>
                {
                    accounts
                        .AddInMemory(account => { });
                });

            return gatewayConfigurationBuilder;
        }

        /// <summary>
        /// Configures the accounts for <see cref="ParbadVirtualGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<ParbadVirtualGateway> WithAccounts(
            this IGatewayConfigurationBuilder<ParbadVirtualGateway> builder,
            Action<IGatewayAccountBuilder<ParbadVirtualGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }

        /// <summary>
        /// Configures Parbad Virtual gateway options.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        public static IGatewayConfigurationBuilder<ParbadVirtualGateway> WithOptions(
            this IGatewayConfigurationBuilder<ParbadVirtualGateway> builder,
            Action<ParbadVirtualGatewayOptions> configureOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.Configure(configureOptions);

            return builder;
        }
    }
}
