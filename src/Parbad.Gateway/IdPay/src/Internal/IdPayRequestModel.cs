﻿// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.IdPay.Internal
{
    internal class IdPayRequestModel
    {
        [JsonProperty("order_id")]
        public long OrderId { get; set; }

        public long Amount { get; set; }

        public string Callback { get; set; }
    }
}
