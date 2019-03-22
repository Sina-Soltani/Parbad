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

        public static string GetName(this IGateway gateway)
        {
            if (gateway == null) throw new ArgumentNullException(nameof(gateway));

            return GetNameByType(gateway.GetType());
        }

        public static string GetName<TGateway>() where TGateway : class, IGateway
        {
            return GetNameByType(typeof(TGateway));
        }

        public static string GetNameByType(Type gatewayType)
        {
            if (gatewayType == null) throw new ArgumentNullException(nameof(gatewayType));

            IsGateway(gatewayType, throwException: true);

            var name = gatewayType.HasAttribute<GatewayAttribute>()
                ? gatewayType.GetCustomAttribute<GatewayAttribute>().Name
                : gatewayType.Name;

            name = name.ToggleStringAtEnd("Gateway", true);

            return name;
        }

        public static bool CompareName(Type gatewayType, string gatewayName)
        {
            if (gatewayType == null) throw new ArgumentNullException(nameof(gatewayType));
            if (gatewayName == null) throw new ArgumentNullException(nameof(gatewayName));

            gatewayName = gatewayName.ToggleStringAtEnd("Gateway", true);

            return string.Equals(gatewayName, GetNameByType(gatewayType), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// </summary>
        /// <param name="gatewayName"></param>
        /// <param name="throwException"></param>
        /// <exception cref="GatewayNotFoundException"></exception>
        public static Type FindGatewayTypeByName(string gatewayName, bool throwException = false)
        {
            if (gatewayName == null) throw new ArgumentNullException(nameof(gatewayName));

            var gatewayType = FindAllGatewaysTypes().SingleOrDefault(type => CompareName(type, gatewayName));

            if (gatewayType == null && throwException)
            {
                throw new GatewayNotFoundException(gatewayName);
            }

            return gatewayType;
        }
    }
}
