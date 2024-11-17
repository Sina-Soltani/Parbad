// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Abstraction;

namespace Parbad.Gateway.Pasargad;

public class PasargadGatewayAccount : GatewayAccount
{
    public string MerchantCode { get; set; }

    public string TerminalCode { get; set; }

    public string PrivateKey { get; set; }
}
