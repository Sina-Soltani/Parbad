// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Parbad.Exceptions;
using Parbad.TrackingNumberProviders;

namespace Parbad.Abstraction
{
    public class Invoice
    {
        public Invoice(long trackingNumber, Money amount, CallbackUrl callbackUrl, Type gatewayType, IDictionary<string, object> additionalData = null)
        {
            if (trackingNumber <= 0)
            {
                throw new InvalidTrackingNumberException(trackingNumber);
            }

            TrackingNumber = trackingNumber;
            Amount = amount;
            CallbackUrl = callbackUrl;
            GatewayType = gatewayType;
            AdditionalData = additionalData ?? new Dictionary<string, object>();
        }

        public long TrackingNumber { get; }

        public Money Amount { get; }

        public CallbackUrl CallbackUrl { get; }

        public Type GatewayType { get; }

        public IDictionary<string, object> AdditionalData { get; }
    }
}
