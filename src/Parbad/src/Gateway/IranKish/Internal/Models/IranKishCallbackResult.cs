// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.IranKish.Internal.Models
{
    internal class IranKishCallbackResult
    {
        public bool IsSucceed { get; set; }

        public string Message { get; set; }

        public string Token { get; set; }

        public string AcceptorId { get; set; }

        public string ResponseCode { get; set; }

        public string PaymentId { get; set; }

        public string RequestId { get; set; }

        public string Sha256OfPan { get; set; }

        public string RetrievalReferenceNumber { get; set; }

        public string Amount { get; set; }

        public string MaskedPan { get; set; }

        public string SystemTraceAuditNumber { get; set; }
    }
}
