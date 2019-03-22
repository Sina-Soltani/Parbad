// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Parbad.GatewayProviders.Melli
{
    /// <summary>
    /// Melli Gateway options class.
    /// </summary>
    public class MelliGatewayOptions
    {
        [Required(ErrorMessage = "TerminalId is required.", AllowEmptyStrings = false)]
        public string TerminalId { get; set; }

        [Required(ErrorMessage = "MerchantId is required.", AllowEmptyStrings = false)]
        public string MerchantId { get; set; }

        [Required(ErrorMessage = "TerminalKey is required.", AllowEmptyStrings = false)]
        public string TerminalKey { get; set; }
    }
}
