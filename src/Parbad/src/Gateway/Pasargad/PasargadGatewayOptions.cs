// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Pasargad;

public class PasargadGatewayOptions
{
    public string PaymentPageUrl { get; set; } = "https://pep.shaparak.ir/payment.aspx";

    public string ApiBaseUrl { get; set; } = "https://pep.shaparak.ir/Api/v1/";

    public string ApiGetTokenUrl { get; set; } = "Payment/GetToken";

    public string ApiVerificationUrl { get; set; } = "Payment/VerifyPayment";

    public string ApiRefundUrl { get; set; } = "Payment/RefundPayment";
}
