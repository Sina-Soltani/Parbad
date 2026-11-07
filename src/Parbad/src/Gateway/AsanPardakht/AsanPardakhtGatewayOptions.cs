// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Gateway.AsanPardakht;

public class AsanPardakhtGatewayOptions
{
    public string PaymentPageUrl { get; set; } = "https://asan.shaparak.ir/";

    /// <summary>
    /// The base URL of AsanPardakht's REST API.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://ipgrest.asanpardakht.ir/v1/";
}