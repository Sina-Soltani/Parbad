// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Gateway.PayIr.Internal
{
    internal class PayIrVerifyResponseModel
    {
        public long Amount { get; set; }

        public string Status { get; set; }

        public string TransId { get; set; }

        public string Message { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public bool IsSucceed => string.Equals(Status, "1", StringComparison.InvariantCultureIgnoreCase);
    }
}
