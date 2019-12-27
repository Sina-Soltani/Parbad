// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Gateway.AsanPardakht;
using Parbad.GatewayBuilders;

namespace Parbad.Builder
{
    public static class AsanPardakhtGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds AsanPardakht gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<AsanPardakhtGateway> AddAsanPardakht(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<AsanPardakhtGateway>()
                .WithHttpClient(clientBuilder => clientBuilder.ConfigureHttpClient(client =>
                    client.BaseAddress = new Uri(AsanPardakhtHelper.BaseServiceUrl)));
        }

        /// <summary>
        /// Configures the accounts for <see cref="AsanPardakhtGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<AsanPardakhtGateway> WithAccounts(
            this IGatewayConfigurationBuilder<AsanPardakhtGateway> builder,
            Action<IGatewayAccountBuilder<AsanPardakhtGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }
    }
}
