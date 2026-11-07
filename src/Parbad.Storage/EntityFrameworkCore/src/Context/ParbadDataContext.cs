// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Parbad.Storage.EntityFrameworkCore.Domain;
using Parbad.Storage.EntityFrameworkCore.Internal;
using Parbad.Storage.EntityFrameworkCore.Options;

namespace Parbad.Storage.EntityFrameworkCore.Context;

/// <summary>
/// Parbad's own <see cref="DbContext"/>, for applications that keep Parbad's tables in a database of their own.
/// Applications that keep them in their own database call <c>modelBuilder.UseParbad()</c> in their own context instead.
/// </summary>
public class ParbadDataContext : DbContext
{
    /// <summary>
    /// Initializes an instance of <see cref="ParbadDataContext"/>. The table options come with
    /// <paramref name="options"/> (see <c>UseEfCore</c>); without them, the defaults are used.
    /// </summary>
    public ParbadDataContext(DbContextOptions<ParbadDataContext> options) : base(options)
    {
        ModelOptions = options.FindExtension<ParbadModelOptionsExtension>()?.ModelOptions ?? new ParbadModelOptions();
    }

    /// <summary>
    /// The tables of Parbad's entities.
    /// </summary>
    public ParbadModelOptions ModelOptions { get; }

    public DbSet<PaymentEntity> Payments { get; set; } = null!;

    public DbSet<TransactionEntity> Transactions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseParbad(ModelOptions);
    }
}
