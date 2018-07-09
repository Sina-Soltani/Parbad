using System;

namespace Parbad.Exceptions
{
    [Serializable]
    public class GatewayNotFoundException : Exception
    {
        public GatewayNotFoundException() : base("Selected gateway is not found.")
        {
        }
    }
}