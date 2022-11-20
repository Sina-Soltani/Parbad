// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.IranKish
{
    public class IranKishGatewayAdditionalData
    {
        /// <summary>
        /// To enable the card-recall feature of the gateway, set this additional data with user's
        /// mobile-phone number (like "989123456789") or email.
        /// </summary>
        public string MobileNumberOrEmail { get; set; }
    }
}
