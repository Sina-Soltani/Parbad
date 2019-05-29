using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Abstraction;

namespace Parbad.GatewayBuilders
{
    /// <summary>
    /// A builder for building gateway accounts.
    /// </summary>
    /// <typeparam name="TAccount">Type of the gateway account.</typeparam>
    public interface IGatewayAccountBuilder<TAccount> where TAccount : GatewayAccount
    {
        /// <summary>
        /// Specifies the contract for a collection of service descriptors.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Removes the account sources of this gateway.
        /// </summary>
        IGatewayAccountBuilder<TAccount> Clear();

        /// <summary>
        /// Adds the given <paramref name="source"/> for specifying the accounts of type <typeparamref name="TAccount"/>.
        /// </summary>
        /// <param name="source">An account source for specifying the accounts.</param>
        IGatewayAccountBuilder<TAccount> Add(IGatewayAccountSource<TAccount> source);

        /// <summary>
        /// Adds the given <typeparamref name="TSource"/> for specifying the accounts of type <typeparamref name="TAccount"/>.
        /// </summary>
        /// <typeparam name="TSource">Type of the account source.</typeparam>
        /// <param name="serviceLifetime">Lifetime of <typeparamref name="TSource"></typeparamref>.</param>
        IGatewayAccountBuilder<TAccount> Add<TSource>(ServiceLifetime serviceLifetime)
            where TSource : class, IGatewayAccountSource<TAccount>;

        /// <summary>
        /// Adds the given account source factory for specifying the accounts of type <typeparamref name="TAccount"/>.
        /// </summary>
        /// <param name="factory">A factory for creating an instance of <see cref="IGatewayAccountSource{TAccount}"/>.</param>
        /// <param name="serviceLifetime">Lifetime of the given source.</param>
        IGatewayAccountBuilder<TAccount> Add(
            Func<IServiceProvider, IGatewayAccountSource<TAccount>> factory,
            ServiceLifetime serviceLifetime);
    }
}
