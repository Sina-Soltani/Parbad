// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Parbad.GatewayProviders.Pasargad
{
    public class PasargadGatewayOptions
    {
        [Required(ErrorMessage = "MerchantCode is required.", AllowEmptyStrings = false)]
        public string MerchantCode { get; set; }

        [Required(ErrorMessage = "TerminalCode is required.", AllowEmptyStrings = false)]
        public string TerminalCode { get; set; }

        [Required(ErrorMessage = "PrivateKey is required.", AllowEmptyStrings = false)]
        [PasargadPrivateKeyValidation(ErrorMessage = "Pasargad Private Key is not a valid XML. Please check your Private Key again.")]
        public string PrivateKey { get; set; }
    }
}
