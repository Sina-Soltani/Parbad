using System;

namespace Parbad.Providers.Melli
{
    /// <summary>
    /// The exception that is thrown when an error by signing is occurred.
    /// </summary>
    public class MelliGatewayDataSigningException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the Parbad.Providers.Melli.MelliGatewayDataSigningException class.
        /// </summary>
        /// <param name="mainException">The main Exception object.</param>
        public MelliGatewayDataSigningException(Exception mainException) : base($"MelliBank signing data failed. {mainException.Message}", mainException)
        {

        }
    }
}