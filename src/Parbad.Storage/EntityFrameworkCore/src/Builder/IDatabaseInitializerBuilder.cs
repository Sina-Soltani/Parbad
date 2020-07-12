// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Storage.EntityFrameworkCore.Initializers;

namespace Parbad.Storage.EntityFrameworkCore.Builder
{
    [Obsolete("Database Initializers are not supported anymore and will be removed in a future version.")]
    public interface IDatabaseInitializerBuilder
    {
        /// <summary>
        /// Provides solution to perform payment request, verify the requested payment and
        /// refund a payment.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Adds the specified <typeparamref name="TInitializer"/> to Parbad services.
        /// Initializers will be called in order that you specified.
        /// </summary>
        /// <typeparam name="TInitializer"></typeparam>
        /// <param name="initializerLifetime">The lifetime of the specified <typeparamref name="TInitializer"/>.</param>
        [Obsolete("Database Initializers are not supported anymore and will be removed in a future version.")]
        IDatabaseInitializerBuilder AddInitializer<TInitializer>(
            ServiceLifetime initializerLifetime = ServiceLifetime.Transient)
            where TInitializer : class, IDatabaseInitializer;

        /// <summary>
        /// Adds the specified Initializer to Parbad services.
        /// Initializers will be called in order that you specified.
        /// </summary>
        /// <param name="initializer"></param>
        [Obsolete("Database Initializers are not supported anymore and will be removed in a future version.")]
        IDatabaseInitializerBuilder AddInitializer(IDatabaseInitializer initializer);

        /// <summary>
        /// Adds the specified Initializer to Parbad services.
        /// Initializers will be called in order that you specified.
        /// </summary>
        /// <param name="implementationFactory"></param>
        /// <param name="initializerLifetime">The lifetime of the specified Initializer.</param>
        [Obsolete("Database Initializers are not supported anymore and will be removed in a future version.")]
        IDatabaseInitializerBuilder AddInitializer(
            Func<IServiceProvider, IDatabaseInitializer> implementationFactory,
            ServiceLifetime initializerLifetime = ServiceLifetime.Transient);
    }
}
