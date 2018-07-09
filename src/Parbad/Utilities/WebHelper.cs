using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Parbad.Utilities
{
    internal static class WebHelper
    {
        public static string SendXmlWebRequest(string url, string data)
        {
            if (url.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(url));
            }

            var webRequest = WebRequest.CreateHttp(url);

            webRequest.Method = "POST";
            webRequest.ContentType = "text/xml;charset=UTF-8";

            var buffer = Encoding.UTF8.GetBytes(data);

            webRequest.ContentLength = buffer.Length;

            //  Send request
            using (var requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(buffer, 0, buffer.Length);
            }

            //  Get response
            using (var webResponse = webRequest.GetResponse())
            using (var dataStream = webResponse.GetResponseStream())
            using (var readerStream = new StreamReader(dataStream))
            {
                return readerStream.ReadToEnd();
            }
        }

        public static async Task<string> SendXmlWebRequestAsync(string url, string data)
        {
            if (url.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(url));
            }

            var webRequest = WebRequest.CreateHttp(url);

            webRequest.Method = "POST";
            webRequest.ContentType = "text/xml;charset=UTF-8";

            var buffer = Encoding.UTF8.GetBytes(data);

            webRequest.ContentLength = buffer.Length;

            //  Send request
            using (var requestStream = await webRequest.GetRequestStreamAsync())
            {
                await requestStream.WriteAsync(buffer, 0, buffer.Length);
            }

            //  Get response
            using (var webResponse = await webRequest.GetResponseAsync())
            using (var dataStream = webResponse.GetResponseStream())
            using (var readerStream = new StreamReader(dataStream))
            {
                return await readerStream.ReadToEndAsync();
            }
        }

        public static string SendWebRequest(string url, string data, string methodType, string contentType)
        {
            if (url.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (methodType.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(url));
            }

            var webRequest = (HttpWebRequest)WebRequest.Create(url);

            webRequest.Method = methodType;
            webRequest.ContentType = contentType;

            var buffer = Encoding.UTF8.GetBytes(data);

            webRequest.ContentLength = buffer.Length;

            //ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidation;

            //  Send request
            using (var requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(buffer, 0, buffer.Length);
            }

            //  Get response
            using (var webResponse = webRequest.GetResponse())
            using (var dataStream = webResponse.GetResponseStream())
            using (var readerStream = new StreamReader(dataStream))
            {
                return readerStream.ReadToEnd();
            }
        }

        public static async Task<string> SendWebRequestAsync(string url, string data, string methodType, string contentType)
        {
            if (url.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (methodType.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(url));
            }

            var webRequest = (HttpWebRequest)WebRequest.Create(url);

            webRequest.Method = methodType;
            webRequest.ContentType = contentType;

            var buffer = Encoding.UTF8.GetBytes(data);

            webRequest.ContentLength = buffer.Length;

            //ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidation;

            //  Send request
            using (var requestStream = await webRequest.GetRequestStreamAsync())
            {
                await requestStream.WriteAsync(buffer, 0, buffer.Length);
            }

            //  Get response
            using (var webResponse = await webRequest.GetResponseAsync())
            using (var dataStream = webResponse.GetResponseStream())
            using (var readerStream = new StreamReader(dataStream))
            {
                return await readerStream.ReadToEndAsync();
            }
        }

        private static bool RemoteCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None;
        }
    }
}