using Parbad.Abstraction;

namespace Parbad.Gateway.AsanPardakht
{
    public class AsanPardakhtGatewayAccount : GatewayAccount
    {
        public string MerchantConfigurationId { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Key { get; set; }

        public string IV { get; set; }
    }
}