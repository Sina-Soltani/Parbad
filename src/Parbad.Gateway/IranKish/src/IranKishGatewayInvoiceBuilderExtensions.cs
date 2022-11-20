using System;
using Parbad.Abstraction;
using Parbad.Gateway.IranKish.Internal;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.IranKish
{
    public static class IranKishGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to Iran Kish gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseIranKish(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(IranKishGateway.Name);
        }

        /// <summary>
        /// Sets additional data for <see cref="IranKishGateway"/>.
        /// </summary>
        public static IInvoiceBuilder SetIranKishAdditionalData(this IInvoiceBuilder builder, IranKishGatewayAdditionalData data)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (!string.IsNullOrWhiteSpace(data.MobileNumberOrEmail))
            {
                builder.AddOrUpdateProperty(IranKishHelper.CmsPreservationIdKey, data.MobileNumberOrEmail);
            }

            return builder;
        }
    }
}
