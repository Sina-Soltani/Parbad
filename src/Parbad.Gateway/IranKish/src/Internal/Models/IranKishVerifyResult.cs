// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.IranKish.Internal.Models
{
    internal class IranKishVerifyResult
    {
        public string ResponseCode { get; set; }

        public string Description { get; set; }

        public bool Status { get; set; }

        public IranKishVerifyResultInfo Result { get; set; }
    }

    internal class IranKishVerifyResultInfo
    {
        public string ResponseCode { get; set; }

        public string SystemTraceAuditNumber { get; set; }

        public string RetrievalReferenceNumber { get; set; }

        public int TransactionDate { get; set; }

        public int TransactionTime { get; set; }

        public string ProcessCode { get; set; }

        public string BillType { get; set; }

        public string BillId { get; set; }

        public string PaymentId { get; set; }

        public string Amount { get; set; }
    }
}
