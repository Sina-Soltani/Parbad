using System.IO.Compression;
using System.Web;
using Parbad.Utilities;

namespace Parbad.Web
{
    internal static class ResponseCompressor
    {
        private enum AcceptEncoding
        {
            None,
            GZip,
            Deflate
        }

        public static void Compress(HttpContext httpContext)
        {
            var acceptEncoding = GetAcceptEncoding(httpContext.Request);

            if (acceptEncoding == AcceptEncoding.None)
            {
                return;
            }

            var response = httpContext.Response;

            if (acceptEncoding == AcceptEncoding.GZip)
            {
                response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);

                response.Headers.Remove("Content-Encoding");

                response.AppendHeader("Content-Encoding", "gzip");
            }
            else
            {
                response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);

                response.Headers.Remove("Content-Encoding");

                response.AppendHeader("Content-Encoding", "deflate");
            }

            //  Allow proxy servers to cache encoded and unencoded versions separately
            response.AppendHeader("Vary", "Content-Encoding");
        }

        private static AcceptEncoding GetAcceptEncoding(HttpRequest httpRequest)
        {
            var acceptEncoding = httpRequest.Headers["Accept-Encoding"];

            if (acceptEncoding.IsNullOrWhiteSpace())
            {
                return AcceptEncoding.None;
            }

            if (acceptEncoding.Contains("gzip"))
            {
                return AcceptEncoding.GZip;
            }

            if (acceptEncoding.Contains("deflate"))
            {
                return AcceptEncoding.Deflate;
            }

            return AcceptEncoding.None;
        }
    }
}