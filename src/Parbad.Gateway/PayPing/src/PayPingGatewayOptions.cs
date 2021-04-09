// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.PayPing
{
    public class PayPingGatewayOptions
    {
        /// <summary>
        /// https://api.payping.ir/v2/pay
        /// </summary>
        public string ApiRequestUrl { get; set; } = "https://api.payping.ir/v2/pay";

        /// <summary>
        /// https://api.payping.ir/v2/pay/verify
        /// </summary>
        public string ApiVerificationUrl { get; set; } = "https://api.payping.ir/v2/pay/verify";

        /// <summary>
        /// https://api.payping.ir/v2/pay/gotoipg/{Code}
        /// </summary>
        public string PaymentPageUrl { get; set; } = "https://api.payping.ir/v2/pay/gotoipg";
    }
}
