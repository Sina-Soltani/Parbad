using System;
using Parbad.Abstraction;
using Parbad.Gateway.Saman.Internal;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.Saman
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

            return builder.SetGateway(SamanGateway.Name);
        }

        /// <summary>
        /// Enables or disables Saman Mobile gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="enable">If true, the invoice will be sent to Saman Mobile gateway. Otherwise it will be sent to Saman Web gateway.</param>
        public static IInvoiceBuilder EnableSamanMobileGateway(this IInvoiceBuilder builder, bool enable = true)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.AddOrUpdateAdditionalData(SamanHelper.MobileGatewayKey, enable);

            return builder;
        }

        internal static bool IsSamanMobileGatewayEnabled(this Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            return invoice.Properties.ContainsKey(SamanHelper.MobileGatewayKey) &&
                   (bool)invoice.Properties[SamanHelper.MobileGatewayKey];
        }
    }
}
