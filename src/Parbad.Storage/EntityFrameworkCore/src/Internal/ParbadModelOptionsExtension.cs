// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Storage.EntityFrameworkCore.Options;

namespace Parbad.Storage.EntityFrameworkCore.Internal;

/// <summary>
/// Carries the table options of <see cref="Context.ParbadDataContext"/> in its <c>DbContextOptions</c>, because a
/// context created by <c>IDbContextFactory</c> receives nothing else.
/// </summary>
internal sealed class ParbadModelOptionsExtension : IDbContextOptionsExtension
{
    public ParbadModelOptionsExtension(ParbadModelOptions modelOptions)
    {
        ModelOptions = modelOptions;
        Info = new ExtensionInfo(this);
    }

    public ParbadModelOptions ModelOptions { get; }

    public DbContextOptionsExtensionInfo Info { get; }

    public void ApplyServices(IServiceCollection services)
    {
    }

    public void Validate(IDbContextOptions options)
    {
    }

    private sealed class ExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "using Parbad's model options ";

        public override int GetServiceProviderHashCode() => 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is ExtensionInfo;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
        }
    }
}
