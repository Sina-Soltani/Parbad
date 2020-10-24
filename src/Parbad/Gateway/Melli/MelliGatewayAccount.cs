// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Abstraction;

namespace Parbad.Gateway.Melli
{
    /// <summary>
    /// Melli Gateway options class.
    /// </summary>
    public class MelliGatewayAccount : GatewayAccount
    {
        /// <summary>
        /// Terminal Id that provide by bank
        /// </summary>
        public string TerminalId { get; set; }
        /// <summary>
        /// Merchant Id that provide by bank
        /// </summary>
        public string MerchantId { get; set; }
        /// <summary>
        /// Terminal Key that provide by bank
        /// </summary>
        public string TerminalKey { get; set; }
    }
}
