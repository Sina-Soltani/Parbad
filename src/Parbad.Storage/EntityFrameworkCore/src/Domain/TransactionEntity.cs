// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Storage.Abstractions.Models;
using System;

namespace Parbad.Storage.EntityFrameworkCore.Domain
{
    public class TransactionEntity
    {
        public long Id { get; set; }

        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }

        public bool IsSucceed { get; set; }

        public string Message { get; set; }

        public string AdditionalData { get; set; }

        public long PaymentId { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public PaymentEntity Payment { get; set; }
    }
}
