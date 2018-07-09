using System.Linq;
using Parbad.Core;
using Parbad.Exceptions;
using Parbad.Utilities;

namespace Parbad.Providers
{
    internal static class GatewayFactory
    {
        public static GatewayBase CreateGateway(Gateway selectedGateway)
        {
            var allGateways = GatewayFinder.GetAllGateways();

            var gatewayBase = allGateways.FirstOrDefault(gateway => gateway.Name == selectedGateway.ToString());

            if (gatewayBase == null)
            {
                throw new GatewayNotFoundException();
            }

            return gatewayBase;
        }
    }
}