// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Storage.Abstractions.Models;

/// <summary>
/// Type of transaction.
/// </summary>
public enum TransactionType : byte
{
    /// <summary>The payment request sent to the gateway.</summary>
    Request = 0,

    /// <summary>The verification of the payment with the gateway.</summary>
    Verify = 1,

    /// <summary>A refund of the payment.</summary>
    Refund = 2,

    /// <summary>The cancellation of the payment by the application (<c>IOnlinePayment.CancelAsync</c>).
    /// Before 3.12, cancellations were stored as <see cref="Verify"/>.</summary>
    Cancel = 3
}
