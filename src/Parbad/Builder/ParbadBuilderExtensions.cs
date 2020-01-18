// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection.Extensions;
using Parbad;
using Parbad.Abstraction;
using Parbad.Builder;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using Parbad.TrackingNumberProviders;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ParbadBuilderExtensions
    {
        /// <summary>
        /// Adds Parbad pre-configured services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        public static IParbadBuilder AddParbad(this IServiceCollection services)
        {
            var builder = new ParbadBuilder(services);

            builder.Services.AddOptions();

            builder.Services.AddHttpClient();

            builder.Services.TryAddTransient<IOnlinePayment, DefaultOnlinePayment>();
            builder.Services.TryAddSingleton<IOnlinePaymentAccessor, OnlinePaymentAccessor>();

            builder.Services.TryAddTransient<IInvoiceBuilder, DefaultInvoiceBuilder>();

            builder.Services.TryAddTransient<IGatewayProvider, DefaultGatewayProvider>();

            builder.Services.TryAddTransient<AutoIncrementTrackingNumberProvider>();
            builder.Services.TryAddTransient<AutoRandomTrackingNumberProvider>();

            builder.ConfigureMessages(options => { });

            builder.ConfigurePaymentToken(tokenBuilder => tokenBuilder.UseGuidQueryStringPaymentTokenProvider());

            return builder;
        }
    }
}
