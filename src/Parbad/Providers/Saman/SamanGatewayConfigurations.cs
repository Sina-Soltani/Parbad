using System;
using Parbad.Utilities;

namespace Parbad.Providers.Saman
{
    public class SamanGatewayConfiguration
    {
        public SamanGatewayConfiguration(string merchantId, string password)
        {
            if (merchantId.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(merchantId), $"The Gateway: \"Saman\" configuration failed. \"{nameof(merchantId)}\" is null or empty");
            }

            if (password.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(password), $"The Gateway: \"Saman\" configuration failed. \"{nameof(password)}\" is null or empty");
            }

            MerchantId = merchantId;
            Password = password;
        }

        public string MerchantId { get; }

        public string Password { get; }
    }
}