// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Abstraction;
using Parbad.Exceptions;

namespace Parbad.Internal
{
    /// <exception cref="GatewayNotFoundException"></exception>
    /// <inheritdoc />
    public class GatewayProvider : IGatewayProvider
    {
        private readonly IServiceProvider _services;

        public GatewayProvider(IServiceProvider services)
        {
            _services = services;
        }

        /// <inheritdoc />
        public virtual IGateway Provide(Type gatewayType)
        {
            GatewayHelper.IsGateway(gatewayType, throwException: true);

            var gateway = _services.GetService(gatewayType);

            if (gateway == null)
            {
                throw new GatewayNotFoundException(GatewayHelper.GetNameByType(gatewayType));
            }

            return (IGateway)gateway;
        }
    }
}
