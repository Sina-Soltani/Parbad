// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Parbad.Data.Context;
using Parbad.TrackingNumberProviders;

namespace Parbad.Internal
{
    public class AutoIncrementTrackingNumberProvider : ITrackingNumberProvider
    {
        private readonly ParbadDataContext _database;
        private readonly IOptions<AutoTrackingNumberOptions> _options;

        public AutoIncrementTrackingNumberProvider(ParbadDataContext database, IOptions<AutoTrackingNumberOptions> options)
        {
            _database = database;
            _options = options;
        }

        public virtual async Task<long> ProvideAsync(CancellationToken cancellationToken = default)
        {
            long max;

            var minimumValue = _options.Value.MinimumValue;

            if (!await _database.Payments.AnyAsync(cancellationToken))
            {
                max = minimumValue;
            }
            else
            {
                max = await _database.Payments.MaxAsync(model => model.TrackingNumber, cancellationToken);

                if (max < minimumValue)
                {
                    max = minimumValue;
                }
                else
                {
                    max += 1;
                }
            }

            return max;
        }
    }
}
