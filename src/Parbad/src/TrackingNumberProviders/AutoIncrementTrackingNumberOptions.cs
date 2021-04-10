// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.TrackingNumberProviders
{
    public class AutoIncrementTrackingNumberOptions
    {
        public static long DefaultMinimumNumber => 1000;

        /// <summary>
        /// The default value equals to 1000.
        /// </summary>
        public long MinimumValue { get; set; } = DefaultMinimumNumber;

        /// <summary>
        /// The Tracking Number increments by this property. The default value equals to 1.
        /// </summary>
        public int Increment { get; set; } = 1;
    }
}
