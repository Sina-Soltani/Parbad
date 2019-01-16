using System;
using Parbad.Utilities;

namespace Parbad.Providers.Melli
{
    /// <summary>
    /// Melli Gateway configurations class.
    /// </summary>
    public class MelliGatewayConfiguration
    {
        /// <summary>
        /// Initializes an instance of <see cref="MelliGatewayConfiguration"/>
        /// </summary>
        /// <param name="terminalId"></param>
        /// <param name="merchantId"></param>
        /// <param name="terminalKey"></param>
        public MelliGatewayConfiguration(string terminalId, string merchantId, string terminalKey)
        {
            if (terminalId.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(terminalId), $"The Gateway: \"Melli\" configuration failed. \"{nameof(terminalId)}\" is null or empty");
            if (merchantId.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(merchantId), $"The Gateway: \"Melli\" configuration failed. \"{nameof(merchantId)}\" is null or empty");
            if (terminalKey.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(terminalKey), $"The Gateway: \"Melli\" configuration failed. \"{nameof(terminalKey)}\" is null or empty");

            TerminalId = terminalId;
            MerchantId = merchantId;
            TerminalKey = terminalKey;
        }

        public string TerminalId { get; }

        public string MerchantId { get; }

        public string TerminalKey { get; }
    }
}