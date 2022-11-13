// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Parbad.Gateway.IranKish.Internal.Models
{
    internal class IranKishTokenRequest
    {
        public IranKishAuthenticationEnvelope AuthenticationEnvelope { get; set; }

        public IranKishTokenRequestInfo Request { get; set; }
    }

    internal class IranKishAuthenticationEnvelope
    {
        public string Data { get; set; }

        public string Iv { get; set; }
    }

    internal class IranKishTokenRequestInfo
    {
        public string AcceptorId { get; set; }

        public long Amount { get; set; }

        public IranKishBillInfo BillInfo { get; set; }

        public string CmsPreservationId { get; set; }

        public List<IranKishMultiplexParameter> MultiplexParameters { get; set; }

        public string PaymentId { get; set; }

        public string RequestId { get; set; }

        public long RequestTimestamp { get; set; }

        public string RevertUri { get; set; }

        public string TerminalId { get; set; }

        public string TransactionType { get; set; }

        public List<KeyValuePair<string, string>> AdditionalParameters { get; set; }

    }

    internal class IranKishBillInfo
    {
        public string BillId { get; set; }

        public string BillPaymentId { get; set; }
    }

    internal class IranKishMultiplexParameter
    {
        public string Iban { get; set; }

        public int Amount { get; set; }
    }

}
