// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Abstraction;
using Parbad.GatewayBuilders;

namespace Parbad.Builder
{
    public static class GatewayHttpClientConfigurationBuilderExtensions
    {
        /// <summary>
        /// Configures the <see cref="HttpClient"/> of the specified gateway to use an <see cref="IWebProxy"/>
        /// for sending HTTP requests and receiving HTTP responses.
        /// </summary>
        /// <typeparam name="TGateway"></typeparam>
        /// <param name="builder"></param>
        /// <param name="proxyUrl">URL of proxy server.</param>
        public static IGatewayConfigurationBuilder<TGateway> WithProxy<TGateway>(
            this IGatewayConfigurationBuilder<TGateway> builder,
            Uri proxyUrl)
            where TGateway : class, IGateway
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (proxyUrl == null) throw new ArgumentNullException(nameof(proxyUrl));

            return builder.WithProxy(new WebProxy(proxyUrl));
        }

        /// <summary>
        /// Configures the <see cref="HttpClient"/> of the specified gateway to use an <see cref="IWebProxy"/>
        /// for sending HTTP requests and receiving HTTP responses.
        /// </summary>
        /// <typeparam name="TGateway"></typeparam>
        /// <param name="builder"></param>
        /// <param name="proxyUrl">URL of proxy server.</param>
        /// <param name="userName">Username of proxy server.</param>
        /// <param name="password">Password of proxy server.</param>
        public static IGatewayConfigurationBuilder<TGateway> WithProxy<TGateway>(
            this IGatewayConfigurationBuilder<TGateway> builder,
            Uri proxyUrl,
            string userName,
            string password)
            where TGateway : class, IGateway
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (proxyUrl == null) throw new ArgumentNullException(nameof(proxyUrl));
            if (userName == null) throw new ArgumentNullException(nameof(userName));
            if (password == null) throw new ArgumentNullException(nameof(password));

            return builder.WithProxy(new WebProxy(proxyUrl)
            {
                Credentials = new NetworkCredential(userName, password)
            });
        }

        /// <summary>
        /// Configures the <see cref="HttpClient"/> of the specified gateway to use an <see cref="IWebProxy"/>
        /// for sending HTTP requests and receiving HTTP responses.
        /// </summary>
        /// <typeparam name="TGateway"></typeparam>
        /// <param name="builder"></param>
        /// <param name="webProxy">The proxy that must be used.</param>
        /// <returns></returns>
        public static IGatewayConfigurationBuilder<TGateway> WithProxy<TGateway>(
            this IGatewayConfigurationBuilder<TGateway> builder,
            IWebProxy webProxy)
            where TGateway : class, IGateway
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (webProxy == null) throw new ArgumentNullException(nameof(webProxy));

            builder.WithHttpClient(clientBuilder =>
                clientBuilder.ConfigurePrimaryHttpMessageHandler(()
                    => new HttpClientHandler
                    {
                        Proxy = webProxy
                    }));

            return builder;
        }
    }
}
