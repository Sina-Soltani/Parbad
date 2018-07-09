using System;
using Parbad.Providers;

namespace Parbad.Exceptions
{
    [Serializable]
    public class GatewayConfigurationException : Exception
    {
        public GatewayConfigurationException(Gateway gateway) : base($"The gateway \"{gateway}\" is not configured. use ParbadConfiguration.Gateways methods to configure the gateways.")
        {
        }
    }
}