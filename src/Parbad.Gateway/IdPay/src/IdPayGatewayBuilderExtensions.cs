// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Gateway.IdPay;
using Parbad.Gateway.IdPay.Internal;
using Parbad.GatewayBuilders;

namespace Parbad.Builder
{
    public static class IdPayGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds the IDPay.ir gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<IdPayGateway> AddIdPay(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<IdPayGateway>()
                .WithHttpClient(clientBuilder =>
                    clientBuilder.ConfigureHttpClient(client => client.BaseAddress = new Uri(IdPayHelper.ApiUrl)));
        }

        /// <summary>
        /// Configures the accounts for IDPay.ir.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<IdPayGateway> WithAccounts(
            this IGatewayConfigurationBuilder<IdPayGateway> builder,
            Action<IGatewayAccountBuilder<IdPayGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }
    }
}
