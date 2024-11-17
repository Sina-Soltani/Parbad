// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Parbad.Abstraction;
using Parbad.GatewayBuilders;
using Parbad.Net;

namespace Parbad.Internal;

/// <inheritdoc />
internal class GatewayConfigurationBuilder<TGateway> : IGatewayConfigurationBuilder<TGateway>
    where TGateway : class, IGateway
{
    public GatewayConfigurationBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <inheritdoc />
    public IServiceCollection Services { get; }

    /// <inheritdoc />
    public IGatewayConfigurationBuilder<TGateway> WithAccounts<TAccount>(Action<IGatewayAccountBuilder<TAccount>> configureAccounts)
        where TAccount : GatewayAccount, new()
    {
        if (configureAccounts == null) throw new ArgumentNullException(nameof(configureAccounts));

        configureAccounts(new GatewayAccountBuilder<TAccount>(Services));

        Services
           .TryAddTransient<
                IGatewayAccountProvider<TAccount>,
                GatewayAccountProvider<TAccount>>();

        return this;
    }

    /// <inheritdoc />
    public IGatewayConfigurationBuilder<TGateway> WithHttpClient(Action<IHttpClientBuilder> configureHttpClient)
    {
        if (configureHttpClient == null) throw new ArgumentNullException(nameof(configureHttpClient));

        var httpClientBuilder = Services.AddHttpClientForGateway<TGateway>();

        configureHttpClient(httpClientBuilder);

        return this;
    }

    /// <inheritdoc />
    public IGatewayConfigurationBuilder<TGateway> WithHttpClient<TGatewayApi, TGatewayApiImplementation>(Action<IServiceProvider, HttpClient> configureHttpClient,
                                                                                                         Action<IHttpClientBuilder> configureHttpClientBuilder = null)
        where TGatewayApi : class
        where TGatewayApiImplementation : class, TGatewayApi
    {
        if (configureHttpClient == null) throw new ArgumentNullException(nameof(configureHttpClient));

        var httpClientBuilder = Services.AddHttpClient<TGatewayApi, TGatewayApiImplementation>(configureHttpClient);

        if (configureHttpClientBuilder != null)
        {
            configureHttpClientBuilder(httpClientBuilder);
        }

        return this;
    }
}
