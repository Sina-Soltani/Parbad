using System;
using Parbad.GatewayProviders.Mellat;
using Parbad.InvoiceBuilder;

namespace Parbad
{
    public static class MellatGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Mellat gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseMellat(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGatewayType<MellatGateway>();
        }
    }
}
