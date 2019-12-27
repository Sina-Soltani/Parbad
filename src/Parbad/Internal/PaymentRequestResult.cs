// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Internal
{
    public class PaymentRequestResult : PaymentResult, IPaymentRequestResult
    {
        public IGatewayTransporter GatewayTransporter { get; set; }

        public static PaymentRequestResult Succeed(IGatewayTransporter gatewayTransporter, string gatewayAccountName)
        {
            return new PaymentRequestResult
            {
                IsSucceed = true,
                GatewayAccountName = gatewayAccountName,
                GatewayTransporter = gatewayTransporter
            };
        }

        public static PaymentRequestResult Failed(string message, string gatewayAccountName = null)
        {
            return new PaymentRequestResult
            {
                IsSucceed = false,
                Message = message,
                GatewayAccountName = gatewayAccountName,
                GatewayTransporter = new NullGatewayTransporter()
            };
        }
    }
}
