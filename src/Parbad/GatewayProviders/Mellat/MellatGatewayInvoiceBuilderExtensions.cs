using System;
using Parbad.GatewayProviders.Mellat;
using Parbad.GatewayProviders.Mellat.Internal;
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

            return builder.UseGateway(MellatGateway.Name);
        }

        /// <summary>
        /// Contains extra functions for Mellat Gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureMellatInvoiceBuilder">Helps to build the extra functions of Mellat Gateway.</param>
        public static IInvoiceBuilder UseMellat(this IInvoiceBuilder builder, Action<IMellatGatewayInvoiceBuilder> configureMellatInvoiceBuilder)
        {
            builder.UseMellat();

            configureMellatInvoiceBuilder(new MellatGatewayInvoiceBuilder(builder));

            return builder;
        }
    }
}
