using System.Collections.Generic;
using Parbad.Abstraction;
using Parbad.Exceptions;

namespace Parbad.GatewayBuilders
{
    /// <summary>
    /// A collection for holding gateway accounts.
    /// </summary>
    /// <typeparam name="TAccount">Type of account.</typeparam>
    /// <exception cref="DuplicateAccountException"></exception>
    public interface IGatewayAccountCollection<TAccount> : ICollection<TAccount>
        where TAccount : GatewayAccount
    {
        /// <summary>
        /// Gets an account with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of account.</param>
        TAccount Get(string name);

        /// <summary>
        /// Gets the default account.
        /// </summary>
        /// <returns></returns>
        TAccount GetDefaultAccount();
    }
}
