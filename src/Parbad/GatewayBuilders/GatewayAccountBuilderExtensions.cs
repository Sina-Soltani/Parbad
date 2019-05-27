using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Parbad.Abstraction;
using Parbad.GatewayBuilders;
using Parbad.Internal;

namespace Parbad.Builder
{
    public static class GatewayAccountBuilderExtensions
    {
        /// <summary>
        /// Adds an in-memory account of type <typeparamref name="TAccount"/>.
        /// </summary>
        /// <typeparam name="TAccount"></typeparam>
        /// <param name="builder"></param>
        /// <param name="accounts">The accounts to add.</param>
        public static IGatewayAccountBuilder<TAccount> AddInMemory<TAccount>(
            this IGatewayAccountBuilder<TAccount> builder,
            IEnumerable<TAccount> accounts)
            where TAccount : GatewayAccount, new()
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (accounts == null) throw new ArgumentNullException(nameof(accounts));

            return builder.Add(new InMemoryGatewayAccountSource<TAccount>(accounts));
        }

        /// <summary>
        /// Adds an in-memory account of type <typeparamref name="TAccount"/>.
        /// </summary>
        /// <typeparam name="TAccount"></typeparam>
        /// <param name="builder"></param>
        /// <param name="configureAccount">Configures the <typeparamref name="TAccount"/>.</param>
        public static IGatewayAccountBuilder<TAccount> AddInMemory<TAccount>(
            this IGatewayAccountBuilder<TAccount> builder, Action<TAccount> configureAccount)
            where TAccount : GatewayAccount, new()
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configureAccount == null) throw new ArgumentNullException(nameof(configureAccount));

            var account = new TAccount();

            configureAccount(account);

            builder.AddInMemory(new[] { account });

            return builder;
        }

        /// <summary>
        /// Adds an account of type <typeparamref name="TAccount"/> using the given <paramref name="configuration"/>.
        /// </summary>
        /// <typeparam name="TAccount"></typeparam>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        public static IGatewayAccountBuilder<TAccount> AddFromConfiguration<TAccount>(
            this IGatewayAccountBuilder<TAccount> builder,
            IConfiguration configuration)
            where TAccount : GatewayAccount, new()
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            builder.Add(new MsConfigurationGatewayAccountSource<TAccount>(configuration));

            return builder;
        }

        internal static IGatewayBuilder AddGatewayAccountProvider<TAccount>(this IGatewayBuilder builder)
            where TAccount : GatewayAccount
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services
                .TryAddTransient<
                    IGatewayAccountProvider<TAccount>,
                    GatewayAccountProvider<TAccount>>();

            return builder;
        }
    }
}
