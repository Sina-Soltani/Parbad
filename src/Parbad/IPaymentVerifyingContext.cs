// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad
{
    /// <summary>
    /// Describes the invoice which sent by the gateway. You can compare its data with your
    /// database and also cancel the payment operation if you need.
    /// </summary>
    public interface IPaymentVerifyingContext
    {
        /// <summary>
        /// Gets the TrackingNumber of requested payment.
        /// </summary>
        long TrackingNumber { get; }

        /// <summary>
        /// Gets the gateway's name of requested payment.
        /// </summary>
        string GatewayName { get; }

        /// <summary>
        /// Cancel the Verify operation. No Verifying request will be sent to the gateway.
        /// In this case, the money will transferred back to the client's bank account after
        /// about 15-60 minutes.
        /// </summary>
        /// <param name="reason">The reason for cancelling the operation. It will be saved in
        /// Message field in database.</param>
        void CancelPayment(string reason = null);
    }
}
