// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Abstraction;
using Parbad.Exceptions;

namespace Parbad.Internal
{
    public static class GatewayProviderExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="gatewayProvider"></param>
        /// <param name="gatewayName"></param>
        /// <exception cref="GatewayNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static IGateway Provide(this IGatewayProvider gatewayProvider, string gatewayName)
        {
            if (gatewayProvider == null) throw new ArgumentNullException(nameof(gatewayProvider));

            return gatewayProvider.Provide(GatewayHelper.FindGatewayTypeByName(gatewayName, true));
        }
    }
}
