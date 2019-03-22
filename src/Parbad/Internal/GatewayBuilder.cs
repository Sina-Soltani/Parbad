// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Abstraction;
using Parbad.GatewayBuilders;

namespace Parbad.Internal
{
    /// <inheritdoc />
    public class GatewayBuilder : IGatewayBuilder
    {
        /// <summary>
        /// Initializes an instance of <see cref="GatewayBuilder"/>.
        /// </summary>
        /// <param name="services"></param>
        public GatewayBuilder(IServiceCollection services)
        {
            Services = services;
        }

        /// <inheritdoc />
        public IServiceCollection Services { get; }

        /// <inheritdoc />
        public IGatewayConfigurationBuilder<TGateway> AddGateway<TGateway>(Uri baseServiceUrl,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where TGateway : class, IGateway
        {
            Services.TryAdd<TGateway>(serviceLifetime);

            return new GatewayConfigurationBuilder<TGateway>(Services)
                .WithHttpClient(builder => builder.ConfigureHttpClient(client => client.BaseAddress = baseServiceUrl));
        }
    }
}
