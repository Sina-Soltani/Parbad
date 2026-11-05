// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Gateway.AsanPardakht;

public class AsanPardakhtGatewayOptions
{
    private const string SoapObsoleteMessage = "Not used: since Parbad 3.12 the AsanPardakht gateway uses AsanPardakht's REST API (ApiBaseUrl). This property will be removed in Parbad 4.0.";

    public string PaymentPageUrl { get; set; } = "https://asan.shaparak.ir/";

    /// <summary>
    /// The base URL of AsanPardakht's REST API.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://ipgrest.asanpardakht.ir/v1/";

    [Obsolete(SoapObsoleteMessage)]
    public string ApiUrl { get; set; } = "https://ipgsoap.asanpardakht.ir/paygate/merchantservices.asmx";

    [Obsolete(SoapObsoleteMessage)]
    public string EncryptUrl { get; set; } = "https://ipgsoap.asanpardakht.ir/paygate/internalutils.asmx?op=EncryptInAES";

    [Obsolete(SoapObsoleteMessage)]
    public string DecryptUrl { get; set; } = "https://ipgsoap.asanpardakht.ir/paygate/internalutils.asmx?op=DecryptInAES";
}