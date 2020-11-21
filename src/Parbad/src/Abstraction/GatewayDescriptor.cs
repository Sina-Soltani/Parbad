// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Internal;

namespace Parbad.Abstraction
{
    /// <summary>
    /// Describes the type of a gateway.
    /// </summary>
    public class GatewayDescriptor
    {
        /// <summary>
        /// Initializes an instance of <see cref="GatewayDescriptor"/>.
        /// </summary>
        /// <param name="gatewayType">Type of the gateway.</param>
        public GatewayDescriptor(Type gatewayType)
        {
            GatewayHelper.IsGateway(gatewayType, throwException: true);

            GatewayType = gatewayType;
        }

        /// <summary>
        /// Gets the type of the gateway.
        /// </summary>
        public Type GatewayType { get; }
    }
}
