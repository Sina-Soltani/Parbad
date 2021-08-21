// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaRequestResultModel
    {
        public bool IsSucceed => !string.IsNullOrEmpty(Result) &&
                                 Result.Equals("erSucceed", StringComparison.OrdinalIgnoreCase);

        public string Result { get; set; }

        public string ExpirationDate { get; set; }

        public string Token { get; set; }

        public string ChannelId { get; set; }

        public string UserId { get; set; }
    }
}
