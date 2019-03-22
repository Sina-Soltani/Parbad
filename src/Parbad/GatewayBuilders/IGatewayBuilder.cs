// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Abstraction;

namespace Parbad.GatewayBuilders
{
    /// <summary>
    /// A builder for building a gateway.
    /// </summary>
    public interface IGatewayBuilder
    {
        /// <summary>
        /// Specifies the contract for a collection of service descriptors.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Adds the specified gateway to Parbad services.
        /// </summary>
        /// <typeparam name="TGateway">Type of gateway.</typeparam>
        /// <param name="baseServiceUrl">Base service address of <typeparamref name="TGateway"/>.</param>
        /// <param name="serviceLifetime">Lifetime of <typeparamref name="TGateway"/>.</param>
        IGatewayConfigurationBuilder<TGateway> AddGateway<TGateway>(
            Uri baseServiceUrl,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
            where TGateway : class, IGateway;
    }
}
