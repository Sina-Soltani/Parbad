// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Exceptions
{
    [Serializable]
    public class GatewayNotFoundException : Exception
    {
        public GatewayNotFoundException(string gatewayName) : base($"No gateway found with the name {gatewayName}. " +
                                                                   "Make sure you have already added the required gateways. " +
                                                                   "To add and configure a gateway use the ConfigureGateways method.")
        {
        }
    }
}
