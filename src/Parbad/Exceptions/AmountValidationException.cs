using System;

namespace Parbad.Exceptions
{
    [Serializable]
    public class AmountValidationException : Exception
    {
        public AmountValidationException(long amount) : base($"Amount ({amount}) is not valid. It must be a positive number.")
        {
        }
    }
}