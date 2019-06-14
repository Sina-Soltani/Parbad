// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.PaymentTokenProviders;

namespace Parbad.Internal
{
    internal class PaymentTokenBuilder : IPaymentTokenBuilder
    {
        public PaymentTokenBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public void AddPaymentTokenProvider<TProvider>(ServiceLifetime serviceLifetime) where TProvider : class, IPaymentTokenProvider
        {
            Services.AddOrUpdate<IPaymentTokenProvider, TProvider>(serviceLifetime);
        }

        public void AddPaymentTokenProvider(Func<IServiceProvider, IPaymentTokenProvider> factory, ServiceLifetime serviceLifetime)
        {
            Services.AddOrUpdate(factory, serviceLifetime);
        }
    }
}
