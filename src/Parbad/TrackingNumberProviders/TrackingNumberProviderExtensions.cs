// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using Parbad.TrackingNumberProviders;

namespace Parbad
{
    public static class TrackingNumberProviderExtensions
    {
        /// <summary>
        /// Sets the tracking number of invoice.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="trackingNumber">
        /// The tracking number of invoice.
        /// <para>Note: It must be unique for each payment requests.</para>
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IInvoiceBuilder SetTrackingNumber(this IInvoiceBuilder builder, long trackingNumber)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetTrackingNumberProvider(new UserInputTrackingNumberProvider(trackingNumber));
        }

        /// <summary>
        /// Sets a provider which generates unique tracking numbers for each payment requests.
        /// </summary>
        /// <typeparam name="TProvider">Type of provider.</typeparam>
        /// <param name="builder"></param>
        public static IInvoiceBuilder SetTrackingNumberProvider<TProvider>(this IInvoiceBuilder builder)
            where TProvider : class, ITrackingNumberProvider
        {
            return builder.SetTrackingNumberProvider(provider => provider.GetRequiredService<TProvider>());
        }

        /// <summary>
        /// Sets a provider which generates unique tracking numbers for each payment requests.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="factory"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IInvoiceBuilder SetTrackingNumberProvider(this IInvoiceBuilder builder, Func<IServiceProvider, ITrackingNumberProvider> factory)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            return builder.SetTrackingNumberProvider(factory(builder.Services));
        }

        /// <summary>
        /// Generates automatically a new tracking number which is greater than the latest number.
        /// <para>Note: You can also set a starting number by configuring the AutoTrackingNumberOptions.</para>
        /// </summary>
        public static IInvoiceBuilder UseAutoIncrementTrackingNumber(this IInvoiceBuilder builder, long minimumValue)
        {
            SetMinimumValue(builder, minimumValue);

            return builder.UseAutoIncrementTrackingNumber();
        }

        /// <summary>
        /// Generates automatically a new tracking number which is greater than the latest number.
        /// <para>Note: You can also set a starting number by configuring the AutoTrackingNumberOptions.</para>
        /// </summary>
        public static IInvoiceBuilder UseAutoIncrementTrackingNumber(this IInvoiceBuilder builder) =>
            builder.SetTrackingNumberProvider<AutoIncrementTrackingNumberProvider>();

        /// <summary>
        /// Generates a new tracking number randomly in <see cref="long"/> range.
        /// </summary>
        public static IInvoiceBuilder UseAutoRandomTrackingNumber(this IInvoiceBuilder builder) =>
            builder.SetTrackingNumberProvider<AutoRandomTrackingNumberProvider>();

        /// <summary>
        /// Generates a new tracking number randomly in <see cref="long"/> range.
        /// </summary>
        public static IInvoiceBuilder UseAutoRandomTrackingNumber(this IInvoiceBuilder builder, long minimumValue)
        {
            SetMinimumValue(builder, minimumValue);

            return builder.SetTrackingNumberProvider<AutoIncrementTrackingNumberProvider>();
        }

        private static void SetMinimumValue(IInvoiceBuilder builder, long minimumValue)
        {
            var options = builder.Services.GetRequiredService<IOptions<AutoTrackingNumberOptions>>();

            options.Value.MinimumValue = minimumValue;
        }
    }
}
