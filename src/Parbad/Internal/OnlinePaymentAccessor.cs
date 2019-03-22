// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Exceptions;

namespace Parbad.Internal
{
    /// <inheritdoc />
    public class OnlinePaymentAccessor : IOnlinePaymentAccessor
    {
        /// <summary>
        /// Initializes an instance of <see cref="OnlinePaymentAccessor"/>.
        /// </summary>
        /// <param name="services"></param>
        public OnlinePaymentAccessor(IServiceProvider services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <inheritdoc />
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public IOnlinePayment OnlinePayment
        {
            get
            {
                var instance = Services.GetService<IOnlinePayment>();

                if (instance == null)
                {
                    throw new ParbadServiceNotInitializedException();
                }

                return instance;
            }
        }
    }
}
