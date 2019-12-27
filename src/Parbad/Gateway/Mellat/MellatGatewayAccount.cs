// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Parbad.Abstraction;

namespace Parbad.Gateway.Mellat
{
    public class MellatGatewayAccount : GatewayAccount
    {
        public long TerminalId { get; set; }

        [Required(ErrorMessage = "User Name is required.", AllowEmptyStrings = false)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "User Password is required.", AllowEmptyStrings = false)]
        public string UserPassword { get; set; }

        /// <summary>
        /// The requests will be sent to the test terminal of Mellat Gateway.
        /// </summary>
        public bool IsTestTerminal { get; set; }
    }
}
