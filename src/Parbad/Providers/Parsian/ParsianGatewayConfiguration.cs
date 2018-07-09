using System;

namespace Parbad.Providers.Parsian
{
    public class ParsianGatewayConfiguration
    {
        public ParsianGatewayConfiguration(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin))
            {
                throw new ArgumentNullException(nameof(pin), $"The Gateway: \"Parsian\" configuration failed. \"{nameof(pin)}\" is null or empty");
            }

            Pin = pin;
        }

        public string Pin { get; }
    }
}