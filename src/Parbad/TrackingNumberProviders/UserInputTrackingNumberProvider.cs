// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Parbad.Exceptions;
using Parbad.TrackingNumberProviders;

namespace Parbad.Internal
{
    /// <summary>
    /// Gets the tracking number direct from the user.
    /// </summary>
    /// <exception cref="InvalidTrackingNumberException"></exception>
    public class UserInputTrackingNumberProvider : ITrackingNumberProvider
    {
        /// <summary>
        /// Initializes an instance of <see cref="UserInputTrackingNumberProvider"/> with the
        /// given <paramref name="trackingNumber"/>.
        /// </summary>
        /// <param name="trackingNumber"></param>
        /// <exception cref="InvalidTrackingNumberException"></exception>
        public UserInputTrackingNumberProvider(long trackingNumber)
        {
            if (trackingNumber <= 0) throw new InvalidTrackingNumberException(trackingNumber);

            TrackingNumber = trackingNumber;
        }

        public long TrackingNumber { get; }

        public Task<long> ProvideAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TrackingNumber);
        }
    }
}
