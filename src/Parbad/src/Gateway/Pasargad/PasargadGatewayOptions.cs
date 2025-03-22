// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Pasargad;

public class PasargadGatewayOptions
{
    /// <summary>
    /// like https://pep.shaparak.ir/dorsa2 OR https://pep.shaparak.ir/dorsa1
    /// </summary>
    public string ApiBaseUrl { get; set; }

    public string ApiGetTokenUrl { get; set; } = "Token/GetToken";

    public string ApiPurchaseUrl { get; set; } = "Api/Payment/purchase";

    public string ApiVerificationUrl { get; set; } = "Api/Payment/Verify-Payment";

    public string ApiRefundUrl { get; set; } = "Api/Payment/Reverse-Transactions";
}
