using System;
using Parbad.GatewayProviders.IranKish;
using Parbad.InvoiceBuilder;

namespace Parbad
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

            return builder.SetGatewayType<IranKishGateway>();
        }
    }
}
