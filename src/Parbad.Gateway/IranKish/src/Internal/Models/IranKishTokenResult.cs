// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.IranKish.Internal.Models
{
    internal class IranKishTokenResult
    {
        public string ResponseCode { get; set; }

        public object Description { get; set; }

        public bool Status { get; set; }

        public IranKishTokenResultInfo Result { get; set; }
    }

    internal class IranKishTokenResultInfo
    {
        public string Token { get; set; }

        public int InitiateTimeStamp { get; set; }

        public int ExpiryTimeStamp { get; set; }

        public string TransactionType { get; set; }

        public IranKishBillInfo BillInfo { get; set; }
    }
}
