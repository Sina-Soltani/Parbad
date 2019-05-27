// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Parbad.Internal
{
    /// <inheritdoc />
    public class PaymentResult : IPaymentResult
    {
        /// <summary>
        /// Initializes an instance of <see cref="PaymentResult"/>.
        /// </summary>
        public PaymentResult()
        {
            AdditionalData = new Dictionary<string, string>();
            DatabaseAdditionalData = new Dictionary<string, string>();
        }

        /// <inheritdoc />
        public long TrackingNumber { get; set; }

        /// <inheritdoc />
        public Money Amount { get; set; }

        /// <inheritdoc />
        public bool IsSucceed { get; set; }

        /// <inheritdoc />
        public string GatewayName { get; set; }

        /// <inheritdoc />
        public string GatewayAccountName { get; set; }

        /// <inheritdoc />
        public string Message { get; set; }

        /// <inheritdoc />
        public IDictionary<string, string> AdditionalData { get; protected set; }

        public IDictionary<string, string> DatabaseAdditionalData { get; protected set; }
    }
}
