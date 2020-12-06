// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.GatewayBuilders;

namespace Parbad.Gateway.ZarinPal
{
    public static class ZarinPalGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds the ZarinPal gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<ZarinPalGateway> AddZarinPal(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddGateway<ZarinPalGateway>();
        }

        /// <summary>
        /// Configures the accounts for <see cref="ZarinPalGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<ZarinPalGateway> WithAccounts(
            this IGatewayConfigurationBuilder<ZarinPalGateway> builder,
            Action<IGatewayAccountBuilder<ZarinPalGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }
    }
}
