// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Parbad.Data.Domain.Transactions;

namespace Parbad.Data.Domain.Payments
{
    public class Payment : BaseEntity
    {
        public long TrackingNumber { get; set; }

        public decimal Amount { get; set; }

        public string Token { get; set; }

        public string TransactionCode { get; set; }

        public string GatewayName { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsPaid { get; set; }

        public string GatewayAccountName { get; set; }

        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
