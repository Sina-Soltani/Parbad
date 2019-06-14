// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Parbad.Internal;
using Parbad.Storage.Abstractions;

namespace Parbad.TrackingNumberProviders
{
    public class AutoRandomTrackingNumberProvider : ITrackingNumberProvider
    {
        private readonly IStorage _storage;
        private readonly AutoTrackingNumberOptions _options;

        public AutoRandomTrackingNumberProvider(IStorage storage, IOptions<AutoTrackingNumberOptions> options)
        {
            _storage = storage;
            _options = options.Value;
        }

        public virtual Task<long> ProvideAsync(CancellationToken cancellationToken = default)
        {
            var trackingNumbers = _storage.Payments.Select(model => model.TrackingNumber).ToList();

            var minimumValue = _options.MinimumValue;

            var newNumber = minimumValue;

            if (trackingNumbers.Count > 0)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    newNumber = RandomNumberGenerator.Next();

                    if (newNumber >= minimumValue && !trackingNumbers.Contains(newNumber))
                    {
                        break;
                    }
                }
            }

            return Task.FromResult(newNumber);
        }
    }
}
