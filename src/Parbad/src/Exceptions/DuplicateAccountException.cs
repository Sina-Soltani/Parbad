using System;
using Parbad.Abstraction;

namespace Parbad.Exceptions
{
    [Serializable]
    public class DuplicateAccountException : Exception
    {
        public DuplicateAccountException(GatewayAccount account)
            : base($"There is an account already with the name {account.Name}. Make sure to use different names for accounts.")
        {
        }
    }
}
