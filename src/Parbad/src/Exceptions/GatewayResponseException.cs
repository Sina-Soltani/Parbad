// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;

namespace Parbad.Exceptions
{
    /// <summary>
    /// The gateway returned a response that cannot be read, for example an HTML error page from a proxy, an empty
    /// body or invalid JSON.
    /// <para>
    /// It is not known whether the bank processed the call, so the payment is not recorded as failed: it stays
    /// open and the verification can be retried. It derives from <see cref="HttpRequestException"/>, so code that
    /// already handles network errors handles it too.
    /// </para>
    /// </summary>
    public class GatewayResponseException : HttpRequestException
    {
        /// <summary>
        /// Initializes an instance of <see cref="GatewayResponseException"/>.
        /// </summary>
        public GatewayResponseException(HttpStatusCode statusCode, string contentType, string reason)
            : base($"The gateway returned a response that cannot be read ({reason}). HTTP status: {(int)statusCode} {statusCode}. Content type: {contentType ?? "none"}.")
        {
            ResponseStatusCode = statusCode;
            ContentType = contentType;
        }

        /// <summary>
        /// The HTTP status code of the response.
        /// </summary>
        public HttpStatusCode ResponseStatusCode { get; }

        /// <summary>
        /// The content type of the response, if any.
        /// </summary>
        public string ContentType { get; }
    }
}
