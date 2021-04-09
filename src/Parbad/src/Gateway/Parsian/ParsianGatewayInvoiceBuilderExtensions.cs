using Parbad.Abstraction;
using Parbad.InvoiceBuilder;
using System;

namespace Parbad.Gateway.Parsian
{
    public static class ParsianGatewayInvoiceBuilderExtensions
    {
        private static string AdditionalDataKey => "ParsianAdditionalData";

        /// <summary>
        /// The invoice will be sent to Parsian gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseParsian(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(ParsianGateway.Name);
        }

        /// <summary>
        /// Sets some additional data for Parsian Gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="additionalData"></param>
        public static IInvoiceBuilder SetParsianAdditionalData(this IInvoiceBuilder builder, ParsianGatewayAdditionalDataRequest additionalData)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (additionalData == null) throw new ArgumentNullException(nameof(additionalData));

            builder.AddOrUpdateProperty(AdditionalDataKey, additionalData);

            return builder;
        }

        internal static ParsianGatewayAdditionalDataRequest GetParsianAdditionalData(this Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            if (invoice.Properties.ContainsKey(AdditionalDataKey))
            {
                return (ParsianGatewayAdditionalDataRequest)invoice.Properties[AdditionalDataKey];
            }

            return null;
        }
    }
}
