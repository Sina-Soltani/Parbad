// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Parbad.Storage.EntityFrameworkCore.Domain;

public class PaymentEntity
{
    public long Id { get; set; }

    public long TrackingNumber { get; set; }

    public decimal Amount { get; set; }

    public string Token { get; set; }

    public string TransactionCode { get; set; }

    public string GatewayName { get; set; }

    public string GatewayAccountName { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsPaid { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public List<TransactionEntity> Transactions { get; set; } = new List<TransactionEntity>();
}