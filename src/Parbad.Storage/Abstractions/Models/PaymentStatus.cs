// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Storage.Abstractions.Models;

/// <summary>
/// The state of a payment: <see cref="Requested"/>, then <see cref="Paid"/>, <see cref="Failed"/> or
/// <see cref="Cancelled"/>, and after refunds <see cref="PartiallyRefunded"/> or <see cref="Refunded"/>.
/// </summary>
public enum PaymentStatus : byte
{
    /// <summary>The payment was requested and is not completed yet.</summary>
    Requested = 0,

    /// <summary>The bank confirmed the payment.</summary>
    Paid = 1,

    /// <summary>The payment failed: the request or the verification was rejected.</summary>
    Failed = 2,

    /// <summary>The application cancelled the payment (<c>IOnlinePayment.CancelAsync</c>).</summary>
    Cancelled = 3,

    /// <summary>A part of the paid amount was refunded.</summary>
    PartiallyRefunded = 4,

    /// <summary>The whole paid amount was refunded.</summary>
    Refunded = 5
}
