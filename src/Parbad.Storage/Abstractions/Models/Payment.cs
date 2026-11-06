// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Storage.Abstractions.Models;

[Serializable]
public class Payment
{
    public long Id { get; set; }

    public long TrackingNumber { get; set; }

    public decimal Amount { get; set; }

    public string Token { get; set; } = null!;

    /// <summary>
    /// The transaction code shown to the customer (e.g. the RRN or the bank's tracking code).
    /// </summary>
    public string? TransactionCode { get; set; }

    /// <summary>
    /// The reference the bank verified for this payment. Within a gateway, one bank reference settles only one payment.
    /// </summary>
    public string? BankReference { get; set; }

    public string GatewayName { get; set; } = null!;

    public string? GatewayAccountName { get; set; }

    /// <summary>
    /// The state of the payment.
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// The amount refunded so far.
    /// </summary>
    public decimal RefundedAmount { get; set; }

    /// <summary>
    /// Whether the payment is completed, regardless of whether it is paid or not (derived from <see cref="Status"/>).
    /// </summary>
    public bool IsCompleted => Status != PaymentStatus.Requested;

    /// <summary>
    /// Whether the bank confirmed the payment, including payments refunded later (derived from <see cref="Status"/>).
    /// </summary>
    public bool IsPaid => Status is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded or PaymentStatus.Refunded;
}
