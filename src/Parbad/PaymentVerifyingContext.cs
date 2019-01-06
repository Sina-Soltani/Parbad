using System;
using Parbad.Providers;
using Parbad.Utilities;

namespace Parbad
{
    internal class PaymentVerifyingContext : IPaymentVerifyingContext
    {
        public PaymentVerifyingContext(Gateway gateway, long orderNumber, string referenceId)
        {
            if (orderNumber <= 0) throw new ArgumentOutOfRangeException(nameof(orderNumber));
            if (referenceId.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(referenceId));

            Gateway = gateway;
            OrderNumber = orderNumber;
            ReferenceId = referenceId;
        }

        public Gateway Gateway { get; }

        public long OrderNumber { get; }

        public string ReferenceId { get; }

        public bool IsCanceled { get; set; }

        public string CancellationReason { get; set; }

        public void Cancel(string reason)
        {
            IsCanceled = true;

            CancellationReason = reason;
        }
    }
}