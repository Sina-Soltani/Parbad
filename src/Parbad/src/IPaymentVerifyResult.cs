// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

#nullable enable

namespace Parbad;

/// <summary>
/// Describes the status of the Verify operation.
/// </summary>
public enum PaymentVerifyResultStatus
{
    /// <summary>
    /// The Verify operation was successful.
    /// </summary>
    Succeed,

    /// <summary>
    /// The Verify operation is failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The invoice has been verified before.
    /// </summary>
    AlreadyVerified
}

/// <summary>
/// Describes the result of the Verify operation.
/// </summary>
public interface IPaymentVerifyResult : IPaymentResult
{
    /// <summary>
    /// Gets the transaction code from the gateway.
    /// </summary>
    string? TransactionCode { get; }

    /// <summary>
    /// Gets the status of the Verify operation.
    /// </summary>
    PaymentVerifyResultStatus Status { get; }

    /// <summary>
    /// The reference the bank verified (e.g. Saman's RefNum, Sepehr's digital receipt), which can differ from the
    /// customer-facing <see cref="TransactionCode"/>. Within a gateway, one bank reference settles only one payment.
    /// </summary>
    string? BankReference { get; }

    /// <summary>
    /// The amount the bank reported for the verified transaction, or null when the bank does not report it. When set,
    /// Parbad accepts a successful verification only if it equals the amount of the invoice.
    /// </summary>
    Money? BankReportedAmount { get; }

    /// <summary>
    /// The order id (tracking number) the bank reported for the verified transaction, or null when the bank does not
    /// report it. When set, Parbad accepts a successful verification only if it equals the invoice's tracking number.
    /// </summary>
    string? BankReportedOrderId { get; }

    /// <summary>
    /// True when the bank answered "already verified" and the gateway cannot prove from the bank's answer that the
    /// payment belongs to this invoice. Parbad then records it as paid only if no other paid payment has the same
    /// <see cref="BankReference"/>.
    /// </summary>
    bool IsAlreadyVerifiedAtBank { get; }
}