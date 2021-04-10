// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using Parbad.Storage.Abstractions;
using System.Linq;
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
        private readonly IStorage _storage;
        private readonly AutoRandomTrackingNumberOptions _options;

        public AutoRandomTrackingNumber(IStorage storage, IOptions<AutoRandomTrackingNumberOptions> options)
        {
            _storage = storage;
            _options = options.Value;
        }

        public virtual Task FormatAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            var trackingNumbers = _storage.Payments.Select(model => model.TrackingNumber).ToList();

            var minimumValue = _options.MinimumValue;

            var trackingNumber = 0L;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) break;

                trackingNumber = RandomNumberGenerator.Next();

                if (trackingNumber >= minimumValue && !trackingNumbers.Contains(trackingNumber))
                {
                    break;
                }
            }

            invoice.TrackingNumber = trackingNumber;

            return Task.CompletedTask;
        }
    }
}
