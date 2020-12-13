// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Abstraction;
using Parbad.Exceptions;

namespace Parbad.Internal
{
    /// <exception cref="GatewayNotFoundException"></exception>
    /// <inheritdoc />
    public class DefaultGatewayProvider : IGatewayProvider
    {
        private readonly IServiceProvider _services;

        /// <summary>
        /// Initializes an instance of <see cref="DefaultGatewayProvider"/>.
        /// </summary>
        /// <param name="services"></param>
        public DefaultGatewayProvider(IServiceProvider services)
        {
            _services = services;
        }

        /// <inheritdoc />
        public virtual IGateway Provide(string gatewayName)
        {
            var descriptors = _services.GetServices<GatewayDescriptor>();

            var comparedDescriptors = descriptors
                .Where(descriptor => GatewayHelper.CompareName(descriptor.GatewayType, gatewayName))
                .ToList();

            if (comparedDescriptors.Count == 0) throw new GatewayNotFoundException(gatewayName);
            if (comparedDescriptors.Count > 1) throw new InvalidOperationException($"More than one gateway with the name {gatewayName} found.");

            var gateway = _services.GetService(comparedDescriptors[0].GatewayType);

            if (gateway == null) throw new GatewayNotFoundException(gatewayName);

            return (IGateway)gateway;
        }
    }
}
