// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Parbad.Internal
{
    public class PaymentResult : IPaymentResult
    {
        public PaymentResult()
        {
            AdditionalData = new Dictionary<string, string>();
            DatabaseAdditionalData = new Dictionary<string, string>();
        }

        public long TrackingNumber { get; set; }

        public Money Amount { get; set; }

        public bool IsSucceed { get; set; }

        public string GatewayName { get; set; }

        public string Message { get; set; }

        public IDictionary<string, string> AdditionalData { get; protected set; }

        public IDictionary<string, string> DatabaseAdditionalData { get; protected set; }
    }
}
