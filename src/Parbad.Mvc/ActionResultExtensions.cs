using System;
using System.Web.Mvc;
using Parbad.Core;

namespace Parbad.Mvc
{
    /// <summary>
    /// Extensions for ActionResult object.
    /// </summary>
    public static class ActionResultExtensions
    {
        /// <summary>
        /// Converts the <see cref="RequestResult"/> object into an <see cref="ActionResult"/>.
        /// </summary>
        /// <param name="requestResult"></param>
        /// <param name="onFailuresHandler">Will be invoke if <see cref="RequestResultStatus"/> does not equal to <see cref="RequestResultStatus.Success"/></param>
        /// <returns></returns>
        public static ActionResult ToActionResult(this RequestResult requestResult, Func<ActionResult> onFailuresHandler = null)
        {
            return new RequestActionResult(requestResult, onFailuresHandler);
        }
    }
}