using System;
using Parbad.Exceptions;
using Parbad.Utilities;

namespace Parbad.Core
{
    /// <summary>
    /// Invoice class
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// Initializes new Invoice object.
        /// </summary>
        /// <param name="orderNumber">Order number. Must be unique for each requests.</param>
        /// <param name="amount"></param>
        /// <param name="callBackUrl"></param>
        public Invoice(long orderNumber, long amount, string callBackUrl)
        {
            if (orderNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(orderNumber));
            }

            OrderNumber = orderNumber;

            if (amount <= 0)
            {
                throw new AmountValidationException(amount);
            }

            Amount = amount;

            if (callBackUrl.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(callBackUrl));
            }

            if (!CommonTools.IsValidUrl(callBackUrl))
            {
                throw new CallbackUrlValidationException(callBackUrl);
            }

            CallbackUrl = callBackUrl;
        }

        /// <summary>
        /// Get order number.
        /// </summary>
        public long OrderNumber { get; }

        public long Amount { get; }

        public string CallbackUrl { get; internal set; }
    }
}