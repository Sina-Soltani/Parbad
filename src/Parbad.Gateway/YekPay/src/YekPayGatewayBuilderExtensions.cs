// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Gateway.YekPay.Internal;
using Parbad.GatewayBuilders;

namespace Parbad.Gateway.YekPay
{
    public static class YekPayGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds the YekPay gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<YekPayGateway> AddYekPay(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<YekPayGateway>()
                .WithHttpClient(clientBuilder =>
                    clientBuilder.ConfigureHttpClient(client => client.BaseAddress = new Uri(YekPayHelper.ApiBaseUrl)));
        }

        /// <summary>
        /// Configures the accounts for YekPay gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<YekPayGateway> WithAccounts(
            this IGatewayConfigurationBuilder<YekPayGateway> builder,
            Action<IGatewayAccountBuilder<YekPayGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }
    }
}
