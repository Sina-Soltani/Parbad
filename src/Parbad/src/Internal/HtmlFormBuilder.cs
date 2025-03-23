// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Parbad.Internal;

internal static class HtmlFormBuilder
{
    public static string CreateForm(string url, IEnumerable<KeyValuePair<string, string>> data, string nonce = null)
    {
        var fields = string.Join("", data.Select(CreateHiddenInput));

        var nonceProp = string.IsNullOrWhiteSpace(nonce) ? null : $" nonce=\"{nonce}\"";

        return
            "<html>" +
            "<body>" +
            $"<form id=\"paymentForm\" action=\"{url}\" method=\"post\" />" +
            fields +
            "</form>" +
            $"<script type=\"text/javascript\"{nonceProp}>" +
            "document.getElementById('paymentForm').submit();" +
            "</script>" +
            "</body>" +
            "</html>";
    }

    private static string CreateHiddenInput(KeyValuePair<string, string> data)
    {
        return $"<input type=\"hidden\" name=\"{data.Key}\" value=\"{data.Value}\" />";
    }
}
