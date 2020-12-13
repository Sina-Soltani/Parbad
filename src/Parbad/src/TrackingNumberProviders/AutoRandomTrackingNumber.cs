// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using Parbad.Storage.Abstractions;

namespace Parbad.TrackingNumberProviders
{
    public class AutoRandomTrackingNumber : IInvoiceFormatter
    {
        private readonly IStorage _storage;
        private readonly AutoTrackingNumberOptions _options;

        public AutoRandomTrackingNumber(IStorage storage, IOptions<AutoTrackingNumberOptions> options)
        {
            _storage = storage;
            _options = options.Value;
        }

        public Task FormatAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            var trackingNumbers = _storage.Payments.Select(model => model.TrackingNumber).ToList();

            var minimumValue = _options.MinimumValue;
            var maximumValue = _options.MaximumValue;

            var newNumber = 0L;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) break;

                newNumber = RandomNumberGenerator.Next();

                if (newNumber < minimumValue) continue;

                if (trackingNumbers.Count > 0 && !trackingNumbers.Contains(newNumber)) break;

                if (newNumber >= minimumValue && newNumber <= maximumValue) break;
            }

            invoice.TrackingNumber = newNumber;

            return Task.CompletedTask;
        }
    }
}
