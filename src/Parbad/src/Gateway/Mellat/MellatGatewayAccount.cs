// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Abstraction;

namespace Parbad.Gateway.Mellat
{
    public class MellatGatewayAccount : GatewayAccount
    {
        public long TerminalId { get; set; }

        public string UserName { get; set; }

        public string UserPassword { get; set; }
    }
}
