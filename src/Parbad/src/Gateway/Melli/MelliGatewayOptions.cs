// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Melli;

public class MelliGatewayOptions
{
    public string PaymentPageUrl { get; set; } = "https://sadad.shaparak.ir/VPG/Purchase";

    public string ApiBaseUrl { get; set; } = "https://sadad.shaparak.ir/VPG/api/v0/";

    public string ApiRequestUrl { get; set; } = "Request/PaymentRequest";

    public string ApiVerificationUrl { get; set; } = "Advice/Verify";
}
