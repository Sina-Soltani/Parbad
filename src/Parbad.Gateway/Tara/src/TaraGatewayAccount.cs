// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Abstraction;

namespace Parbad.Gateway.Tara
{
    public class TaraGatewayAccount : GatewayAccount
    {
        /// <summary>
        /// Gets or sets the username for Tara gateway authentication.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password for Tara gateway authentication.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the IP address to use for API calls.
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Gets or sets whether to use the staging/test environment.
        /// When true, uses https://stage-pay.tara360.ir/pay instead of production.
        /// </summary>
        public bool IsTest { get; set; }
    }
}

