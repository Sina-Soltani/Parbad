// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Parbad.Abstraction;
using Parbad.Exceptions;

namespace Parbad.Internal
{
    internal static class GatewayHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="throwException"></param>
        /// <exception cref="InvalidGatewayTypeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsGateway(Type type, bool throwException = false)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var result = typeof(IGateway).IsAssignableFrom(type) &&
                         !type.IsInterface &&
                         !type.IsAbstract &&
                         type.IsClass;

            if (!result && throwException)
            {
                throw new InvalidGatewayTypeException(type);
            }

            return result;
        }

        public static IEnumerable<Type> FindAllGatewaysFromAssemblyContaining<TAssembly>() where TAssembly : class
        {
            return typeof(TAssembly)
                .Assembly
                .GetTypes()
                .Where(type => IsGateway(type));
        }

        public static IEnumerable<Type> FindAllGatewaysTypes()
        {
            return FindAllGatewaysFromAssemblyContaining<IGateway>();
        }

        public static string GetCompleteGatewayName(this IGateway gateway)
        {
            if (gateway == null) throw new ArgumentNullException(nameof(gateway));

            return GetCompleteGatewayName(gateway.GetType());
        }

        public static string GetCompleteGatewayName<TGateway>() where TGateway : class, IGateway
        {
            return GetCompleteGatewayName(typeof(TGateway));
        }

        public static string GetCompleteGatewayName(Type gatewayType)
        {
            if (gatewayType == null) throw new ArgumentNullException(nameof(gatewayType));

            IsGateway(gatewayType, throwException: true);

            return gatewayType.Name;
        }

        public static string GetRoutingGatewayName(Type gatewayType)
        {
            if (gatewayType == null) throw new ArgumentNullException(nameof(gatewayType));

            IsGateway(gatewayType, throwException: true);

            if (gatewayType.HasAttribute<GatewayAttribute>())
            {
                return gatewayType.GetCustomAttribute<GatewayAttribute>().Name;
            }

            var gatewayName = GetCompleteGatewayName(gatewayType).ToggleStringAtEnd("Gateway", false);

            return gatewayName;
        }

        public static string GetRoutingGatewayName(this IGateway gateway)
        {
            if (gateway == null) throw new ArgumentNullException(nameof(gateway));

            var gatewayType = gateway.GetType();

            return GetRoutingGatewayName(gatewayType);
        }

        public static bool CompareName(Type gatewayType, string gatewayName)
        {
            if (gatewayType == null) throw new ArgumentNullException(nameof(gatewayType));
            if (gatewayName == null) throw new ArgumentNullException(nameof(gatewayName));

            return string.Equals(gatewayName, GetRoutingGatewayName(gatewayType), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// </summary>
        /// <param name="gatewayName"></param>
        /// <param name="throwException"></param>
        /// <exception cref="GatewayNotFoundException"></exception>
        public static Type FindGatewayTypeByName(string gatewayName, bool throwException = false)
        {
            if (gatewayName == null) throw new ArgumentNullException(nameof(gatewayName));

            var gateways = FindAllGatewaysTypes().Where(type => CompareName(type, gatewayName)).ToList();

            if (gateways.Count == 0)
            {
                if (throwException) throw new GatewayNotFoundException(gatewayName);

                return null;
            }

            if (gateways.Count > 1)
            {
                throw new InvalidOperationException($"More than one gateway exist with the name {gatewayName}");
            }

            return gateways[0];
        }
    }
}
