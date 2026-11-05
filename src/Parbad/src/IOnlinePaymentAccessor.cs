// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Exceptions;

namespace Parbad
{
    /// <summary>
    /// Provides an instance of <see cref="IOnlinePayment"/> from configured <see cref="IServiceProvider"/>.
    /// </summary>
    public interface IOnlinePaymentAccessor
    {
        /// <summary>
        /// Defines a mechanism for retrieving a service object; that is, an object that
        /// provides custom support to other objects.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Gets an instance of <see cref="IOnlinePayment"/>.
        /// </summary>
        /// <exception cref="ParbadServiceNotInitializedException"></exception>
        [Obsolete("Resolves IOnlinePayment from the application's root service provider, so services meant to live for one request (e.g. the EF Core DbContext) are shared by all requests and never released. Use the CreateScope() extension method instead. This member will be removed in Parbad 4.0.")]
        IOnlinePayment OnlinePayment { get; }
    }
}
