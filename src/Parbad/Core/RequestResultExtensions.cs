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
        /// Redirects the client to the gateway Website.
        /// </summary>
        /// <param name="requestResult"></param>
        /// <param name="httpContext">The current HttpContext object.</param>
        public static void Process(this RequestResult requestResult, HttpContext httpContext)
        {
            Process(requestResult, new HttpContextWrapper(httpContext));
        }

        /// <summary>
        /// Redirects the client to the gateway Website.
        /// </summary>
        /// <param name="requestResult"></param>
        /// <param name="httpContext">The current HttpContext object.</param>
        /// <param name="onRequestFailedHandler"></param>
        public static void Process(this RequestResult requestResult, HttpContext httpContext, Action onRequestFailedHandler)
        {
            Process(requestResult, new HttpContextWrapper(httpContext), onRequestFailedHandler);
        }

        /// <summary>
        /// Redirects the client to the gateway Website.
        /// </summary>
        /// <param name="requestResult">RequestResult object</param>
        /// <param name="httpContext">HttpContext object</param>
        public static void Process(this RequestResult requestResult, HttpContextBase httpContext)
        {
            Process(requestResult, httpContext, null);
        }

        /// <summary>
        /// Redirects the client to the gateway Website.
        /// </summary>
        /// <param name="requestResult">RequestResult object</param>
        /// <param name="httpContext">HttpContext object</param>
        /// <param name="onRequestFailedHandler">If RequestResult is not successful then this delegate will be invoke.</param>
        public static void Process(this RequestResult requestResult, HttpContextBase httpContext, Action onRequestFailedHandler)
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