using System;
using System.Web;
using Parbad.Utilities;

namespace Parbad.Web
{
    internal static class InternalExtensions
    {
        public static bool TryGetPaymentDataId(this HttpRequest httpRequest, out Guid id)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            try
            {
                id = Guid.Parse(httpRequest.QueryString["PaymentID"]);
                return true;
            }
            catch
            {
                id = Guid.Empty;
                return false;
            }
        }

        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request["X-Requested-With"] == "XMLHttpRequest" ||
                   request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        public static void AppendParbadHeaders(this HttpResponse httpResponse)
        {
            httpResponse.Cache.SetCacheability(HttpCacheability.NoCache);

            httpResponse.AppendHeader("Online-Payment-System", "Powered By Parbad (http://www.parbad.ir)");
        }

        public static Uri AppendQueryString(this Uri uri, string key, string value)
        {
            var queryString = uri.Query;

            if (queryString.IsNullOrWhiteSpace())
            {
                queryString = "?";
            }

            if (queryString.Length > 1)
            {
                queryString += "&";
            }

            queryString += $"{key}={value}";

            var url = $"{uri.Scheme}://{uri.Host}";

            if (uri.Port > 0 && uri.Port != 80)
            {
                url += $":{uri.Port}";
            }

            url += $"{uri.AbsolutePath}";

            return new Uri(url + queryString);
        }
    }
}