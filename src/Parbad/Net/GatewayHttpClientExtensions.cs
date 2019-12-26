// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Abstraction;
using Parbad.Internal;

namespace Parbad.Net
{
    /// <summary>
    /// Manages the <see cref="HttpClient"/> for a specific gateway.
    /// </summary>
    public static class GatewayHttpClientExtensions
    {
        public static IHttpClientBuilder AddHttpClientForGateway<TGateway>(this IServiceCollection services) where TGateway : class, IGateway
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services.AddHttpClient(GatewayHelper.GetCompleteGatewayName<TGateway>());
        }

        public static HttpClient CreateClient(this IHttpClientFactory factory, IGateway gateway)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (gateway == null) throw new ArgumentNullException(nameof(gateway));

            return factory.CreateClient(gateway.GetCompleteGatewayName());
        }
    }
}
