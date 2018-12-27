using System;
using Parbad.Utilities;

namespace Parbad.Providers.Melli
{
    public class MelliGatewayConfiguration
    {
        public MelliGatewayConfiguration(string terminalId, string merchantId, string merchantKey)
        {
            if (terminalId.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(terminalId), $"The Gateway: \"Melli\" configuration failed. \"{nameof(terminalId)}\" is null or empty");
            if (merchantId.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(merchantId), $"The Gateway: \"Melli\" configuration failed. \"{nameof(merchantId)}\" is null or empty");
            if (merchantKey.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(merchantKey), $"The Gateway: \"Melli\" configuration failed. \"{nameof(merchantKey)}\" is null or empty");

            TerminalId = terminalId;
            MerchantId = merchantId;
            MerchantKey = merchantKey;
        }

        public string TerminalId { get; }

        public string MerchantId { get; }

        public string MerchantKey { get; }
    }
}