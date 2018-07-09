using System;
using System.Web.Mvc;
using Parbad.Core;

namespace Parbad.Mvc
{
    public class RequestActionResult : ActionResult
    {
        private readonly RequestResult _requestResult;
        private readonly Func<ActionResult> _onFailuresHandler;

        public RequestActionResult(RequestResult payResult) : this(payResult, null)
        {
        }

        public RequestActionResult(RequestResult payResult, Func<ActionResult> onFailuresHandler)
        {
            if (payResult == null)
            {
                throw new ArgumentNullException(nameof(payResult));
            }

            _requestResult = payResult;
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