// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Abstraction;
using Parbad.InvoiceBuilder;
using Parbad.TrackingNumberProviders;

namespace Parbad.Internal
{
    /// <inheritdoc />
    public class DefaultInvoiceBuilder : IInvoiceBuilder
    {
        private ITrackingNumberProvider _trackingNumberProvider;
        private Type _gatewayType;

        /// <summary>
        /// Initializes an instance of <see cref="DefaultInvoiceBuilder"/> class.
        /// </summary>
        /// <param name="services"></param>
        public DefaultInvoiceBuilder(IServiceProvider services)
        {
            Services = services;

            AdditionalData = new Dictionary<string, object>();
        }

        /// <inheritdoc />
        public long TrackingNumber { get; set; }

        /// <inheritdoc />
        public Money Amount { get; set; }

        /// <inheritdoc />
        public CallbackUrl CallbackUrl { get; set; }

        /// <inheritdoc />
        public Type GatewayType
        {
            get => _gatewayType;
            set
            {
                GatewayHelper.IsGateway(value, throwException: true);

                _gatewayType = value;
            }
        }

        /// <inheritdoc />
        public IDictionary<string, object> AdditionalData { get; set; }

        /// <inheritdoc />
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public virtual IInvoiceBuilder SetTrackingNumberProvider(ITrackingNumberProvider provider)
        {
            _trackingNumberProvider = provider;

            return this;
        }

        /// <inheritdoc />
        public virtual IInvoiceBuilder SetAmount(Money amount)
        {
            Amount = amount;

            return this;
        }

        /// <inheritdoc />
        public virtual IInvoiceBuilder SetCallbackUrl(CallbackUrl callbackUrl)
        {
            CallbackUrl = callbackUrl;

            return this;
        }

        /// <inheritdoc />
        public virtual IInvoiceBuilder AddAdditionalData(string key, object value)
        {
            AdditionalData.Add(key, value);

            return this;
        }

        /// <inheritdoc />
        public virtual IInvoiceBuilder SetGatewayType(Type gatewayType)
        {
            GatewayType = gatewayType;

            return this;
        }

        /// <inheritdoc />
        public virtual async Task<Invoice> BuildAsync(CancellationToken cancellationToken = default)
        {
            if (_trackingNumberProvider != null)
            {
                TrackingNumber = await _trackingNumberProvider.ProvideAsync(cancellationToken).ConfigureAwaitFalse();
            }

            return new Invoice(
                TrackingNumber,
                Amount,
                CallbackUrl,
                _gatewayType,
                AdditionalData
            );
        }
    }
}
