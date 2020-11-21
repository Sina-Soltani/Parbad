// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.InvoiceBuilder;
using Parbad.TrackingNumberProviders;

namespace Parbad
{
    public static class TrackingNumberProviderExtensions
    {
        /// <summary>
        /// Generates automatically a new tracking number which is greater than the latest number.
        /// <para>Note: You can also set a starting number by configuring the AutoTrackingNumberOptions.</para>
        /// </summary>
        public static IInvoiceBuilder UseAutoIncrementTrackingNumber(this IInvoiceBuilder builder) =>
            builder.AddFormatter<AutoIncrementTrackingNumber>();

        /// <summary>
        /// Generates a new tracking number randomly in <see cref="long"/> range.
        /// </summary>
        public static IInvoiceBuilder UseAutoRandomTrackingNumber(this IInvoiceBuilder builder) =>
            builder.AddFormatter<AutoRandomTrackingNumber>();
    }
}
