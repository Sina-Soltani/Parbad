// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Storage.EntityFrameworkCore.Configuration;

/// <summary>
/// Column names of the payment table that raw SQL refers to, shared with <see cref="PaymentConfiguration"/> so they
/// cannot drift apart.
/// </summary>
internal static class PaymentColumnNames
{
    public const string Id = "payment_id";

    public const string TransactionCode = "transaction_code";

    public const string IsCompleted = "is_completed";

    public const string IsPaid = "is_paid";

    public const string UpdatedOn = "updated_on";
}
