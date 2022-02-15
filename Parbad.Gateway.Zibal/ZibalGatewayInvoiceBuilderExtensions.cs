using System;
using Parbad.Abstraction;
using Parbad.Gateway.Zibal.Internal;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.Zibal
{
    public static class ZibalGatewayInvoiceBuilderExtensions
    {
      

        /// <summary>
        /// The invoice will be sent to Zibal Gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IInvoiceBuilder UseZibalPal(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(ZibalGateway.Name);
        }

        /// <summary>
        /// Sets ZarinPal Gateway data.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="zarinPalInvoice">Describes an invoice for ZarinPal gateway.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IInvoiceBuilder SetZibalData(this IInvoiceBuilder builder, Action<ZibalRequest> configureZibal)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configureZibal == null) throw new ArgumentNullException(nameof(configureZibal));

            var payPingRequest = new ZibalRequest();
            configureZibal(payPingRequest);

            builder.AddOrUpdateProperty(ZibalHelper.ZibalRequestAdditionalKeyName, payPingRequest);
            return builder;
        }

        internal static ZibalRequestModel GetZibalRequest(this Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            if (invoice.Properties.ContainsKey(ZibalHelper.ZibalRequestAdditionalKeyName))
            {
                return (ZibalRequestModel)invoice.Properties[ZibalHelper.ZibalRequestAdditionalKeyName];
            }

            return null;
        }
    }
}