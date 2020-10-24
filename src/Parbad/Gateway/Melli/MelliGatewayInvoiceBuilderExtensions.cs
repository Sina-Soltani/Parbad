using System;
using Parbad.Gateway.Melli.Internal;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.Melli
{
    /// <summary>
    ///     Melli gate way extension that use in startup
    /// </summary>
    public static class MelliGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        ///     The invoice will be sent to Melli gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseMelli(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(MelliGateway.Name);
        }

        /// <summary>
        ///     The invoice will be sent to Melli gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureMelliInvoiceBuilder">Helps to build the extra functions of Melli Gateway.</param>
        public static IInvoiceBuilder UseMelli(this IInvoiceBuilder builder,
            Action<IMelliGatewayInvoiceBuilder> configureMelliInvoiceBuilder)
        {
            if (configureMelliInvoiceBuilder == null)
                throw new ArgumentNullException(nameof(configureMelliInvoiceBuilder));

            UseMelli(builder);

            configureMelliInvoiceBuilder(new MelliGatewayInvoiceBuilder(builder));

            return builder;
        }
    }
}