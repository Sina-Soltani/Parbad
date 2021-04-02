// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Parbad.Abstraction;
using Parbad.Builder;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using Parbad.TrackingNumberProviders;
using System;

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
            services ??= new ServiceCollection();

            var builder = new ParbadBuilder(services);

            builder.Services.AddOptions();

            builder.Services.AddHttpClient();

            builder.Services.TryAddTransient<IOnlinePayment, DefaultOnlinePayment>();
            builder.Services.TryAddSingleton<IOnlinePaymentAccessor, OnlinePaymentAccessor>();

            builder.Services.TryAddTransient<IInvoiceBuilder, DefaultInvoiceBuilder>();

            builder.Services.TryAddTransient<AutoIncrementTrackingNumber>();
            builder.Services.TryAddTransient<AutoRandomTrackingNumber>();

            builder.Services.TryAddTransient<IGatewayProvider, DefaultGatewayProvider>();

            builder.ConfigureOptions(options => { });

            builder.ConfigurePaymentToken(tokenBuilder => tokenBuilder.UseGuidQueryStringPaymentTokenProvider());

            builder.Services.TryAddTransient(typeof(IParbadLogger<>), typeof(ParbadLogger<>));

            return builder;
        }
    }
}
