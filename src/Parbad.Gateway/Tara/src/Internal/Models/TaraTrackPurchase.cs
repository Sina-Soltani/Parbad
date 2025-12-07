// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Parbad.Gateway.Tara.Internal.Models
{
    internal class TaraTrackPurchase
    {
        public string token { get; set; }
        public string result { get; set; }
        public string description { get; set; }
        public string doTime { get; set; }
        public List<TaraServiceAmount> serviveAmountList { get; set; }
        public string amount { get; set; }
        public string rrn { get; set; }
        public string type { get; set; }
    }
}



