// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaCheckRequestModel
    {
        public FanAvaRequestModel.WSContextModel WSContext { get; set; }

        public string Token { get; set; }
    }
}
