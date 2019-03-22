// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Abstraction;
using Parbad.GatewayBuilders;
using Parbad.Net;

namespace Parbad.Internal
{
    /// <inheritdoc />
    public class GatewayConfigurationBuilder<TGateway> : IGatewayConfigurationBuilder<TGateway>
        where TGateway : class, IGateway
    {
        /// <summary>
        /// Initializes an instance of <see cref="GatewayConfigurationBuilder{TGateway}"/>.
        /// </summary>
        /// <param name="services"></param>
        public GatewayConfigurationBuilder(IServiceCollection services)
        {
            Services = services;
        }

        /// <inheritdoc />
        public IServiceCollection Services { get; }

        /// <inheritdoc />
        public IGatewayConfigurationBuilder<TGateway> WithOptions<TOptions>(Action<TOptions> configureOptions) where TOptions : class, new()
        {
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            Services.Configure(configureOptions);

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
    }
}
