using System;
using Parbad.Gateway.AsanPardakht;
using Parbad.InvoiceBuilder;

namespace Parbad
{
    public static class AsanPardakhtGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Asan Pardakht gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseAsanPardakht(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(AsanPardakhtGateway.Name);
        }
    }
}
