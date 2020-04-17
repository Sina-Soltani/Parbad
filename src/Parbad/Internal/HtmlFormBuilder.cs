// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Parbad.Internal
{
    internal static class HtmlFormBuilder
    {
        public static string CreateForm(string url, IEnumerable<KeyValuePair<string, string>> data)
        {
            var fields = string.Join("", data.Select(item => CreateHiddenInput(item.Key, item.Value)));

            return
                "<html>" +
                "<body>" +
                $"<form id=\"paymentForm\" action=\"{url}\" method=\"post\" />" +
                fields +
                "</form>" +
                "<script type=\"text/javascript\">" +
                "document.getElementById('paymentForm').submit();" +
                "</script>" +
                "</body>" +
                "</html>";
        }

        public static string CreateHiddenInput(string name, string value)
        {
            return $"<input type=\"hidden\" name=\"{name}\" value=\"{value}\" />";
        }
    }
}
