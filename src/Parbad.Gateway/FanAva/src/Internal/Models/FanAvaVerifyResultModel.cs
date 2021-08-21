// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using System;

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaVerifyResultModel
    {
        public bool IsSucceed => !string.IsNullOrWhiteSpace(Result) &&
                                 Result.Equals("erSucceed", StringComparison.OrdinalIgnoreCase);

        public string Result { get; set; }

        public string Amount { get; set; }

        [JsonProperty("RefNum")]
        public string InvoiceNumber { get; set; }
    }
}
