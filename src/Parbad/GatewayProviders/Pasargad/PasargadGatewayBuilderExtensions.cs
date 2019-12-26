// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.GatewayBuilders;
using Parbad.GatewayProviders.Pasargad;

namespace Parbad.Builder
{
    public static class PasargadGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds Pasargad gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<PasargadGateway> AddPasargad(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<PasargadGateway>()
                .WithHttpClient(clientBuilder => clientBuilder.ConfigureHttpClient(client =>
                    client.BaseAddress = new Uri(PasargadHelper.BaseServiceUrl)));
        }

        /// <summary>
        /// Configures the accounts for <see cref="PasargadGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<PasargadGateway> WithAccounts(
            this IGatewayConfigurationBuilder<PasargadGateway> builder,
            Action<IGatewayAccountBuilder<PasargadGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }
    }
}
