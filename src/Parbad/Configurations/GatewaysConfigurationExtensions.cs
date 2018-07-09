using System;
using Parbad.Providers;
using Parbad.Providers.Mellat;
using Parbad.Providers.Parbad;
using Parbad.Providers.Parsian;
using Parbad.Providers.Pasargad;
using Parbad.Providers.Saman;
using Parbad.Providers.Tejarat;
using Parbad.Utilities;

namespace Parbad.Configurations
{
    internal static class GatewaysConfigurationExtensions
    {
        internal static MellatGatewayConfiguration GetMellatGatewayConfiguration(this GatewaysConfiguration gatewayConfiguration)
        {
            if (gatewayConfiguration == null)
            {
                throw new ArgumentNullException(nameof(gatewayConfiguration));
            }

            return gatewayConfiguration.GetGatewayConfiguration<MellatGatewayConfiguration>(Gateway.Mellat);
        }

        internal static ParbadVirtualGatewayConfiguration GetParbadVirtualGatewayConfiguration(this GatewaysConfiguration gatewayConfiguration)
        {
            if (gatewayConfiguration == null)
            {
                throw new ArgumentNullException(nameof(gatewayConfiguration));
            }

            return gatewayConfiguration.GetGatewayConfiguration<ParbadVirtualGatewayConfiguration>(Gateway.ParbadVirtualGateway);
        }

        internal static bool HasPassword(this ParbadVirtualGatewayConfiguration parbadVirtualGatewayConfiguration)
        {
            if (parbadVirtualGatewayConfiguration == null)
            {
                throw new ArgumentNullException(nameof(parbadVirtualGatewayConfiguration));
            }

            return !parbadVirtualGatewayConfiguration.GatewayPassword.IsNullOrWhiteSpace();
        }

        internal static ParsianGatewayConfiguration GetParsianGatewayConfiguration(this GatewaysConfiguration gatewayConfiguration)
        {
            if (gatewayConfiguration == null)
            {
                throw new ArgumentNullException(nameof(gatewayConfiguration));
            }

            return gatewayConfiguration.GetGatewayConfiguration<ParsianGatewayConfiguration>(Gateway.Parsian);
        }

        internal static PasargadGatewayConfiguration GetPasargadGatewayConfiguration(this GatewaysConfiguration gatewayConfiguration)
        {
            if (gatewayConfiguration == null)
            {
                throw new ArgumentNullException(nameof(gatewayConfiguration));
            }

            return gatewayConfiguration.GetGatewayConfiguration<PasargadGatewayConfiguration>(Gateway.Pasargad);
        }

        internal static SamanGatewayConfiguration GetSamanGatewayConfiguration(this GatewaysConfiguration gatewayConfiguration)
        {
            if (gatewayConfiguration == null)
            {
                throw new ArgumentNullException(nameof(gatewayConfiguration));
            }

            return gatewayConfiguration.GetGatewayConfiguration<SamanGatewayConfiguration>(Gateway.Saman);
        }

        internal static TejaratGatewayConfiguration GetTejaratGatewayConfiguration(this GatewaysConfiguration gatewayConfiguration)
        {
            if (gatewayConfiguration == null)
            {
                throw new ArgumentNullException(nameof(gatewayConfiguration));
            }

            return gatewayConfiguration.GetGatewayConfiguration<TejaratGatewayConfiguration>(Gateway.Tejarat);
        }

        internal static TConfiguration GetGatewayConfiguration<TConfiguration>(this GatewaysConfiguration configuration, Gateway gateway) where TConfiguration : class
        {
            return configuration.GetGatewayConfiguration(gateway) as TConfiguration;
        }

        internal static bool IsGatewayConfigured(this GatewaysConfiguration configuration, Gateway gateway)
        {
            return configuration.GetGatewayConfiguration(gateway) != null;
        }
    }
}