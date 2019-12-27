// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Gateway.Saman;
using Parbad.GatewayBuilders;

namespace Parbad.Builder
{
    public static class SamanGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds Saman gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<SamanGateway> AddSaman(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<SamanGateway>()
                .WithHttpClient(clientBuilder => clientBuilder.ConfigureHttpClient(client =>
                    client.BaseAddress = new Uri(SamanHelper.BaseServiceUrl)));
        }

        /// <summary>
        /// Configures the accounts for <see cref="SamanGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<SamanGateway> WithAccounts(
            this IGatewayConfigurationBuilder<SamanGateway> builder,
            Action<IGatewayAccountBuilder<SamanGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }
    }
}
