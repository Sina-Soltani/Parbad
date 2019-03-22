// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Parbad.Data.Context;
using Parbad.TrackingNumberProviders;

namespace Parbad.Internal
{
    public class AutoRandomTrackingNumberProvider : ITrackingNumberProvider
    {
        private readonly ParbadDataContext _database;
        private readonly IOptions<AutoTrackingNumberOptions> _options;

        public AutoRandomTrackingNumberProvider(ParbadDataContext database, IOptions<AutoTrackingNumberOptions> options)
        {
            _database = database;
            _options = options;
        }

        public virtual async Task<long> ProvideAsync(CancellationToken cancellationToken = default)
        {
            var trackingNumbers = await _database.Payments.Select(model => model.TrackingNumber).ToListAsync(cancellationToken);

            var minimumValue = _options.Value.MinimumValue;

            var newNumber = minimumValue;

            if (trackingNumbers.Count == 0)
            {
                newNumber = minimumValue;
            }
            else
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

            return newNumber;
        }
    }
}
