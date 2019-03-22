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
        IOnlinePayment OnlinePayment { get; }
    }
}
