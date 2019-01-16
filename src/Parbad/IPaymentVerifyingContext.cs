using Parbad.Providers;

namespace Parbad
{
    /// <summary>
    /// Describes the invoice which sent by the gateway. You can compare its data with your database and also cancel the payment operation if you need.
    /// </summary>
    public interface IPaymentVerifyingContext
    {
        /// <summary>
        /// The gateway, that sent the invoice.
        /// </summary>
        Gateway Gateway { get; }

        /// <summary>
        /// Order Number of payment.
        /// </summary>
        long OrderNumber { get; }

        /// <summary>
        /// Reference ID of payment.
        /// </summary>
        string ReferenceId { get; }

        /// <summary>
        /// Cancel the operation. No Verifying request will be sent to the gateway. In this case, the money will transferred back to the client's bank account after about 15-60 minutes.
        /// </summary>
        /// <param name="reason">The reason for cancelling the operation.</param>
        void Cancel(string reason = null);
    }
}