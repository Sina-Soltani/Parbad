// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaRequestModel
    {
        public WSContextModel WSContext { get; set; }

        public string TransType { get; set; }

        public string ReserveNum { get; set; }

        public long Amount { get; set; }

        public string RedirectUrl { get; set; }

        public string MobileNo { get; set; }

        public string Email { get; set; }

        public string GoodsReferenceId { get; set; }

        public string MerchantGoodReferenceId { get; set; }

        public IEnumerable<FanAvaGatewayApportionmentAccount> ApportionmentAccountList { get; set; }

        internal class WSContextModel
        {
            public string UserId { get; set; }

            public string Password { get; set; }
        }
    }
}
