// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.InvoiceBuilder;
using Parbad.Storage.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.TrackingNumberProviders
{
    /// <summary>
    /// An Invoice Formatter which generates automatically and incrementally a Tracking Number
    /// depending on <see cref="IStorage"/> and <see cref="AutoIncrementTrackingNumberOptions"/>.
    /// </summary>
    public class AutoIncrementTrackingNumber : IInvoiceFormatter
    {
        private readonly IStorage _storage;
        private readonly AutoIncrementTrackingNumberOptions _options;

        public AutoIncrementTrackingNumber(IStorage storage, IOptions<AutoIncrementTrackingNumberOptions> options)
        {
            _storage = storage;
            _options = options.Value;
        }

        /// <summary>
        /// Generates the Tracking Number.
        /// </summary>
        /// <param name="invoice"></param>
        /// <param name="cancellationToken"></param>
        public virtual Task FormatAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            long trackingNumber;

            var minimumValue = _options.MinimumValue;

            if (!_storage.Payments.Any())
            {
                trackingNumber = minimumValue;
            }
            else
            {
                trackingNumber = _storage.Payments.Max(model => model.TrackingNumber);

                if (trackingNumber < minimumValue)
                {
                    trackingNumber = minimumValue;
                }
                else
                {
                    trackingNumber += _options.Increment;
                }
            }

            invoice.TrackingNumber = trackingNumber;

            return Task.CompletedTask;
        }
    }
}
