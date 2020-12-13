// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad
{
    /// <summary>
    /// Provides a static version of <see cref="IOnlinePayment"/>.
    /// <para> You use this method when using no dependency injections such as
    /// Microsoft.Extensions.DependencyInjection in your application,
    /// otherwise you don't need to use this and can inject the <see cref="IOnlinePayment"/>
    /// interface wherever you need.
    /// </para>
    /// </summary>
    public static class StaticOnlinePayment
    {
        private static IOnlinePaymentAccessor _parbadAccessor;

        /// <summary>
        /// Gets a new instance of <see cref="IOnlinePayment"/> from the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public static IOnlinePayment Instance
        {
            get
            {
                if (_parbadAccessor == null) throw new InvalidOperationException($"{nameof(StaticOnlinePayment)} is not initialized.");

                return _parbadAccessor.OnlinePayment;
            }
        }

        /// <summary>
        /// Initializes the <see cref="StaticOnlinePayment"/> class for static usage.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        internal static void Initialize(IOnlinePaymentAccessor parbadAccessor)
        {
            _parbadAccessor = parbadAccessor ?? throw new ArgumentNullException(nameof(parbadAccessor));
        }
    }
}
