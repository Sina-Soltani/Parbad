// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Gateway.Parsian;
using Parbad.GatewayBuilders;

namespace Parbad.Builder
{
    public static class ParsianGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds Parsian gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<ParsianGateway> AddParsian(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<ParsianGateway>()
                .WithHttpClient(clientBuilder => clientBuilder.ConfigureHttpClient(client =>
                    client.BaseAddress = new Uri(ParsianHelper.BaseServiceUrl)));
        }

        /// <summary>
        /// Configures the accounts for <see cref="ParsianGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<ParsianGateway> WithAccounts(
            this IGatewayConfigurationBuilder<ParsianGateway> builder,
            Action<IGatewayAccountBuilder<ParsianGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }
    }
}
