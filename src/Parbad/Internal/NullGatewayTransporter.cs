// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Internal
{
    public class NullGatewayTransporter : IGatewayTransporter
    {
        public NullGatewayTransporter()
        {
            Descriptor = null;
        }

        public GatewayTransporterDescriptor Descriptor { get; }

        public Task TransportAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
