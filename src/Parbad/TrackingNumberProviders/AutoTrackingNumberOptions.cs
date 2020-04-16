// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Configuration;

namespace Parbad.TrackingNumberProviders
{
    public class AutoTrackingNumberOptions
    {
        /// <summary>
        /// The specific key for <see cref="IConfiguration"/>
        /// </summary>
        public static readonly string ConfigurationKey = "Parbad:AutoTrackingNumber";

        public long MinimumValue { get; set; } = 1000;

        public long MaximumValue { get; set; } = long.MaxValue;
    }
}
