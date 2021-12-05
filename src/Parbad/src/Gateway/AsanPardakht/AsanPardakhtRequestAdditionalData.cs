// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Parbad.Gateway.AsanPardakht
{
    public class AsanPardakhtRequestAdditionalData
    {
        public string MobileNumber { get; set; }

        public string PaymentId { get; set; }

        public string AdditionalData { get; set; }

        public List<AsanPardakhtSettlementPortionModel> SettlementPortions { get; set; } = new();
    }
}
