// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Gateway.Sepehr;
using Parbad.Gateway.Sepehr.Internal;
using Parbad.GatewayBuilders;

namespace Parbad.Builder
{
    public static class SepehrGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds the Sepehr gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<SepehrGateway> AddSepehr(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<SepehrGateway>()
                .WithHttpClient(clientBuilder =>
                    clientBuilder.ConfigureHttpClient(client => client.BaseAddress = new Uri(SepehrHelper.ApiBaseUrl)));
        }

        /// <summary>
        /// Configures the accounts for Sepehr gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<SepehrGateway> WithAccounts(
            this IGatewayConfigurationBuilder<SepehrGateway> builder,
            Action<IGatewayAccountBuilder<SepehrGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }
    }
}
