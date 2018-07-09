using System;

namespace Parbad.Core
{
    internal class GatewayRefundPaymentContext
    {
        public GatewayRefundPaymentContext(long orderNumber, Money amount, string referenceId, string transactionId, string additionalData)
        {
            if (orderNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(orderNumber));
            }

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            OrderNumber = orderNumber;
            Amount = amount;
            ReferenceId = referenceId;
            TransactionId = transactionId;
            AdditionalData = additionalData;
        }

        public long OrderNumber { get; }

        public Money Amount { get; }

        public string ReferenceId { get; }

        public string TransactionId { get; }

        public string AdditionalData { get; }
    }
}