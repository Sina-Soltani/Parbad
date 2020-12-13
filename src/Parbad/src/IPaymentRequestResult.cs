// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad
{
    /// <summary>
    /// Describes the status of the requested invoice.
    /// </summary>
    public enum PaymentRequestResultStatus
    {
        /// <summary>
        /// Request was successful.
        /// </summary>
        Succeed,

        /// <summary>
        /// Request is failed.
        /// </summary>
        Failed,

        /// <summary>
        /// The tracking number is already exists or used before.
        /// </summary>
        TrackingNumberAlreadyExists
    }

    /// <summary>
    /// Describes the result of the Request operation.
    /// </summary>
    public interface IPaymentRequestResult : IPaymentResult
    {
        /// <summary>
        /// Gets the status of the requested invoice.
        /// </summary>
        PaymentRequestResultStatus Status { get; }

        /// <summary>
        /// Redirects the client to the gateway website.
        /// </summary>
        IGatewayTransporter GatewayTransporter { get; }
    }
}
