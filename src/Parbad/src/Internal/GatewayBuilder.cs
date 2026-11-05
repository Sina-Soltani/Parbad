// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Parbad.Abstraction;
using Parbad.GatewayBuilders;
using Parbad.Net;

namespace Parbad.Internal;

/// <inheritdoc />
internal class GatewayBuilder : IGatewayBuilder
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
    public IGatewayConfigurationBuilder<TGateway> AddGateway<TGateway>(
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TGateway : class, IGateway
    {
        Services.AddSingleton(new GatewayDescriptor(typeof(TGateway)));

        Services.TryAdd<TGateway>(serviceLifetime);

        // Registered here, so every gateway's client gets Parbad's defaults (e.g. the timeout), also without WithHttpClient.
        Services.AddHttpClientForGateway<TGateway>();

        return new GatewayConfigurationBuilder<TGateway>(Services);
    }
}
