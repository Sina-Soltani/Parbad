using Parbad.Abstraction;
using Parbad.InvoiceBuilder;
using System;
using System.Collections.Generic;

namespace Parbad.Gateway.FanAva
{
    public static class FanAvaGatewayInvoiceBuilderExtensions
    {
        /// <summary>
        /// The invoice will be sent to <see cref="FanAvaGateway"/>.
        /// </summary>
        public static IInvoiceBuilder UseFanAva(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(FanAvaGateway.Name);
        }

        /// <summary>
        /// Sets the additional data for <see cref="FanAvaGateway"/>.
        /// </summary>
        public static IInvoiceBuilder SetFanAvaAdditionalData(
            this IInvoiceBuilder builder,
            FanAvaGatewayAdditionalDataRequestType type = FanAvaGatewayAdditionalDataRequestType.Goods,
            string mobileNumber = null,
            string email = null,
            string goodsReferenceId = null,
            string merchantGoodReferenceId = null,
            IEnumerable<FanAvaGatewayApportionmentAccount> apportionmentAccountList = null)
        {
            return SetFanAvaAdditionalData(builder, new FanAvaGatewayAdditionalDataRequest
            {
                Type = type,
                MobileNumber = mobileNumber,
                Email = email,
                GoodsReferenceId = goodsReferenceId,
                MerchantGoodReferenceId = merchantGoodReferenceId,
                ApportionmentAccountList = apportionmentAccountList
            });
        }

        /// <summary>
        /// Sets the additional data for <see cref="FanAvaGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="additionalData"></param>
        public static IInvoiceBuilder SetFanAvaAdditionalData(this IInvoiceBuilder builder, FanAvaGatewayAdditionalDataRequest additionalData)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (additionalData == null) throw new ArgumentNullException(nameof(additionalData));

            builder.AddOrUpdateProperty(FanAvaGatewayAdditionalDataRequest.InvoicePropertyKey, additionalData);

            return builder;
        }

        internal static FanAvaGatewayAdditionalDataRequest GetFanAvaAdditionalData(this Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            if (invoice.Properties.ContainsKey(FanAvaGatewayAdditionalDataRequest.InvoicePropertyKey))
            {
                return (FanAvaGatewayAdditionalDataRequest)invoice.Properties[FanAvaGatewayAdditionalDataRequest.InvoicePropertyKey];
            }

            return null;
        }
    }
}
