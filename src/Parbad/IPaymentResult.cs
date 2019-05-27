// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Parbad
{
    /// <summary>
    /// Describes the result of a payment operation.
    /// </summary>
    public interface IPaymentResult
    {
        /// <summary>
        /// A unique number which will be used for tracking a specific payment.
        /// </summary>
        long TrackingNumber { get; }

        /// <summary>
        /// The amount of the payment.
        /// </summary>
        Money Amount { get; }

        /// <summary>
        /// Determines weather the payment was successful or not.
        /// </summary>
        bool IsSucceed { get; }

        /// <summary>
        /// Name of the gateway which the client has paid the payment in.
        /// </summary>
        string GatewayName { get; }

        /// <summary>
        /// Name of the gateway account.
        /// </summary>
        string GatewayAccountName { get; }

        /// <summary>
        /// A short message about the operation.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Includes some additional data.
        /// </summary>
        IDictionary<string, string> AdditionalData { get; }
    }
}
