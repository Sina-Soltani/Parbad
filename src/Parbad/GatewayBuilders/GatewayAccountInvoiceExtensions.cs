using System;
using Parbad.Abstraction;
using Parbad.Internal;
using Parbad.InvoiceBuilder;

namespace Parbad
{
    public static class GatewayAccountInvoiceExtensions
    {
        /// <summary>
        /// Gateway Account key in <see cref="Invoice.AdditionalData"/> property.
        /// </summary>
        public const string GatewayAccountKeyName = "AccountName";

        /// <summary>
        /// Uses the given account to communicate with the gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="accountName">Name of the account.</param>
        public static IInvoiceBuilder UseAccount(this IInvoiceBuilder builder, string accountName)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (accountName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(accountName));

            builder.AddAdditionalData(GatewayAccountKeyName, accountName);

            return builder;
        }

        /// <summary>
        /// Gets the account name if specified.
        /// </summary>
        /// <param name="invoice"></param>
        public static string GetAccountName(this Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            if (!invoice.AdditionalData.ContainsKey(GatewayAccountKeyName)) return null;

            return (string)invoice.AdditionalData[GatewayAccountKeyName];
        }
    }
}
