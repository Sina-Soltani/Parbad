// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Internal
{
    public class PaymentVerifyingContext : IPaymentVerifyingContext
    {
        public long TrackingNumber { get; set; }

        public string GatewayName { get; set; }

        public bool IsCancelled { get; set; }

        public string CancellationReason { get; set; }

        public void CancelPayment(string reason = null)
        {
            IsCancelled = true;

            CancellationReason = reason;
        }
    }
}
