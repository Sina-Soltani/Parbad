// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Parbad.Gateway.Zibal
{
    public class ZibalRequestAdditionalData
    {
        public string? Description { get; set; }

        [JsonProperty("mobile")]
        public string? MobileNumber { get; set; }

        /// <summary>
        /// If true, the Payment's URL will be sent to the specified mobile number via SMS.
        /// </summary>
        [JsonProperty("sms")]
        public bool SendPaymentLinkViaSms { get; set; } = false;

        /// <summary>
        /// Value should be between 0 and 2.
        /// </summary>
        /// <value>0 - Deduction from the transaction | 1 - Deduction from the wallet | 2 - Add to payment amount</value>
        public int FeeMode { get; set; }

        /// <summary>
        /// Forces the client to pay with one of the specified cards.
        /// </summary>
        public List<string>? AllowedCards { get; set; } = new();

        public bool LinkToPay { get; set; } = false;
    }
}
