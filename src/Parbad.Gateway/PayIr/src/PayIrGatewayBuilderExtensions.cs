// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Gateway.PayIr;
using Parbad.Gateway.PayIr.Internal;
using Parbad.GatewayBuilders;

namespace Parbad.Builder
{
    public static class PayIrGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds the Pay.ir gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<PayIrGateway> AddPayIr(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<PayIrGateway>()
                .WithHttpClient(clientBuilder =>
                    clientBuilder.ConfigureHttpClient(client => client.BaseAddress = new Uri(PayIrHelper.WebServiceUrl)));
        }

        /// <summary>
        /// Configures the accounts for Pay.ir.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<PayIrGateway> WithAccounts(
            this IGatewayConfigurationBuilder<PayIrGateway> builder,
            Action<IGatewayAccountBuilder<PayIrGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }
    }
}
