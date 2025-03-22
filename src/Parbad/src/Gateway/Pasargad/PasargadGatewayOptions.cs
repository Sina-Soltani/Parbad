// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Pasargad;

public class PasargadGatewayOptions
{
    /// <summary>
    /// According to Pasargad gateway, the BaseUrl is different for each merchant.
    /// Get the BaseUrl from Pasargad gateway and set the value here.
    /// </summary>
    public string ApiBaseUrl { get; set; }

    public string ApiGetTokenUrl { get; set; } = "Token/GetToken";

    public string ApiPurchaseUrl { get; set; } = "Api/Payment/purchase";

    public string ApiVerificationUrl { get; set; } = "Api/Payment/Verify-Payment";

    public string ApiReverseUrl { get; set; } = "Api/Payment/Reverse-Transactions";
}
