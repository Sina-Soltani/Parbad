// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

#if NETSTANDARD2_0 || NETSTANDARD2_1
namespace System.Runtime.CompilerServices;

/// <summary>Lets the netstandard targets compile <c>init</c> accessors; .NET 5 and later have this type.</summary>
internal static class IsExternalInit
{
}
#endif
