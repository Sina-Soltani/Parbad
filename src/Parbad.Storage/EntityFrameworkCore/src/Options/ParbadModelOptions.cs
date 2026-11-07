// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Storage.EntityFrameworkCore.Options;

/// <summary>
/// The tables of Parbad's entities in an EF Core model.
/// </summary>
public class ParbadModelOptions
{
    /// <summary>
    /// Gets or sets the schema of the tables that do not set their own. The default value is "parbad".
    /// </summary>
    public string? DefaultSchema { get; set; } = "parbad";

    /// <summary>
    /// The payment table. Its default name is "payment".
    /// </summary>
    public TableOptions PaymentTableOptions { get; set; } = new() { Name = "payment" };

    /// <summary>
    /// The transaction table. Its default name is "transaction".
    /// </summary>
    public TableOptions TransactionTableOptions { get; set; } = new() { Name = "transaction" };
}
