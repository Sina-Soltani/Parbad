using Parbad.Abstraction;
using Parbad.Gateway.Tara.Internal.Models;
using Parbad.InvoiceBuilder;
using System;
using System.Collections.Generic;

namespace Parbad.Gateway.Tara
{
    public static class TaraGatewayInvoiceBuilderExtensions
    {
        private const string RequestAdditionalDataKey = "TaraRequestAdditionalData";

        /// <summary>
        /// The invoice will be sent to Tara gateway.
        /// </summary>
        /// <param name="builder"></param>
        public static IInvoiceBuilder UseTara(this IInvoiceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.SetGateway(TaraGateway.Name);
        }

        /// <summary>
        /// Sets additional data for Tara gateway request.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="additionalData"></param>
        public static IInvoiceBuilder SetTaraData(this IInvoiceBuilder builder, TaraRequestAdditionalData additionalData)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (additionalData == null) throw new ArgumentNullException(nameof(additionalData));

            return builder.AddOrUpdateProperty(RequestAdditionalDataKey, additionalData);
        }

        /// <summary>
        /// Sets additional data for Tara gateway request using a configuration action.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureData">Action to configure the additional data</param>
        public static IInvoiceBuilder SetTaraData(this IInvoiceBuilder builder, Action<TaraRequestAdditionalData> configureData)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configureData == null) throw new ArgumentNullException(nameof(configureData));

            var data = new TaraRequestAdditionalData();
            configureData(data);

            return builder.AddOrUpdateProperty(RequestAdditionalDataKey, data);
        }

        /// <summary>
        /// Adds a service amount to the Tara gateway request.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="serviceId">Service identifier</param>
        /// <param name="amount">Amount for this service</param>
        public static IInvoiceBuilder AddTaraServiceAmount(this IInvoiceBuilder builder, long serviceId, long amount)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.ChangeProperties(properties =>
            {
                var data = GetOrCreateTaraData(properties);
                data.ServiceAmountList.Add(new TaraServiceAmount
                {
                    serviceId = serviceId,
                    amount = amount
                });
            });
        }

        /// <summary>
        /// Adds multiple service amounts to the Tara gateway request.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="serviceAmounts">List of service amounts</param>
        public static IInvoiceBuilder AddTaraServiceAmounts(this IInvoiceBuilder builder, IEnumerable<TaraServiceAmount> serviceAmounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (serviceAmounts == null) throw new ArgumentNullException(nameof(serviceAmounts));

            return builder.ChangeProperties(properties =>
            {
                var data = GetOrCreateTaraData(properties);
                data.ServiceAmountList.AddRange(serviceAmounts);
            });
        }

        /// <summary>
        /// Adds an invoice item to the Tara gateway request.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="item">Invoice item to add</param>
        public static IInvoiceBuilder AddTaraInvoiceItem(this IInvoiceBuilder builder, TaraInvoiceItem item)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (item == null) throw new ArgumentNullException(nameof(item));

            return builder.ChangeProperties(properties =>
            {
                var data = GetOrCreateTaraData(properties);
                data.InvoiceItemList.Add(item);
            });
        }

        /// <summary>
        /// Adds multiple invoice items to the Tara gateway request.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="items">List of invoice items</param>
        public static IInvoiceBuilder AddTaraInvoiceItems(this IInvoiceBuilder builder, IEnumerable<TaraInvoiceItem> items)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (items == null) throw new ArgumentNullException(nameof(items));

            return builder.ChangeProperties(properties =>
            {
                var data = GetOrCreateTaraData(properties);
                data.InvoiceItemList.AddRange(items);
            });
        }

        /// <summary>
        /// Sets the mobile number for the Tara gateway request.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="mobile">Mobile number</param>
        public static IInvoiceBuilder SetTaraMobile(this IInvoiceBuilder builder, string mobile)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.ChangeProperties(properties =>
            {
                var data = GetOrCreateTaraData(properties);
                data.Mobile = mobile;
            });
        }

        /// <summary>
        /// Sets the additional data string for the Tara gateway request.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="additionalData">Additional data string</param>
        public static IInvoiceBuilder SetTaraAdditionalData(this IInvoiceBuilder builder, string additionalData)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.ChangeProperties(properties =>
            {
                var data = GetOrCreateTaraData(properties);
                data.AdditionalData = additionalData;
            });
        }

        /// <summary>
        /// Sets the VAT amount for the Tara gateway request.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="vat">VAT amount</param>
        public static IInvoiceBuilder SetTaraVat(this IInvoiceBuilder builder, long vat)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.ChangeProperties(properties =>
            {
                var data = GetOrCreateTaraData(properties);
                data.Vat = vat;
            });
        }

        internal static TaraRequestAdditionalData GetTaraRequestAdditionalData(this Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            if (invoice.Properties.TryGetValue(RequestAdditionalDataKey, out var additionalData))
            {
                return (TaraRequestAdditionalData)additionalData;
            }

            return null;
        }

        private static TaraRequestAdditionalData GetOrCreateTaraData(IDictionary<string, object> properties)
        {
            if (properties.TryGetValue(RequestAdditionalDataKey, out var existingData))
            {
                return (TaraRequestAdditionalData)existingData;
            }

            var data = new TaraRequestAdditionalData();
            properties[RequestAdditionalDataKey] = data;
            return data;
        }
    }
}

