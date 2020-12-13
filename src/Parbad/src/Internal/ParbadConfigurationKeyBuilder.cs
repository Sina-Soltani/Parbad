// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Internal
{
    public static class ParbadConfigurationKeyBuilder
    {
        public static string CreateGatewayKey(string gatewayName)
        {
            return $"parbad:gateways:{gatewayName}";
        }

        public static string CreateGatewayConfigurationKey(string gatewayName, string key)
        {
            return $"{CreateGatewayKey(gatewayName)}:{key}";
        }
    }
}
