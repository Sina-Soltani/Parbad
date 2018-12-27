using System;
using System.Collections.Generic;
using Parbad.Providers;
using Parbad.Providers.IranKish;
using Parbad.Providers.Mellat;
using Parbad.Providers.Melli;
using Parbad.Providers.Parbad;
using Parbad.Providers.Parsian;
using Parbad.Providers.Pasargad;
using Parbad.Providers.Saman;
using Parbad.Providers.Tejarat;

namespace Parbad.Configurations
{
    /// <summary>
    /// It holds all gateways configurations data.
    /// </summary>
    public class GatewaysConfiguration
    {
        private readonly IDictionary<Gateway, object> _gatewayConfigurations;

        internal GatewaysConfiguration()
        {
            _gatewayConfigurations = new Dictionary<Gateway, object>();
        }

        /// <summary>
        /// Configure Mellat gateway
        /// </summary>
        public void ConfigureMellat(MellatGatewayConfiguration mellatGatewayConfiguration)
        {
            if (mellatGatewayConfiguration == null)
            {
                throw new ArgumentNullException(nameof(mellatGatewayConfiguration));
            }

            AddOrUpdate(Gateway.Mellat, mellatGatewayConfiguration);
        }

        /// <summary>
        /// Configure Parbad Virtual Gateway
        /// </summary>
        public void ConfigureParbadVirtualGateway(ParbadVirtualGatewayConfiguration parbadVirtualGatewayConfiguration)
        {
            if (parbadVirtualGatewayConfiguration == null)
            {
                throw new ArgumentNullException(nameof(parbadVirtualGatewayConfiguration));
            }

            AddOrUpdate(Gateway.ParbadVirtualGateway, parbadVirtualGatewayConfiguration);
        }

        /// <summary>
        /// Configure Parsian gateway
        /// </summary>
        public void ConfigureParsian(ParsianGatewayConfiguration parsianGatewayConfiguration)
        {
            if (parsianGatewayConfiguration == null)
            {
                throw new ArgumentNullException(nameof(parsianGatewayConfiguration));
            }

            AddOrUpdate(Gateway.Parsian, parsianGatewayConfiguration);
        }

        /// <summary>
        /// Configure Pasargad gateway
        /// </summary>
        public void ConfigurePasargad(PasargadGatewayConfiguration pasargadGatewayConfiguration)
        {
            if (pasargadGatewayConfiguration == null)
            {
                throw new ArgumentNullException(nameof(pasargadGatewayConfiguration));
            }

            AddOrUpdate(Gateway.Pasargad, pasargadGatewayConfiguration);
        }

        /// <summary>
        /// Configure Saman gateway
        /// </summary>
        public void ConfigureSaman(SamanGatewayConfiguration samanGatewayConfiguration)
        {
            if (samanGatewayConfiguration == null)
            {
                throw new ArgumentNullException(nameof(samanGatewayConfiguration));
            }

            AddOrUpdate(Gateway.Saman, samanGatewayConfiguration);
        }

        /// <summary>
        /// Configure Tejarat gateway
        /// </summary>
        public void ConfigureTejarat(TejaratGatewayConfiguration tejaratGatewayConfiguration)
        {
            if (tejaratGatewayConfiguration == null)
            {
                throw new ArgumentNullException(nameof(tejaratGatewayConfiguration));
            }

            AddOrUpdate(Gateway.Tejarat, tejaratGatewayConfiguration);
        }

        /// <summary>
        /// Configure IranKish gateway
        /// </summary>
        public void ConfigureIranKish(IranKishGatewayConfiguration iranKishGatewayConfiguration)
        {
            if (iranKishGatewayConfiguration == null)
            {
                throw new ArgumentNullException(nameof(iranKishGatewayConfiguration));
            }

            AddOrUpdate(Gateway.IranKish, iranKishGatewayConfiguration);
        }

        /// <summary>
        /// Configure Melli gateway
        /// </summary>
        public void ConfigureMelli(MelliGatewayConfiguration melliGatewayConfiguration)
        {
            if (melliGatewayConfiguration == null) throw new ArgumentNullException(nameof(melliGatewayConfiguration));

            AddOrUpdate(Gateway.Melli, melliGatewayConfiguration);
        }

        internal void AddOrUpdate(Gateway gateway, object gatewayConfiguration)
        {
            if (!_gatewayConfigurations.ContainsKey(gateway))
            {
                _gatewayConfigurations.Add(gateway, gatewayConfiguration);
            }
            else
            {
                _gatewayConfigurations[gateway] = gatewayConfiguration;
            }
        }

        internal object GetGatewayConfiguration(Gateway gateway)
        {
            if (_gatewayConfigurations.ContainsKey(gateway))
            {
                return _gatewayConfigurations[gateway];
            }

            return null;
        }
    }
}