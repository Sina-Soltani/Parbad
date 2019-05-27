// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Abstraction;

namespace Parbad.GatewayBuilders
{
    /// <summary>
    /// A builder for configuring the specified gateway.
    /// </summary>
    /// <typeparam name="TGateway">Type of gateway.</typeparam>
    public interface IGatewayConfigurationBuilder<TGateway> where TGateway : class, IGateway
    {
        /// <summary>
        /// Specifies the contract for a collection of service descriptors.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Configures the accounts of type <typeparamref name="TAccount"/> for <typeparamref name="TGateway"/>.
        /// </summary>
        /// <typeparam name="TAccount">Account of <typeparamref name="TGateway"/>.</typeparam>
        /// <param name="configureAccounts">Configures the accounts.</param>
        IGatewayConfigurationBuilder<TGateway> WithAccounts<TAccount>(
            Action<IGatewayAccountBuilder<TAccount>> configureAccounts)
            where TAccount : GatewayAccount, new();

        /// <summary>
        /// Adds the configured options to Parbad services.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="configureOptions"></param>
        [Obsolete("This is obsolete and will be removed in a future version. Use the WithAccount method instead.")]
        IGatewayConfigurationBuilder<TGateway> WithOptions<TOptions>(Action<TOptions> configureOptions) where TOptions : class, new();

        /// <summary>
        /// Configures the <see cref="HttpClient"/> required by <typeparamref name="TGateway"/>
        /// for sending HTTP requests and receiving HTTP responses.
        /// </summary>
        /// <param name="configureHttpClient">HttpClient configuration.</param>
        IGatewayConfigurationBuilder<TGateway> WithHttpClient(Action<IHttpClientBuilder> configureHttpClient);
    }
}
