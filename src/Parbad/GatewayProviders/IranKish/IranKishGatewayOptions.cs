// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Parbad.GatewayProviders.IranKish
{
    /// <summary>
    /// IranKish gateway options
    /// </summary>
    public class IranKishGatewayOptions
    {
        [Required(ErrorMessage = "MerchantId is required.", AllowEmptyStrings = false)]
        public string MerchantId { get; set; }
    }
}
