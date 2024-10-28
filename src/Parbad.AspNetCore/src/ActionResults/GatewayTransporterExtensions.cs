// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Parbad.AspNetCore.ActionResults;

namespace Parbad.AspNetCore;

public static class GatewayTransporterExtensions
{
    /// <summary>
    /// Transports the client to the specified gateway.
    /// </summary>
    public static IActionResult TransportToGateway(this IGatewayTransporter transporter)
    {
        return new TransportToGatewayActionResult(transporter);
    }
}
