namespace Parbad.Core
{
    /// <summary>
    /// Request result status.
    /// </summary>
    public enum RequestResultStatus
    {
        /// <summary>
        /// Request failed.
        /// </summary>
        Failed = 0,

        /// <summary>
        /// Request succeed.
        /// </summary>
        Success = 1,

        /// <summary>
        /// Request failed because Order Number is used before.
        /// </summary>
        DuplicateOrderNumber = 2
    }
}