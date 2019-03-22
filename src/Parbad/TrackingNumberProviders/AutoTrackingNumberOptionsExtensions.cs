// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parbad.TrackingNumberProviders;

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

        public static IParbadBuilder ConfigureAutoTrackingNumber(this IParbadBuilder builder,
            IConfiguration configuration)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.Configure<AutoTrackingNumberOptions>(configuration);

            return builder;
        }

        /// <summary>
        /// Retrieves the <see cref="IConfiguration"/> section of <see cref="AutoTrackingNumberOptions"/> using <see cref="AutoTrackingNumberOptions.ConfigurationKey"/>.
        /// </summary>
        public static IConfiguration GetAutoTrackingNumberOptionsConfiguration(this IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            return configuration.GetSection(AutoTrackingNumberOptions.ConfigurationKey);
        }
    }
}
