// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Parbad.Options;

namespace Parbad.Internal
{
    /// <inheritdoc />
    public class ParbadOptionsBuilder : IParbadOptionsBuilder
    {
        /// <summary>
        /// Initializes an instance of <see cref="ParbadOptionsBuilder"/>.
        /// </summary>
        /// <param name="services"></param>
        public ParbadOptionsBuilder(IServiceCollection services)
        {
            Services = services;
        }

        /// <inheritdoc />
        public IServiceCollection Services { get; }

        /// <inheritdoc />
        public IParbadOptionsBuilder AddOptionsUsingProvider<TOptions, TOptionsProvider>(
            ServiceLifetime optionsProviderServiceLifetime)
            where TOptions : class, new()
            where TOptionsProvider : class, IParbadOptionsProvider<TOptions>
        {
            Services.TryAddTransient<IOptions<TOptions>, OptionsManager<TOptions>>();

            Services.TryAdd<IParbadOptionsProvider<TOptions>, TOptionsProvider>(optionsProviderServiceLifetime);

            Services.AddTransient<IConfigureOptions<TOptions>, ConfigureFromOptionsProvider<TOptions>>();

            return this;
        }

        /// <inheritdoc />
        public IParbadOptionsBuilder AddOptionsUsingProvider<TOptions>(
            Func<IServiceProvider, IParbadOptionsProvider<TOptions>> optionsProviderFactory,
            ServiceLifetime optionsProviderServiceLifetime)
            where TOptions : class, new()
        {
            Services.TryAdd(ServiceDescriptor.Describe(
                typeof(IOptions<TOptions>),
                typeof(OptionsManager<TOptions>),
                ServiceLifetime.Singleton));

            Services.TryAdd(ServiceDescriptor.Describe(
                typeof(IParbadOptionsProvider<TOptions>),
                optionsProviderFactory,
                optionsProviderServiceLifetime));

            Services.AddTransient<IConfigureOptions<TOptions>, ConfigureFromOptionsProvider<TOptions>>();

            return this;
        }
    }
}
