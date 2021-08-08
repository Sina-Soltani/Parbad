using System;
using Parbad.Gateway.Pasargad;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.FanAva
{
    public static class FanAvaGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to FanAva gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseFanAva(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(FanAvaGateway.Name);
        }
    }
}