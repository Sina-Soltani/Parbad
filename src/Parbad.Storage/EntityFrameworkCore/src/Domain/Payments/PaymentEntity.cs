// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Parbad.Storage.Abstractions;
using Parbad.Storage.EntityFrameworkCore.Domain.Transactions;

namespace Parbad.Storage.EntityFrameworkCore.Domain.Payments
{
    public class PaymentEntity : Payment
    {
        public DateTime CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public List<TransactionEntity> Transactions { get; set; } = new List<TransactionEntity>();
    }
}
