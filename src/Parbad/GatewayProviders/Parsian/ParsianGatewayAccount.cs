// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Parbad.Abstraction;

namespace Parbad.GatewayProviders.Parsian
{
    public class ParsianGatewayAccount : GatewayAccount
    {
        [Required(ErrorMessage = "LoginAccount is required.", AllowEmptyStrings = false)]
        public string LoginAccount { get; set; }
    }
}
