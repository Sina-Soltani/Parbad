// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.GatewayProviders.Melli.Models
{
    [Serializable]
    internal class MelliApiVerifyResult
    {
        public int? ResCode { get; set; }

        public long Amount { get; set; }

        public string Description { get; set; }

        public string RetrivalRefNo { get; set; }

        public string SystemTraceNo { get; set; }

        public long OrderId { get; set; }
    }
}