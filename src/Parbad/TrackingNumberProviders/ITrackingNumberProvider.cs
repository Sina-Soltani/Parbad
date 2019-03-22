// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Parbad.TrackingNumberProviders
{
    /// <summary>
    /// Defines a mechanism for generating a tracking number for each payment requests.
    /// </summary>
    public interface ITrackingNumberProvider
    {
        /// <summary>
        /// Provides a new tracking number.
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task<long> ProvideAsync(CancellationToken cancellationToken = default);
    }
}
