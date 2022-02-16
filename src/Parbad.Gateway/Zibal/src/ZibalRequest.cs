// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Parbad.Gateway.Zibal
{
    public class ZibalRequest
    {
        public string? Description { get; set; }

        public string? CustomerMobile { get; set; }

        /// <summary>
        /// If true, the Payment's URL will be sent to Customer's mobile number.
        /// </summary>
        public bool SendSms { get; set; } = false;

        /// <summary>
        /// value should be between 0 - 2
        /// </summary>
        /// <value>0 - Deduction from the transaction | 1 - Deduction from the wallet | 2 - Add to payment amount</value>
        public int FeeMode { get; set; }

        public List<string>? AllowedCards { get; set; } = new();

        public bool LinkToPay { get; set; } = false;
    }
}
