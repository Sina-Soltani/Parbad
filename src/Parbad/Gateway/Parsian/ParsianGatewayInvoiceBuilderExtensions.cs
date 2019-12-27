using System;
using Parbad.Gateway.Parsian;
using Parbad.InvoiceBuilder;

namespace Parbad
{
    public static class ParsianGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Parsian gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseParsian(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(ParsianGateway.Name);
        }
    }
}
