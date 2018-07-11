namespace Parbad.Core
{
    /// <summary>
    /// Verify result status.
    /// </summary>
    public enum VerifyResultStatus
    {
        /// <summary>
        /// Verify failed.
        /// </summary>
        Failed = 0,

        /// <summary>
        /// Verify succeed.
        /// </summary>
        Success = 1,

        /// <summary>
        /// Verify already verified.
        /// </summary>
        AlreadyVerified = 2,

        /// <summary>
        /// The time for end-user to pay is expired.
        /// </summary>
        PaymentTimeExpired = 3,

        /// <summary>
        /// Payment is not valid.
        /// </summary>
        NotValid = 4,

        /// <summary>
        /// Payment verification is canceled by calling VerifyInvoice.Cancel() method.
        /// </summary>
        CanceledProgrammatically = 5
    }
}