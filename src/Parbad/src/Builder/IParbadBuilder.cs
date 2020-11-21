// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Parbad.Builder
{
    /// <summary>
    /// A builder for building the Parbad services.
    /// </summary>
    public interface IParbadBuilder
    {
        /// <summary>
        /// Specifies the contract for a collection of service descriptors.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Builds a single instance of <see cref="IOnlinePaymentAccessor"/> which provides an
        /// instance of <see cref="IOnlinePayment"/> from configured <see cref="IServiceProvider"/>.
        /// <para>
        /// You use this method if you don't use Microsoft.Extensions.DependencyInjection in your application.
        /// Otherwise in applications such as ASP.NET CORE that uses Microsoft.Extensions.DependencyInjection, 
        /// you don't need to use this method because that gets done for you.
        /// </para>
        /// <para>
        /// You can also register the <see cref="Services"/> in any other
        /// dependency injection tools such as Autofac and use Dependency Injection benefits in your application.
        /// For example you can inject the <see cref="IOnlinePayment"/> where ever you need.
        /// </para>
        /// </summary>
        IOnlinePaymentAccessor Build();
    }
}
