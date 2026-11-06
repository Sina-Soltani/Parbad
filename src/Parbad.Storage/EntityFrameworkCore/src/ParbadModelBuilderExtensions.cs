// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Storage.EntityFrameworkCore.Configuration;
using Parbad.Storage.EntityFrameworkCore.Options;

// In EF Core's namespace, like other libraries' model extensions, so OnModelCreating needs no extra using.
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Adds Parbad's entities to an application's own <see cref="DbContext"/>.
/// </summary>
public static class ParbadModelBuilderExtensions
{
    /// <summary>
    /// Adds Parbad's payment and transaction entities to the model. Call it in <c>OnModelCreating</c> of the context
    /// that is registered with <c>UseEfCore&lt;TContext&gt;()</c>:
    /// <code>
    /// protected override void OnModelCreating(ModelBuilder modelBuilder)
    /// {
    ///     base.OnModelCreating(modelBuilder);
    ///     modelBuilder.UseParbad();
    /// }
    /// </code>
    /// By default the tables are "payment" and "transaction" in the schema "parbad".
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <param name="configure">Changes the table names or the schema.</param>
    public static ModelBuilder UseParbad(this ModelBuilder modelBuilder, Action<ParbadModelOptions>? configure = null)
    {
        var options = new ParbadModelOptions();
        configure?.Invoke(options);

        return modelBuilder.UseParbad(options);
    }

    /// <summary>
    /// Adds Parbad's payment and transaction entities to the model, with the given table options.
    /// </summary>
    public static ModelBuilder UseParbad(this ModelBuilder modelBuilder, ParbadModelOptions options)
    {
        if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
        if (options == null) throw new ArgumentNullException(nameof(options));

        return modelBuilder.ApplyConfiguration(new PaymentConfiguration(options))
                           .ApplyConfiguration(new TransactionConfiguration(options));
    }
}
