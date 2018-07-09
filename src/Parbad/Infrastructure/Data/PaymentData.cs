using System;
using Parbad.Providers;

namespace Parbad.Infrastructure.Data
{
    [Serializable]
    public sealed class PaymentData
    {
        public Guid Id { get; set; }

        public Gateway Gateway { get; set; }

        public long OrderNumber { get; set; }

        public long Amount { get; set; }

        public string ReferenceId { get; set; }

        public string TransactionId { get; set; }

        public PaymentDataStatus Status { get; set; }

        public string AdditionalData { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}