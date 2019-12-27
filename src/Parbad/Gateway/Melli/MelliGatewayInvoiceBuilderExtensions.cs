using System;
using Parbad.Gateway.Melli;
using Parbad.InvoiceBuilder;

namespace Parbad
{
    public static class MelliGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Melli gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseMelli(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.UseGateway(MelliGateway.Name);
        }
    }
}
