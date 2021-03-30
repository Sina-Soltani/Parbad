// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Parbad.Gateway.PayPing;
using Parbad.GatewayBuilders;

namespace Parbad.Gateway.PayPing
{
    public static class PayPingGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds the PayPing gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<PayPingGateway> AddPayPing(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            
            return builder
                .AddGateway<PayPingGateway>()
                .WithHttpClient(clientBuilder => { })
                .WithOptions(options => { });
        }

        /// <summary>
        /// Configures the accounts for PayPing gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<PayPingGateway> WithAccounts(
            this IGatewayConfigurationBuilder<PayPingGateway> builder,
            Action<IGatewayAccountBuilder<PayPingGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }

        /// <summary>
        /// Configures the options for PayPing Gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions">Configuration</param>
        public static IGatewayConfigurationBuilder<PayPingGateway> WithOptions(
            this IGatewayConfigurationBuilder<PayPingGateway> builder,
            Action<PayPingGatewayOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);

            return builder;
        }
    }
}
