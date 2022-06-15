// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.ZarinPal
{
    public class ZarinPalGatewayOptions
    {
        public string ApiRequestUrl { get; set; } = "https://api.zarinpal.com/pg/v4/payment/request.json";

        public string ApiVerificationUrl { get; set; } = "https://api.zarinpal.com/pg/v4/payment/verify.json";

        public string ApiRefundUrl { get; set; } = "https://api.zarinpal.com/pg/v4/payment/refund.json";

        public string PaymentPageUrl { get; set; } = "https://www.zarinpal.com/pg/StartPay/";


        public string SandboxApiRequestUrl { get; set; } = "https://sandbox.zarinpal.com/pg/v4/payment/request.json";

        public string SandboxApiVerificationUrl { get; set; } = "https://sandbox.zarinpal.com/pg/v4/payment/verify.json";

        public string SandboxApiRefundUrl { get; set; } = "https://sandbox.zarinpal.com/pg/v4/payment/refund.json";

        public string SandboxPaymentPageUrl { get; set; } = "https://sandbox.zarinpal.com/pg/StartPay/";
    }
}
