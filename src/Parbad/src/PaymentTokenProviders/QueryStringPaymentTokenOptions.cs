// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Configuration;

namespace Parbad.PaymentTokenProviders
{
    public class QueryStringPaymentTokenOptions
    {
        /// <summary>
        /// The specific key for <see cref="IConfiguration"/>
        /// </summary>
        public static readonly string ConfigurationKey = "Parbad:PaymentTokenProvider:QueryString";

        public static readonly string DefaultQueryName = "paymentToken";

        /// <summary>
        /// The default value is "paymentToken".
        /// </summary>
        public string QueryName { get; set; } = DefaultQueryName;
    }
}
