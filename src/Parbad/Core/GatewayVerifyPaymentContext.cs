using System;
using Parbad.Utilities;

namespace Parbad.Core
{
    internal class GatewayVerifyPaymentContext
    {
        public GatewayVerifyPaymentContext(long orderNumber, Money amount, string referenceId, DateTime createdOn, string additionalData, IRequestParameters requestParameters)
        {
            if (orderNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(orderNumber));
            }

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (referenceId.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(referenceId));
            }

            OrderNumber = orderNumber;
            Amount = amount;
            ReferenceId = referenceId;
            CreatedOn = createdOn;
            AdditionalData = additionalData;
            RequestParameters = requestParameters ?? throw new ArgumentNullException(nameof(requestParameters));
        }

        public long OrderNumber { get; }

        public Money Amount { get; }

        public string ReferenceId { get; }

        public DateTime CreatedOn { get; }

        public string AdditionalData { get; }

        public IRequestParameters RequestParameters { get; }
    }
}