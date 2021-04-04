// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Parbad.TrackingNumberProviders;
using System;

namespace Parbad.Builder
{
    public static class AutoTrackingNumberOptionsExtensions
    {
        public static IParbadBuilder ConfigureAutoTrackingNumber(this IParbadBuilder builder,
            Action<AutoTrackingNumberOptions> configureOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.Configure(configureOptions);

            return builder;
        }
    }
}
