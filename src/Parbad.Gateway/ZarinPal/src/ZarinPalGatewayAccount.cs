// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Abstraction;

namespace Parbad.Gateway.ZarinPal;

/// <summary>
/// Describes an account for ZarinPal Gateway.
/// </summary>
public class ZarinPalGatewayAccount : GatewayAccount
{
    public string MerchantId { get; set; } = null!;

    /// <summary>
    /// The access token of the ZarinPal panel, needed only for refunds.
    /// </summary>
    public string? AuthorizationToken { get; set; }

    public bool IsSandbox { get; set; }
}
