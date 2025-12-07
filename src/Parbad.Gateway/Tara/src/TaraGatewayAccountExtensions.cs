// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Builder;
using Parbad.GatewayBuilders;
using System;

namespace Parbad.Gateway.Tara
{
    public static class TaraGatewayAccountExtensions
    {
        /// <summary>
        /// Sets the username for Tara gateway authentication.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="username">Username</param>
        public static IGatewayAccountBuilder<TaraGatewayAccount> SetUsername(
            this IGatewayAccountBuilder<TaraGatewayAccount> builder,
            string username)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddInMemory(account => account.Username = username);
        }

        /// <summary>
        /// Sets the password for Tara gateway authentication.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="password">Password</param>
        public static IGatewayAccountBuilder<TaraGatewayAccount> SetPassword(
            this IGatewayAccountBuilder<TaraGatewayAccount> builder,
            string password)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddInMemory(account => account.Password = password);
        }

        /// <summary>
        /// Sets the IP address for Tara gateway API calls.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="ip">IP Address</param>
        public static IGatewayAccountBuilder<TaraGatewayAccount> SetIp(
            this IGatewayAccountBuilder<TaraGatewayAccount> builder,
            string ip)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddInMemory(account => account.Ip = ip);
        }

        /// <summary>
        /// Marks this account as a test account to use the staging environment.
        /// This will automatically switch to https://stage-pay.tara360.ir/pay
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="isTest">Whether to use staging environment</param>
        public static IGatewayAccountBuilder<TaraGatewayAccount> UseTestMode(
            this IGatewayAccountBuilder<TaraGatewayAccount> builder,
            bool isTest = true)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.AddInMemory(account => account.IsTest = isTest);
        }
    }
}

