using System;
using Microsoft.EntityFrameworkCore;

namespace Parbad.Storage.EntityFrameworkCore.Tests;

/// <summary>Creates contexts with the given delegate, like the factory that AddDbContextFactory registers.</summary>
internal sealed class TestDbContextFactory<TContext>(Func<TContext> create) : IDbContextFactory<TContext> where TContext : DbContext
{
    public TContext CreateDbContext() => create();
}
