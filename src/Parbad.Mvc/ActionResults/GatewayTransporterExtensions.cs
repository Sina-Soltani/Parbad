// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Web.Mvc;
using Parbad.Mvc.ActionResults;

namespace Parbad.Mvc
{
    public static class GatewayTransporterExtensions
    {
        /// <summary>
        /// Transports the client to the specified gateway.
        /// </summary>
        public static ActionResult TransportToGateway(this IGatewayTransporter transporter)
        {
            return new TransportToGatewayActionResult(transporter);
        }
    }
}
