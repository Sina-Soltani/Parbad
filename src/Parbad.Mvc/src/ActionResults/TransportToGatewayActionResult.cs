// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;

namespace Parbad.Mvc.ActionResults
{
    /// <summary>
    /// An <see cref="ActionResult"/> for transporting the client to the specified gateway.
    /// </summary>
    public class TransportToGatewayActionResult : ActionResult
    {
        private readonly IGatewayTransporter _transporter;

        /// <summary>
        /// Initializes an instance of <see cref="TransportToGatewayActionResult"/> for redirecting the client to the gateway.
        /// </summary>
        public TransportToGatewayActionResult(IGatewayTransporter transporter)
        {
            _transporter = transporter ?? throw new ArgumentNullException(nameof(transporter));
        }

        /// <inheritdoc />
        public override void ExecuteResult(ControllerContext context)
        {
            _transporter.Transport();
        }
    }
}
