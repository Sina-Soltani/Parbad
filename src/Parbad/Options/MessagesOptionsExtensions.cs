// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Options;

namespace Parbad.Builder
{
    public static class MessagesOptionsExtensions
    {
        public static IParbadBuilder ConfigureMessages(this IParbadBuilder builder,
            Action<MessagesOptions> configureOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            builder.Services.Configure(configureOptions);

            return builder;
        }

        public static IParbadBuilder ConfigureMessages(this IParbadBuilder builder,
            IConfiguration configuration)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            builder.Services.Configure<MessagesOptions>(configuration);

            return builder;
        }
    }
}
