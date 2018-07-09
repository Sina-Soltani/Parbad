using System;
using Parbad.Utilities;

namespace Parbad.Providers.Tejarat
{
    public class TejaratGatewayConfiguration
    {
        public TejaratGatewayConfiguration(string merchant, string merchantPassword, string sha1Key)
        {
            if (merchant.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(merchant), $"The Gateway: \"Saman\" configuration failed. \"{nameof(merchant)}\" is null or empty");
            }

            if (merchantPassword.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(merchantPassword), $"The Gateway: \"Saman\" configuration failed. \"{nameof(merchantPassword)}\" is null or empty");
            }

            if (sha1Key.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(sha1Key), $"The Gateway: \"Saman\" configuration failed. \"{nameof(sha1Key)}\" is null or empty");
            }

            Merchant = merchant;
            MerchantPassword = merchantPassword;
            Sha1Key = sha1Key;
        }

        public string Merchant { get; }

        public string MerchantPassword { get; }

        public string Sha1Key { get; }
    }
}