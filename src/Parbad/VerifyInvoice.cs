using System;
using Parbad.Providers;
using Parbad.Utilities;

namespace Parbad
{
    /// <summary>
    /// A context for checking the payment before finishing verify operation.
    /// </summary>
    public class VerifyInvoice
    {
        internal VerifyInvoice(Gateway gateway, long orderNumber, string referenceId)
        {
            if (orderNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(orderNumber));
            }

            if (referenceId.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(referenceId));
            }

            Gateway = gateway;
            OrderNumber = orderNumber;
            ReferenceId = referenceId;
        }

        /// <summary>
        /// Gateway of payment.
        /// </summary>
        public Gateway Gateway { get; }

        /// <summary>
        /// Order Number of payment.
        /// </summary>
        public long OrderNumber { get; }

        /// <summary>
        /// Reference ID of payment.
        /// </summary>
        public string ReferenceId { get; }

        internal bool IsCanceled { get; private set; }

        internal string CancellationReason { get; private set; }

        /// <summary>
        /// Cancel the operation.
        /// </summary>
        public void Cancel()
        {
            Cancel(null);
        }

        /// <summary>
        /// Cancel the operation.
        /// </summary>
        /// <param name="reason">Reason to cancel the operation.</param>
        public void Cancel(string reason)
        {
            IsCanceled = true;

            CancellationReason = reason;
        }
    }
}