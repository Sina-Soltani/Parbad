// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Abstraction;

namespace Parbad.Gateway.AsanPardakht
{
    public class AsanPardakhtGatewayAccount : GatewayAccount
    {
        public long MerchantConfigurationId { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
