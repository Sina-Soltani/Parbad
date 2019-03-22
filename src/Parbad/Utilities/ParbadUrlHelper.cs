// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.WebUtilities;

namespace Parbad.Utilities
{
    internal static class ParbadUrlHelper
    {
        public static string AddQueryString(string url, string name, string value)
        {
            return QueryHelpers.AddQueryString(url, name, value);
        }
    }
}
