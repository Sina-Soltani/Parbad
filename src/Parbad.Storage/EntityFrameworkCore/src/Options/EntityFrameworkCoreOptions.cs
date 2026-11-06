// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Parbad.Storage.EntityFrameworkCore.Context;

namespace Parbad.Storage.EntityFrameworkCore.Options;

/// <summary>
/// The options of Parbad's own <see cref="ParbadDataContext"/>, for applications that keep Parbad's tables in a
/// database of their own.
/// </summary>
public class EntityFrameworkCoreOptions : ParbadModelOptions
{
    /// <summary>
    /// Configures the <see cref="ParbadDataContext"/>: its database provider and connection, and, if needed, the
    /// migrations assembly and history table, e.g.
    /// <c>db => db.UseSqlServer(connectionString, sql => sql.MigrationsAssembly("MyApp.Migrations"))</c>.
    /// </summary>
    public Action<DbContextOptionsBuilder>? ConfigureDbContext { get; set; }
}
