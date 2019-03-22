// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Parbad.Options
{
    /// <summary>
    /// A builder for building Parbad options.
    /// </summary>
    public interface IParbadOptionsBuilder
    {
        /// <summary>
        /// Specifies the contract for a collection of service descriptors.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Adds the given <see cref="IParbadOptionsProvider{TOptions}"/> to <see cref="IServiceCollection"/>.
        /// It will be used for configuring the specified option.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <typeparam name="TOptionsProvider"></typeparam>
        /// <param name="optionsProviderServiceLifetime"></param>
        IParbadOptionsBuilder AddOptionsUsingProvider<TOptions, TOptionsProvider>(
            ServiceLifetime optionsProviderServiceLifetime)
            where TOptions : class, new()
            where TOptionsProvider : class, IParbadOptionsProvider<TOptions>;

        /// <summary>
        /// Adds the given <see cref="IParbadOptionsProvider{TOptions}"/> to <see cref="IServiceCollection"/>.
        /// It will be used for configuring the specified option.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="optionsProviderFactory"></param>
        /// <param name="optionsProviderServiceLifetime"></param>
        IParbadOptionsBuilder AddOptionsUsingProvider<TOptions>(
            Func<IServiceProvider, IParbadOptionsProvider<TOptions>> optionsProviderFactory,
            ServiceLifetime optionsProviderServiceLifetime)
            where TOptions : class, new();
    }
}
