// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Parbad.Abstraction;

namespace Parbad.GatewayProviders.Saman
{
    public class SamanGatewayAccount : GatewayAccount
    {
        [Required(ErrorMessage = "MerchantId is required.", AllowEmptyStrings = false)]
        public string MerchantId { get; set; }

        [Required(ErrorMessage = "Password is required.", AllowEmptyStrings = false)]
        public string Password { get; set; }
    }
}
