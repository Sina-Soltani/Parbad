// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Parbad.Abstraction;
using Parbad.GatewayBuilders;

namespace Parbad.Internal
{
    internal class GatewayAccountBuilder<TAccount> : IGatewayAccountBuilder<TAccount> where TAccount : GatewayAccount
    {
        public GatewayAccountBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public IGatewayAccountBuilder<TAccount> Clear()
        {
            Services.RemoveAll<IGatewayAccountSource<TAccount>>();

            return this;
        }

        public IGatewayAccountBuilder<TAccount> Add(IGatewayAccountSource<TAccount> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            Services.AddSingleton(source);

            return this;
        }

        public IGatewayAccountBuilder<TAccount> Add<TSource>(ServiceLifetime serviceLifetime) where TSource : class, IGatewayAccountSource<TAccount>
        {
            if (!Enum.IsDefined(typeof(ServiceLifetime), serviceLifetime))
                throw new InvalidEnumArgumentException(nameof(serviceLifetime), (int)serviceLifetime,
                    typeof(ServiceLifetime));

            Services.Add<IGatewayAccountSource<TAccount>, TSource>(serviceLifetime);

            return this;
        }

        public IGatewayAccountBuilder<TAccount> Add(Func<IServiceProvider, IGatewayAccountSource<TAccount>> factory, ServiceLifetime serviceLifetime)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (!Enum.IsDefined(typeof(ServiceLifetime), serviceLifetime))
                throw new InvalidEnumArgumentException(nameof(serviceLifetime), (int)serviceLifetime,
                    typeof(ServiceLifetime));

            Services.Add(factory, serviceLifetime);

            return this;
        }
    }
}
