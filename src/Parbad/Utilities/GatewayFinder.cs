using System;
using System.Collections.Generic;
using System.Linq;
using Parbad.Core;
using Parbad.Providers;

namespace Parbad.Utilities
{
    internal static class GatewayFinder
    {
        public static IEnumerable<GatewayBase> GetAllGateways()
        {
            var types = typeof(GatewayFactory).Assembly.GetTypes()
                .Where(type => type.BaseType != null &&
                               type.BaseType == typeof(GatewayBase) &&
                               type.IsSubclassOf(typeof(GatewayBase)) &&
                               !type.IsAbstract &&
                               type.IsClass);

            return types.Select(type => (GatewayBase)Activator.CreateInstance(type));
        }
    }
}