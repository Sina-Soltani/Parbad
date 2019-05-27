using System;
using Parbad.Abstraction;
using Parbad.Internal;

namespace Parbad.GatewayBuilders
{
    public static class GatewayAccountCollectionExtensions
    {
        /// <summary>
        /// Gets an account with the given name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="accounts"></param>
        /// <param name="accountName"></param>
        /// <exception cref="Exception"></exception>
        public static T GetOrDefault<T>(this IGatewayAccountCollection<T> accounts, string accountName)
            where T : GatewayAccount
        {
            T account;

            if (accountName.IsNullOrEmpty())
            {
                account = accounts.GetDefaultAccount();

                if (account == null)
                {
                    throw new Exception($"No accounts of type {typeof(T).Name} exist.");
                }
            }
            else
            {
                account = accounts.Get(accountName);

                if (account == null)
                {
                    throw new Exception($"Account {accountName} does not exist.");
                }
            }

            return account;
        }
    }
}
