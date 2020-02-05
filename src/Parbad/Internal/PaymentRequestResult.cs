// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Internal
{
    public class PaymentRequestResult : PaymentResult, IPaymentRequestResult
    {
        /// <inheritdoc />
        public PaymentRequestResultStatus Status { get; set; }

        /// <inheritdoc />
        public IGatewayTransporter GatewayTransporter { get; set; }

        public override bool IsSucceed => Status == PaymentRequestResultStatus.Succeed;

        public static PaymentRequestResult Succeed(IGatewayTransporter gatewayTransporter, string gatewayAccountName)
        {
            return new PaymentRequestResult
            {
                GatewayAccountName = gatewayAccountName,
                GatewayTransporter = gatewayTransporter,
                Status = PaymentRequestResultStatus.Succeed
            };
        }

        public static PaymentRequestResult Failed(string message, string gatewayAccountName = null)
        {
            return new PaymentRequestResult
            {
                Status = PaymentRequestResultStatus.Failed,
                Message = message,
                GatewayAccountName = gatewayAccountName,
                GatewayTransporter = new NullGatewayTransporter()
            };
        }
    }
}
