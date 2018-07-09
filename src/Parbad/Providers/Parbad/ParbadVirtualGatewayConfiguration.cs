using System;
using Parbad.Utilities;

namespace Parbad.Providers.Parbad
{
    public class ParbadVirtualGatewayConfiguration
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes ParbadVirtualGatewayConfiguration.
        /// </summary>
        /// <param name="gatewayHandlerPath">If you defined any handlers for Parbad.Web.Gateway.ParbadVirtualGatewayHandler inside web.config, then set path of handler.</param>
        public ParbadVirtualGatewayConfiguration(string gatewayHandlerPath) : this(gatewayHandlerPath, null)
        {
        }

        /// <summary>
        /// Initializes ParbadVirtualGatewayConfiguration with a password for accessing gateway.
        /// </summary>
        /// <param name="gatewayHandlerPath">If you defined any handlers for Parbad.Web.Gateway.ParbadVirtualGatewayHandler inside web.config, then set path of handler.</param>
        /// <param name="gatewayPassword">A password for accessing gateway</param>
        public ParbadVirtualGatewayConfiguration(string gatewayHandlerPath, string gatewayPassword)
        {
            if (gatewayHandlerPath.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(gatewayHandlerPath), "Gateway URL cannot be null or empty.");
            }

            GatewayHandlerPath = gatewayHandlerPath;

            GatewayPassword = CommonTools.HashPassword(gatewayPassword);
        }

        /// <summary>
        /// If you defined any handlers for Parbad.Web.Gateway.ParbadVirtualGatewayHandler inside web.config, then set path of handler.
        /// </summary>
        public string GatewayHandlerPath { get; }

        /// <summary>
        /// A password for accessing gateway.
        /// </summary>
        public string GatewayPassword { get; }
    }
}