// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Abstraction
{
    /// <summary>
    /// A provider for providing an <see cref="IGateway"/>.
    /// </summary>
    public interface IGatewayProvider
    {
        /// <summary>
        /// Provides an instance of <see cref="IGateway"/> using the given <paramref name="gatewayType"/>.
        /// </summary>
        /// <param name="gatewayType">Type of the gateway.</param>
        IGateway Provide(Type gatewayType);
    }
}
