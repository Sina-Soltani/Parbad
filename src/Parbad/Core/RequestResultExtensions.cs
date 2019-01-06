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
        [Obsolete("This method is deprecated. Use RedirectToGateway method instead.")]
        public static void Process(this RequestResult requestResult, HttpContext httpContext)
        {
            RedirectToGateway(requestResult, httpContext);
        }

        /// <summary>
        /// Redirects the client to the gateway Website.
        /// </summary>
        /// <param name="requestResult"></param>
        /// <param name="httpContext">The current HttpContext object.</param>
        /// <param name="onRequestFailedHandler"></param>
        [Obsolete("This method is deprecated. Use RedirectToGateway method instead.")]
        public static void Process(this RequestResult requestResult, HttpContext httpContext, Action onRequestFailedHandler)
        {
            RedirectToGateway(requestResult, httpContext, onRequestFailedHandler);
        }

        /// <summary>
        /// Redirects the client to the gateway Website.
        /// </summary>
        /// <param name="requestResult">RequestResult object</param>
        /// <param name="httpContext">HttpContext object</param>
        [Obsolete("This method is deprecated. Use RedirectToGateway method instead.")]
        public static void Process(this RequestResult requestResult, HttpContextBase httpContext)
        {
            RedirectToGateway(requestResult, httpContext);
        }

        /// <summary>
        /// Redirects the client to the gateway Website.
        /// </summary>
        /// <param name="requestResult">RequestResult object</param>
        /// <param name="httpContext">HttpContext object</param>
        /// <param name="onRequestFailedHandler">If RequestResult is not successful then this delegate will be invoke.</param>
        [Obsolete("This method is deprecated. Use RedirectToGateway method instead.")]
        public static void Process(this RequestResult requestResult, HttpContextBase httpContext, Action onRequestFailedHandler)
        {
            RedirectToGateway(requestResult, httpContext, onRequestFailedHandler);
        }

        /// <summary>
        /// Redirects the client to gateway.
        /// </summary>
        /// <param name="requestResult"></param>
        /// <param name="httpContext">The current HttpContext object.</param>
        /// <param name="onFailuresHandler">Will be invoke if <see cref="RequestResultStatus"/> does not equal to <see cref="RequestResultStatus.Success"/></param>>
        public static void RedirectToGateway(this RequestResult requestResult, HttpContext httpContext, Action onFailuresHandler = null)
        {
            RedirectToGateway(requestResult, new HttpContextWrapper(httpContext), onFailuresHandler);
        }

        /// <summary>
        /// Redirects the client to gateway.
        /// </summary>
        /// <param name="requestResult"></param>
        /// <param name="httpContext">The current HttpContext object.</param>
        /// <param name="onFailuresHandler">Will be invoke if <see cref="RequestResultStatus"/> does not equal to <see cref="RequestResultStatus.Success"/></param>>
        public static void RedirectToGateway(this RequestResult requestResult, HttpContextBase httpContext, Action onFailuresHandler = null)
        {
            if (requestResult == null) throw new ArgumentNullException(nameof(requestResult));
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            if (requestResult.IsSuccess())
            {
                if (requestResult.BehaviorMode == GatewayRequestBehaviorMode.Redirect)
                {
                    httpContext.Response.Redirect(requestResult.Content);
                }
                else
                {
                    httpContext.Response.Write(requestResult.Content);
                }

                return;
            }

            if (onFailuresHandler != null)
            {
                onFailuresHandler();
                return;
            }

            httpContext.Response.Write(requestResult.Message);
        }
    }
}