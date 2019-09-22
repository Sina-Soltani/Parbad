// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Builder;

namespace Parbad
{
    /// <inheritdoc />
    public class ParbadBuilder : IParbadBuilder
    {
        /// <summary>
        /// Initializes an instance of <see cref="ParbadBuilder"/> class with the given <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services"></param>
        public ParbadBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <inheritdoc />
        public IServiceCollection Services { get; }

        /// <inheritdoc />
        public IOnlinePaymentAccessor Build()
        {
            var serviceProvider = Services.BuildServiceProvider();

            var onlinePaymentAccessor = serviceProvider.GetRequiredService<IOnlinePaymentAccessor>();

            StaticOnlinePayment.Initialize(onlinePaymentAccessor);

            return onlinePaymentAccessor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IParbadBuilder"/> class with pre-configured services.
        /// </summary>
        public static IParbadBuilder CreateDefaultBuilder(IServiceCollection services = null)
        {
            return (services ?? new ServiceCollection()).AddParbad();
        }
    }
}
