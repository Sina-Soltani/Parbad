// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Parbad.GatewayProviders.Mellat
{
    public class MellatGatewayOptions
    {
        public long TerminalId { get; set; }

        [Required(ErrorMessage = "User Name is required.", AllowEmptyStrings = false)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "User Password is required.", AllowEmptyStrings = false)]
        public string UserPassword { get; set; }

        /// <summary>
        /// Requests will be sent to the test terminal gateway of Mellat.
        /// </summary>
        public bool UseTestTerminal { get; set; }
    }
}
