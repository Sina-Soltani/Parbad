using Parbad.Abstraction;

namespace Parbad.Gateway.Zibal
{
    public class ZibalGatewayAccount: GatewayAccount
    {
        public string Merchant { get; set; }
        public bool IsSandBox { get; set; }
    }
}