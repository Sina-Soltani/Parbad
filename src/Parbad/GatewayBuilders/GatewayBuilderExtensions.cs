// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.GatewayBuilders;
using Parbad.Internal;

namespace Parbad.Builder
{
    /// <summary>
    /// gateway builder
    /// </summary>
    public static class GatewayBuilderExtensions
    {
        /// <summary>
        /// Configures the gateways.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        public static IParbadBuilder ConfigureGateways(this IParbadBuilder builder, Action<IGatewayBuilder> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            configure(new GatewayBuilder(builder.Services));

            return builder;
        }
    }
}
