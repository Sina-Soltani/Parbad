// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Parbad.Gateway.Tara.Internal.Models
{
    internal class TaraInquiryResponse
    {
        public string result { get; set; }
        public string description { get; set; }
        public string doTime { get; set; }
        public List<TaraTrackPurchase> trackPurchaseList { get; set; }
        public string orderId { get; set; }
    }
}



