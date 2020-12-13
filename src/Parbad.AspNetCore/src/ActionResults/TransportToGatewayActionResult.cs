// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Parbad.AspNetCore.ActionResults
{
    /// <summary>
    /// An <see cref="IActionResult"/> for transporting the client to the specified gateway.
    /// </summary>
    public class TransportToGatewayActionResult : IActionResult
    {
        private readonly IGatewayTransporter _transporter;

        /// <summary>
        /// Initializes an instance of <see cref="TransportToGatewayActionResult"/> for redirecting the client to the gateway.
        /// </summary>
        public TransportToGatewayActionResult(IGatewayTransporter transporter)
        {
            _transporter = transporter;
        }

        /// <inheritdoc />
        public Task ExecuteResultAsync(ActionContext context)
        {
            return _transporter.TransportAsync();
        }
    }
}
