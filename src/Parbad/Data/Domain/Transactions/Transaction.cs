// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Data.Domain.Payments;

namespace Parbad.Data.Domain.Transactions
{
    public class Transaction : BaseEntity
    {
        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }

        public bool IsSucceed { get; set; }

        public string Message { get; set; }

        public string AdditionalData { get; set; }

        public long PaymentId { get; set; }
        public Payment Payment { get; set; }
    }
}
