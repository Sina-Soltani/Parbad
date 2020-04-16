// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad
{
    public static class MoneyExtensions
    {
        public static string ToLongString(this Money money)
        {
            return ((long)money).ToString();
        }
    }
}
