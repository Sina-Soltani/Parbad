using System;
using Parbad.GatewayProviders.Saman;
using Parbad.InvoiceBuilder;

namespace Parbad
{
    public static class SamanGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Saman gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseSaman(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGatewayType<SamanGateway>();
        }
    }
}
