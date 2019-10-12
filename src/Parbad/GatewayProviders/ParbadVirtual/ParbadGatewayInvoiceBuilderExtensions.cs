using System;
using Parbad.GatewayProviders.ParbadVirtual;
using Parbad.InvoiceBuilder;

namespace Parbad
{
    public static class ParbadGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Parbad Virtual gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseParbadVirtual(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.UseGateway(ParbadVirtualGateway.Name);
        }
    }
}
