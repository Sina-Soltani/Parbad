// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Parbad.Internal;
using Parbad.PaymentTokenProviders;
using System;

namespace Parbad.Builder
{
    public static class QueryStringPaymentTokenProviderExtensions
    {
        /// <summary>
        /// Uses the <see cref="GuidQueryStringPaymentTokenProvider"/> as <see cref="IPaymentTokenProvider"/>.
        /// It generates a unique <see cref="Guid"/> as a token for each payments and puts it inside the query string
        /// of the URL. The generated token will be retrieved from the query string when the client
        /// comes back from the gateway.
        /// <para>Note: This is the default <see cref="IPaymentTokenProvider"/>, which means you don't need to call this.</para>
        /// </summary>
        /// <param name="builder"></param>
        public static IPaymentTokenBuilder UseGuidQueryStringPaymentTokenProvider(this IPaymentTokenBuilder builder)
        {
            return builder.UseGuidQueryStringPaymentTokenProvider(option => { });
        }

        /// <summary>
        /// Uses the <see cref="GuidQueryStringPaymentTokenProvider"/> as <see cref="IPaymentTokenProvider"/>.
        /// It generates a unique <see cref="Guid"/> as a token for each payments and puts it inside the query string
        /// of the URL. The generated token will be retrieved from the query string when the client
        /// comes back from the gateway.
        /// <para>Note: This is the default <see cref="IPaymentTokenProvider"/>, which means you don't need to call this.</para>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions">A delegate to configure the <see cref="QueryStringPaymentTokenOptions"/>.</param>
        public static IPaymentTokenBuilder UseGuidQueryStringPaymentTokenProvider(this IPaymentTokenBuilder builder,
            Action<QueryStringPaymentTokenOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);

            builder.AddPaymentTokenProvider<GuidQueryStringPaymentTokenProvider>(ServiceLifetime.Transient);

            return builder;
        }
    }
}
