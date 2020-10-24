// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Gateway.Melli.Internal;
using Parbad.GatewayBuilders;

namespace Parbad.Gateway.Melli
{
    public static class MelliGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds Melli gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<MelliGateway> AddMelli(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<MelliGateway>()
                .WithHttpClient(clientBuilder => clientBuilder.ConfigureHttpClient(client =>
                    client.BaseAddress = new Uri(MelliHelper.BaseServiceUrl)));
        }

        /// <summary>
        /// Configures the accounts for <see cref="MelliGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<MelliGateway> WithAccounts(
            this IGatewayConfigurationBuilder<MelliGateway> builder,
            Action<IGatewayAccountBuilder<MelliGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }
    }
}
