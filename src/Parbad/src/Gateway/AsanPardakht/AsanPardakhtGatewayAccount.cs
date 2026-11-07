// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Abstraction;

namespace Parbad.Gateway.AsanPardakht;

public class AsanPardakhtGatewayAccount : GatewayAccount
{
    /// <summary>
    /// The merchant configuration ID (a number), given by AsanPardakht.
    /// </summary>
    public string MerchantConfigurationId { get; set; } = null!;

    /// <summary>
    /// The user name for AsanPardakht's API, sent in the <c>usr</c> header.
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// The password for AsanPardakht's API, sent in the <c>pwd</c> header.
    /// </summary>
    public string Password { get; set; } = null!;
}