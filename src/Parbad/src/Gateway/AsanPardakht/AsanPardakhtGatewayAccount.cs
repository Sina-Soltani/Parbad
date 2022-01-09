// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Abstraction;

namespace Parbad.Gateway.AsanPardakht
{
    public class AsanPardakhtGatewayAccount : GatewayAccount
    {
        public string MerchantConfigurationId { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        /// <summary>
        /// The Key for encrypting/decrypting the data. You can ask Asan Pardakht Support to receive your key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The IV for encrypting/decrypting the data. You can ask Asan Pardakht Support to receive your IV.
        /// </summary>
        public string IV { get; set; }
    }
}
