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
        /// <param name="amount">Amount to pay (IR-Rial)</param>
        /// <param name="callBackUrl">
        /// A complete URL in your website to verify the Invoice. Clients will come back to this URL after they pay the money in the Gateway.
        /// <para>A complete URL would be like: "http://www.mywebsite.com/payment/verify"</para>
        /// </param>
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

        /// <summary>
        /// Amount to pay (IR-Rial)
        /// </summary>
        public long Amount { get; }

        /// <summary>
        /// A complete URL in your website to verify the Invoice. Clients will come back to this URL for verifying the Invoice, after they pay the money in the Gateway.
        /// <para>A complete URL would be like: "http://www.mywebsite.com/payment/verify"</para>
        /// </summary>
        public string CallbackUrl { get; internal set; }
    }
}