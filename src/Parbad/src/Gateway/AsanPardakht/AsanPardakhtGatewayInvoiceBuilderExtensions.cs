using System;
using Parbad;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.AsanPardakht
{
    public static class AsanPardakhtGatewayInvoiceBuilderExtensions
    {
        public static IInvoiceBuilder UseAsanPardakht(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(AsanPardakhtGateway.Name);
        }
    }
}