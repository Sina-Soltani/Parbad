// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Parbad
{
    /// <summary>
    /// Defines a mechanism for transporting the client to a gateway.
    /// </summary>
    public interface IGatewayTransporter
    {
        /// <summary>
        /// Describes a gateway transporter.
        /// </summary>
        GatewayTransporterDescriptor Descriptor { get; }

        /// <summary>
        /// Transports the client to the specified gateway.
        /// </summary>
        Task TransportAsync(CancellationToken cancellationToken = default);
    }
}
