// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Parbad.Internal
{
    public static class ReflectionExtensions
    {
        public static bool HasAttribute<T>(this Type type) where T : Attribute
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return type.GetCustomAttribute<T>() != null;
        }
    }
}
