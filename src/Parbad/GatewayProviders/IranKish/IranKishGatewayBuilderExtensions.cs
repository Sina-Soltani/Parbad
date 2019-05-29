// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.GatewayBuilders;
using Parbad.GatewayProviders.IranKish;

namespace Parbad.Builder
{
    public static class IranKishGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds IranKish gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<IranKishGateway> AddIranKish(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.AddGatewayAccountProvider<IranKishGatewayAccount>();

            return builder.AddGateway<IranKishGateway>(new Uri(IranKishHelper.BaseServiceUrl));
        }

        /// <summary>
        /// Configures the accounts for <see cref="IranKishGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<IranKishGateway> WithAccounts(
            this IGatewayConfigurationBuilder<IranKishGateway> builder,
            Action<IGatewayAccountBuilder<IranKishGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }
    }
}
