// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Abstraction;

namespace Parbad.Gateway.IranKish;

/// <summary>
/// IranKish gateway account.
/// </summary>
public class IranKishGatewayAccount : GatewayAccount
{
    public string TerminalId { get; set; } = null!;

    public string AcceptorId { get; set; } = null!;

    public string PassPhrase { get; set; } = null!;

    public string PublicKey { get; set; } = null!;
}