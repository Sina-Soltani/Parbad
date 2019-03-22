// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Exceptions
{
    [Serializable]
    public class GatewayOptionsConfigurationException : Exception
    {
        public GatewayOptionsConfigurationException(string gatewayName) :
            base($"Gateway {gatewayName} is not configured or has some validation errors.")
        {
        }
    }
}
