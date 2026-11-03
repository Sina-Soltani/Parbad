// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using Parbad.Storage.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.TrackingNumberProviders
{
    /// <summary>
    /// An Invoice Formatter which generates automatically and randomly a Tracking Number
    /// depending on <see cref="IStorage"/> and <see cref="AutoRandomTrackingNumberOptions"/>.
    /// </summary>
    public class AutoRandomTrackingNumber : IInvoiceFormatter
    {
        // With 2^63 possible values a collision is already very unlikely; repeated collisions mean that
        // the range above MinimumValue is (nearly) used up, so retrying for ever would never end.
        private const int MaxAttempts = 10;

        private readonly IStorage _storage;
        private readonly AutoRandomTrackingNumberOptions _options;

        public AutoRandomTrackingNumber(IStorage storage, IOptions<AutoRandomTrackingNumberOptions> options)
        {
            _storage = storage;
            _options = options.Value;
        }

        /// <summary>
        /// Generates a random Tracking Number which is not used by any stored payment.
        /// </summary>
        /// <param name="invoice"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="InvalidOperationException">No unused number was found.</exception>
        public virtual async Task FormatAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            // Random tracking numbers have always been non-negative, whatever the configured minimum.
            var minimumValue = Math.Max(_options.MinimumValue, 0);

            for (var attempt = 0; attempt < MaxAttempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var trackingNumber = RandomNumberGenerator.NextInt64(minimumValue);

                if (!await _storage.DoesPaymentExistAsync(trackingNumber, cancellationToken).ConfigureAwaitFalse())
                {
                    invoice.TrackingNumber = trackingNumber;

                    return;
                }
            }

            throw new InvalidOperationException(
                $"No unused tracking number was found after {MaxAttempts} random attempts in the range " +
                $"[{minimumValue}, {long.MaxValue}]. The range is probably almost used up; " +
                $"lower {nameof(AutoRandomTrackingNumberOptions)}.{nameof(AutoRandomTrackingNumberOptions.MinimumValue)}.");
        }
    }
}
