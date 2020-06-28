// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Parbad.Options;

namespace Parbad.Builder
{
    public static class ParbadOptionsExtensions
    {
        /// <summary>
        /// Configures the Parbad options.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="setupOptions"></param>
        public static IParbadBuilder ConfigureOptions(this IParbadBuilder builder, Action<ParbadOptions> setupOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (setupOptions == null) throw new ArgumentNullException(nameof(setupOptions));

            builder.Services.Configure(setupOptions);

            builder.Services.AddTransient<IOptions<MessagesOptions>>(provider =>
            {
                var messages = provider.GetRequiredService<IOptions<ParbadOptions>>().Value.Messages;

                return new OptionsWrapper<MessagesOptions>(messages);
            });

            return builder;
        }

        /// <summary>
        /// Configures the Parbad options with the given <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        public static IParbadBuilder ConfigureOptions(this IParbadBuilder builder, IConfiguration configuration)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            builder.Services.Configure<ParbadOptions>(configuration);

            return builder;
        }

        [Obsolete("This method is obsolete and will be removed in a future version. The recommended alternative is ConfigureOptions().", error: false)]
        public static IParbadBuilder ConfigureMessages(this IParbadBuilder builder, Action<MessagesOptions> configureOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            builder.ConfigureOptions(options => configureOptions(options.Messages));

            return builder;
        }

        [Obsolete("This method is obsolete and will be removed in a future version. The recommended alternative is ConfigureOptions().", error: false)]
        public static IParbadBuilder ConfigureMessages(this IParbadBuilder builder, IConfiguration configuration)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            builder.Services.Configure<MessagesOptions>(configuration);

            return builder;
        }
    }
}
