// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.GatewayBuilders;
using Parbad.GatewayProviders.Mellat;
using Parbad.GatewayProviders.Mellat.Internal;

namespace Parbad.Builder
{
    public static class MellatGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds Mellat gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<MellatGateway> AddMellat(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<MellatGateway>()
                .WithHttpClient(clientBuilder => clientBuilder.ConfigureHttpClient(client =>
                    client.BaseAddress = new Uri(MellatHelper.BaseServiceUrl)));
        }

        /// <summary>
        /// Configures the accounts for <see cref="MellatGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<MellatGateway> WithAccounts(
            this IGatewayConfigurationBuilder<MellatGateway> builder,
            Action<IGatewayAccountBuilder<MellatGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }
    }
}
