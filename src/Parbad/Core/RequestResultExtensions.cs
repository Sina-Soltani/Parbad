using System;
using System.Web;

namespace Parbad.Core
{
    /// <summary>
    /// Extensions for RequestResult object.
    /// </summary>
    public static class RequestResultExtensions
    {
        /// <summary>
        /// Processes RequestResult object. If result is successful then prepares the gateway, otherwise it decides to write message to the HttpResponse.
        /// </summary>
        /// <param name="requestResult">RequestResult object</param>
        /// <param name="httpContext">HttpContext object</param>
        public static void Process(this RequestResult requestResult, HttpContext httpContext)
        {
            Process(requestResult, httpContext, null);
        }

        /// <summary>
        /// Processes RequestResult object. If result is successful then prepares the gateway, otherwise it decides to write message to the HttpResponse or invoke your onRequestFailedHandler.
        /// </summary>
        /// <param name="requestResult">RequestResult object</param>
        /// <param name="httpContext">HttpContext object</param>
        /// <param name="onRequestFailedHandler">If RequestResult is not successful then this delegate will be invoke.</param>
        public static void Process(this RequestResult requestResult, HttpContext httpContext, Action onRequestFailedHandler)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (!requestResult.IsSuccess())
            {
                if (onRequestFailedHandler != null)
                {
                    onRequestFailedHandler();
                    return;
                }

                httpContext.Response.Write(requestResult.Content);

                return;
            }

            if (requestResult.BehaviorMode == GatewayRequestBehaviorMode.Redirect)
            {
                httpContext.Response.Redirect(requestResult.Content);
            }
            else
            {
                httpContext.Response.Write(requestResult.Content);
            }
        }
    }
}