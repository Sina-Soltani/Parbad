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
        public string TerminalId { get; set; }

        public string MerchantId { get; set; }

        public string TerminalKey { get; set; }
    }
}
