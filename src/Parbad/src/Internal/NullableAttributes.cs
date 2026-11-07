// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

#if NETSTANDARD2_0
namespace System.Diagnostics.CodeAnalysis;

// netstandard2.0 has no nullable attributes; netstandard2.1 and .NET have them. The compiler only needs their names.

[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class NotNullWhenAttribute(bool returnValue) : Attribute
{
    public bool ReturnValue { get; } = returnValue;
}

[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class MaybeNullWhenAttribute(bool returnValue) : Attribute
{
    public bool ReturnValue { get; } = returnValue;
}
#endif
