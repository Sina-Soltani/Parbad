// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GatewayAttribute : Attribute
    {
        public GatewayAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            Name = name;
        }

        public string Name { get; }
    }
}
