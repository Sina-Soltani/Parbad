// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.YekPay.Internal
{
    internal class YekPayRequestModel : YekPayRequest
    {
        public long MerchantId { get; set; }

        public decimal Amount { get; set; }

        public long OrderNumber { get; set; }

        public string Callback { get; set; }
    }
}
