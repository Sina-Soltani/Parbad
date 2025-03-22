// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Parbad.Gateway.Pasargad.Api;
using Parbad.Gateway.Pasargad.Internal;
using Parbad.GatewayBuilders;

namespace Parbad.Gateway.Pasargad;

public static class PasargadGatewayBuilderExtensions
{
    /// <summary>
    /// Adds Pasargad gateway to Parbad services.
    /// </summary>
    /// <param name="builder"></param>
    public static IGatewayConfigurationBuilder<PasargadGateway> AddPasargad(this IGatewayBuilder builder)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        return builder
              .AddGateway<PasargadGateway>()
              .WithHttpClient<PasargadApi>((serviceProvider, httpClient) =>
                                           {
                                               var gatewayOptions = serviceProvider.GetRequiredService<IOptions<PasargadGatewayOptions>>();

                                               httpClient.BaseAddress = new Uri(gatewayOptions.Value.ApiBaseUrl);
                                           })
              .WithOptions(options => { });
    }

    /// <summary>
    /// Configures the HttpClient for <see cref="IPasargadApi"/>.
    /// </summary>
    /// <typeparam name="TGatewayApi">Implementation type of <see cref="IPasargadApi"/>.</typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    public static IGatewayConfigurationBuilder<PasargadGateway> WithHttpClient<TGatewayApi>(this IGatewayConfigurationBuilder<PasargadGateway> builder,
                                                                                            Action<IServiceProvider, HttpClient> configureHttpClient,
                                                                                            Action<IHttpClientBuilder> configureHttpClientBuilder = null)
        where TGatewayApi : class, IPasargadApi
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (configureHttpClient == null) throw new ArgumentNullException(nameof(configureHttpClient));

        builder.WithHttpClient<IPasargadApi, TGatewayApi>(configureHttpClient, configureHttpClientBuilder);

        return builder;
    }

    /// <summary>
    /// Configures the accounts for <see cref="PasargadGateway"/>.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureAccounts">Configures the accounts.</param>
    public static IGatewayConfigurationBuilder<PasargadGateway> WithAccounts(
        this IGatewayConfigurationBuilder<PasargadGateway> builder,
        Action<IGatewayAccountBuilder<PasargadGatewayAccount>> configureAccounts)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        return builder.WithAccounts(configureAccounts);
    }

    /// <summary>
    /// Configures the options for Pasargad Gateway.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureOptions">Configuration</param>
    public static IGatewayConfigurationBuilder<PasargadGateway> WithOptions(
        this IGatewayConfigurationBuilder<PasargadGateway> builder,
        Action<PasargadGatewayOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);

        return builder;
    }
}
