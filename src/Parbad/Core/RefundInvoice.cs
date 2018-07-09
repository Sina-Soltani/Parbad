using System;

namespace Parbad.Core
{
    /// <summary>
    /// RefundInvoice class to refund a payment.
    /// </summary>
    public class RefundInvoice
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes RefundInvoice class with Order Number of payment.
        /// </summary>
        /// <param name="orderNumber">Order Number of payment that must be refund.</param>
        public RefundInvoice(long orderNumber) : this(orderNumber, 0)
        {
        }

        /// <summary>
        /// Initializes RefundInvoice class with Order Number of payment and amount. To refund the complete amount of payment, set 0 (zero) or use first constructor.
        /// </summary>
        /// <param name="orderNumber">Order Number of payment that must be refund.</param>
        /// <param name="amountToRefund">Amount to refund. To refund the complete amount of payment, set 0 (zero) or use first constructor.</param>
        public RefundInvoice(long orderNumber, long amountToRefund)
        {
            if (orderNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(orderNumber), "Order Number must be a positive number.");
            }

            if (amountToRefund < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amountToRefund), "Amount of Refund cannot be a negative number.");
            }

            OrderNumber = orderNumber;
            AmountToRefund = amountToRefund;
        }

        /// <summary>
        /// Order Number of payment
        /// </summary>
        public long OrderNumber { get; }

        /// <summary>
        /// Amount to refund
        /// </summary>
        public long AmountToRefund { get; }
    }
}