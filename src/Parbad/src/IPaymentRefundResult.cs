// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad
{
    /// <summary>
    /// Describes the status of the Refund operation.
    /// </summary>
    public enum PaymentRefundResultStatus
    {
        /// <summary>
        /// The Verify operation was successful.
        /// </summary>
        Succeed,

        /// <summary>
        /// The Verify operation is failed.
        /// </summary>
        Failed,

        /// <summary>
        /// The invoice is already refunded before.
        /// </summary>
        AlreadyRefunded
    }

    /// <summary>
    /// Describes the result of the Refund operation.
    /// </summary>
    public interface IPaymentRefundResult : IPaymentResult
    {
        /// <summary>
        /// Gets the status of the Refund operation.
        /// </summary>
        PaymentRefundResultStatus Status { get; }
    }
}
