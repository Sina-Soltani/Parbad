using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;

namespace Parbad.Utilities
{
    public static class QueryHelper
    {
        public static string AddQueryString(string url, string key, string value)
        {
            return AddQueryString(url, new[] { new KeyValuePair<string, string>(key, value) });
        }

        public static string AddQueryString(string url, IEnumerable<KeyValuePair<string, string>> queries)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (queries == null) throw new ArgumentNullException(nameof(queries));

            var anchorIndex = url.IndexOf('#');
            var uriToBeAppended = url;
            var anchorText = "";

            if (anchorIndex != -1)
            {
                anchorText = url.Substring(anchorIndex);

                uriToBeAppended = url.Substring(0, anchorIndex);
            }

            var queryIndex = uriToBeAppended.IndexOf('?');
            var hasQuery = queryIndex != -1;

            var generatedUrl = new StringBuilder();

            generatedUrl.Append(uriToBeAppended);

            foreach (var query in queries)
            {
                if (query.Value == null) continue;

                generatedUrl.Append(hasQuery ? '&' : '?');
                generatedUrl.Append(UrlEncoder.Default.Encode(query.Key));
                generatedUrl.Append('=');
                generatedUrl.Append(UrlEncoder.Default.Encode(query.Value));

                hasQuery = true;
            }

            generatedUrl.Append(anchorText);

            return generatedUrl.ToString();
        }
    }
}
