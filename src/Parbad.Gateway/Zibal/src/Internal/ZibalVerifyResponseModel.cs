// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Gateway.Zibal.Internal
{
    internal class ZibalVerifyResponseModel
    {
        public DateTime? PaidAt { get; set; }

        public string? CardNumber { get; set; }

        public int? Status { get; set; }

        public int Amount { get; set; }

        public long? RefNumber { get; set; }

        public string? Description { get; set; }

        public string? OrderId { get; set; }

        public int Result { get; set; }

        public string? Message { get; set; }
    }
}
