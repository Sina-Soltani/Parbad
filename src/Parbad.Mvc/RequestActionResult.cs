using System;
using System.Web.Mvc;
using Parbad.Core;

namespace Parbad.Mvc
{
    /// <summary>
    /// An ActionResult for redirecting the client to the gateway.
    /// </summary>
    public class RequestActionResult : ActionResult
    {
        private readonly RequestResult _requestResult;
        private readonly Func<ActionResult> _onFailuresHandler;

        /// <summary>
        /// An ActionResult for redirecting the client to the gateway.
        /// </summary>
        /// <param name="requestResult">RequestResult object.</param>
        public RequestActionResult(RequestResult requestResult) : this(requestResult, null)
        {
        }

        /// <summary>
        /// An ActionResult for redirecting the client to the gateway.
        /// </summary>
        /// <param name="requestResult">RequestResult object.</param>
        /// <param name="onFailuresHandler">
        /// This handler would be called when RequestResult object failed.
        /// <para>For example you can return View("Error")</para>
        /// </param>
        public RequestActionResult(RequestResult requestResult, Func<ActionResult> onFailuresHandler)
        {
            if (requestResult == null)
            {
                throw new ArgumentNullException(nameof(requestResult));
            }

            _requestResult = requestResult;
            _onFailuresHandler = onFailuresHandler;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (_requestResult.Status != RequestResultStatus.Success && _onFailuresHandler != null)
            {
                var actionResult = _onFailuresHandler();

                if (actionResult == null)
                {
                    throw new Exception("onFailuresHandler returns null instead of ActionResult.");
                }

                actionResult.ExecuteResult(context);

                return;
            }

            _requestResult.Process(context.HttpContext.ApplicationInstance.Context);
        }
    }
}