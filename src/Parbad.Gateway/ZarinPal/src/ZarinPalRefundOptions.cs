// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.ZarinPal;

/// <summary>
/// How ZarinPal pays a refund back to the customer.
/// </summary>
public enum ZarinPalRefundMethod
{
    /// <summary>In the next PAYA settlement cycle.</summary>
    Paya,

    /// <summary>Instantly to the customer's card.</summary>
    Card
}

/// <summary>
/// The reason of a refund, as ZarinPal records it.
/// </summary>
public enum ZarinPalRefundReason
{
    CustomerRequest,
    DuplicateTransaction,
    SuspiciousTransaction,
    Other
}
