// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Parbad.Gateway.Saman.Api;
using Parbad.Gateway.Saman.Internal;
using Parbad.GatewayBuilders;

namespace Parbad.Gateway.Saman;

public static class SamanGatewayBuilderExtensions
{
    /// <summary>
    /// Adds Saman gateway to Parbad services.
    /// </summary>
    /// <param name="builder"></param>
    public static IGatewayConfigurationBuilder<SamanGateway> AddSaman(this IGatewayBuilder builder)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        return builder
              .AddGateway<SamanGateway>()
              .WithHttpClient<SamanApi>((serviceProvider, httpClient) =>
                                           {
                                               var gatewayOptions = serviceProvider.GetRequiredService<IOptions<SamanGatewayOptions>>();

                                               httpClient.BaseAddress = new Uri(gatewayOptions.Value.ApiBaseUrl);
                                           })
              .WithOptions(options => { });
    }

    /// <summary>
    /// Configures the HttpClient for <see cref="ISamanApi"/>.
    /// </summary>
    /// <typeparam name="TGatewayApi">Implementation type of <see cref="ISamanApi"/>.</typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    public static IGatewayConfigurationBuilder<SamanGateway> WithHttpClient<TGatewayApi>(this IGatewayConfigurationBuilder<SamanGateway> builder,
                                                                                         Action<IServiceProvider, HttpClient> configureHttpClient,
                                                                                         Action<IHttpClientBuilder> configureHttpClientBuilder = null)
        where TGatewayApi : class, ISamanApi
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (configureHttpClient == null) throw new ArgumentNullException(nameof(configureHttpClient));

        builder.WithHttpClient<ISamanApi, TGatewayApi>(configureHttpClient, configureHttpClientBuilder);

        return builder;
    }

    /// <summary>
    /// Configures the accounts for <see cref="SamanGateway"/>.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureAccounts">Configures the accounts.</param>
    public static IGatewayConfigurationBuilder<SamanGateway> WithAccounts(
        this IGatewayConfigurationBuilder<SamanGateway> builder,
        Action<IGatewayAccountBuilder<SamanGatewayAccount>> configureAccounts)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        return builder.WithAccounts(configureAccounts);
    }

    /// <summary>
    /// Configures the options for Saman Gateway.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureOptions">Configuration</param>
    public static IGatewayConfigurationBuilder<SamanGateway> WithOptions(
        this IGatewayConfigurationBuilder<SamanGateway> builder,
        Action<SamanGatewayOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);

        return builder;
    }
}
