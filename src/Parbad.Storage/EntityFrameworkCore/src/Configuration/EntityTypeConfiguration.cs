// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parbad.Storage.EntityFrameworkCore.Options;

namespace Parbad.Storage.EntityFrameworkCore.Configuration;

/// <summary>
/// Applies the configuration on the specified entity.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public abstract class EntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class
{
    /// <summary>
    /// Initializes an instance of <see cref="EntityTypeConfiguration{TEntity}"/>.
    /// </summary>
    protected EntityTypeConfiguration(ParbadModelOptions modelOptions)
    {
        ModelOptions = modelOptions;
    }

    /// <summary>
    /// The tables of Parbad's entities.
    /// </summary>
    public ParbadModelOptions ModelOptions { get; }

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        Configure(builder, ModelOptions);
    }

    /// <summary>
    /// Configures the entity.
    /// </summary>
    public abstract void Configure(EntityTypeBuilder<TEntity> builder, ParbadModelOptions modelOptions);
}