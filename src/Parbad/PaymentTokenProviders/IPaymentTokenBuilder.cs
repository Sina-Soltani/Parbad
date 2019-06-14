// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Parbad.PaymentTokenProviders
{
    /// <summary>
    /// A builder for building the <see cref="IPaymentTokenProvider"/> services.
    /// </summary>
    public interface IPaymentTokenBuilder
    {
        /// <summary>
        /// Specifies the contract for a collection of service descriptors.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Adds a provider to the <see cref="IServiceCollection"/> with the given
        /// <paramref name="serviceLifetime"/>.
        /// </summary>
        /// <typeparam name="TProvider">Type of provider</typeparam>
        /// <param name="serviceLifetime">Specifies the lifetime of the <see cref="IPaymentTokenProvider"/>
        /// in an Microsoft.Extensions.DependencyInjection.IServiceCollection.</param>
        void AddPaymentTokenProvider<TProvider>(ServiceLifetime serviceLifetime)
            where TProvider : class, IPaymentTokenProvider;

        /// <summary>
        /// Adds a provider to the <see cref="IServiceCollection"/> using the given
        /// <paramref name="implementationFactory"/> and <paramref name="serviceLifetime"/>.
        /// </summary>
        /// <param name="implementationFactory">A factory to create new instances of the
        /// <see cref="IPaymentTokenProvider"/> implementation.</param>
        /// <param name="serviceLifetime">Specifies the lifetime of the <see cref="IPaymentTokenProvider"/>
        /// in an Microsoft.Extensions.DependencyInjection.IServiceCollection.</param>
        void AddPaymentTokenProvider(Func<IServiceProvider, IPaymentTokenProvider> implementationFactory,
            ServiceLifetime serviceLifetime);
    }
}
