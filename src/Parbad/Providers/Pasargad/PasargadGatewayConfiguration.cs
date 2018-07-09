using System;
using Parbad.Utilities;

namespace Parbad.Providers.Pasargad
{
    public class PasargadGatewayConfiguration
    {
        public PasargadGatewayConfiguration(string merchantCode, string terminalCode, string privateKey)
        {
            if (merchantCode.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(merchantCode), $"The Gateway: \"Pasargad\" configuration failed. \"{nameof(merchantCode)}\" is null or empty");
            }

            if (terminalCode.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(terminalCode), $"The Gateway: \"Pasargad\" configuration failed. \"{nameof(terminalCode)}\" is null or empty");
            }

            if (privateKey.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(privateKey), $"The Gateway: \"Pasargad\" configuration failed. \"{nameof(privateKey)}\" is null or empty");
            }

            if (!PasargadHelper.IsPrivateKeyValid(privateKey))
            {
                throw new Exception("Pasargad Private Key is not a valid XML. Please check your Private Key again.");
            }

            MerchantCode = merchantCode;
            TerminalCode = terminalCode;
            PrivateKey = privateKey;
        }

        public string MerchantCode { get; }

        public string TerminalCode { get; }

        public string PrivateKey { get; }
    }
}