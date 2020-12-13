// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Primitives;

namespace Parbad.Internal
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrEmpty(this StringValues value)
        {
            return StringValues.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool Equals(this StringValues value, string other, StringComparison comparisonType)
        {
            return string.Equals(value, other, comparisonType);
        }

        public static string ToggleStringAtStart(this string value, string stringValue, bool shouldHave)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (shouldHave)
            {
                if (!value.StartsWith(stringValue))
                {
                    value = stringValue + value;
                }
            }
            else
            {
                if (value.StartsWith(stringValue))
                {

                    value = value.Substring(stringValue.Length, value.Length - stringValue.Length);
                }
            }

            return value;
        }

        public static string ToggleStringAtEnd(this string value, string stringValue, bool shouldHave)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (shouldHave)
            {
                if (!value.EndsWith(stringValue))
                {
                    value += stringValue;
                }
            }
            else
            {
                if (value.EndsWith(stringValue))
                {
                    value = value.Substring(0, value.Length - stringValue.Length);
                }
            }

            return value;
        }
    }
}
