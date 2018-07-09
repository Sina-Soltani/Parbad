namespace Parbad.Core
{
    /// <summary>
    /// Request's result details.
    /// </summary>
    public class RequestResult
    {
        internal RequestResult(RequestResultStatus status, string message) : this(status, message, string.Empty)
        {
        }

        internal RequestResult(RequestResultStatus status, string message, string referenceId)
        {
            Status = status;
            Message = message;
            ReferenceId = referenceId;
        }

        /// <summary>
        /// Status of result.
        /// </summary>
        public RequestResultStatus Status { get; }

        /// <summary>
        /// A unique Reference Id that gateway sent.
        /// </summary>
        public string ReferenceId { get; }

        /// <summary>
        /// Result message
        /// </summary>
        public string Message { get; }

        internal GatewayRequestBehaviorMode BehaviorMode { get; set; }

        internal string Content { get; set; }

        internal string AdditionalData { get; set; }
    }
}