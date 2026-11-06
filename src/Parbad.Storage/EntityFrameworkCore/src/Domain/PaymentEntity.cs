// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.Storage.EntityFrameworkCore.Domain;

public class PaymentEntity
{
    public long Id { get; set; }

    public long TrackingNumber { get; set; }

    public decimal Amount { get; set; }

    public string Token { get; set; } = null!;

    public string? TransactionCode { get; set; }

    public string GatewayName { get; set; } = null!;

    public string? GatewayAccountName { get; set; }

    /// <summary>
    /// The reference the bank verified. Unique per gateway among the payments that have one.
    /// </summary>
    public string? BankReference { get; set; }

    public PaymentStatus Status { get; set; }

    public decimal RefundedAmount { get; set; }

    /// <summary>
    /// Kept in step with <see cref="Status"/>, for queries and reports written before Parbad 4.0.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Kept in step with <see cref="Status"/>, for queries and reports written before Parbad 4.0.
    /// </summary>
    public bool IsPaid { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public List<TransactionEntity> Transactions { get; set; } = new List<TransactionEntity>();
}