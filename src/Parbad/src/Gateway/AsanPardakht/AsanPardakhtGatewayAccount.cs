// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Abstraction;

namespace Parbad.Gateway.AsanPardakht;

public class AsanPardakhtGatewayAccount : GatewayAccount
{
    private const string SoapObsoleteMessage = "Not used: since Parbad 3.12 the AsanPardakht gateway uses AsanPardakht's REST API, which needs no encryption. This property will be removed in Parbad 4.0.";

    /// <summary>
    /// The merchant configuration ID (a number), given by AsanPardakht.
    /// </summary>
    public string MerchantConfigurationId { get; set; }

    /// <summary>
    /// The user name for AsanPardakht's API, sent in the <c>usr</c> header.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// The password for AsanPardakht's API, sent in the <c>pwd</c> header.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// The key that AsanPardakht's former SOAP API used for encrypting the data.
    /// </summary>
    [Obsolete(SoapObsoleteMessage)]
    public string Key { get; set; }

    /// <summary>
    /// The IV that AsanPardakht's former SOAP API used for encrypting the data.
    /// </summary>
    [Obsolete(SoapObsoleteMessage)]
    public string IV { get; set; }
}