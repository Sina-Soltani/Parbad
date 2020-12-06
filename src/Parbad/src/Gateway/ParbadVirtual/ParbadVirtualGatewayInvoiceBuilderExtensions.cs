using System;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.ParbadVirtual
{
    public static class ParbadVirtualGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Parbad Virtual gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseParbadVirtual(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(ParbadVirtualGateway.Name);
        }
    }
}
