// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Parbad.Storage.Abstractions;

namespace Parbad.TrackingNumberProviders
{
    public class AutoIncrementTrackingNumberProvider : ITrackingNumberProvider
    {
        private readonly IStorage _storage;
        private readonly AutoTrackingNumberOptions _options;

        public AutoIncrementTrackingNumberProvider(IStorage storage, IOptions<AutoTrackingNumberOptions> options)
        {
            _storage = storage;
            _options = options.Value;
        }

        public virtual Task<long> ProvideAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            long max;

            var minimumValue = _options.MinimumValue;

            if (!_storage.Payments.Any())
            {
                max = minimumValue;
            }
            else
            {
                max = _storage.Payments.Max(model => model.TrackingNumber);

                if (max < minimumValue)
                {
                    max = minimumValue;
                }
                else
                {
                    max += 1;
                }
            }

            return Task.FromResult(max);
        }
    }
}
