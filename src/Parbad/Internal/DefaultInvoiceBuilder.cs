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
        private Money _amount;
        private CallbackUrl _url;
        private readonly IDictionary<string, object> _additionalData;
        private Type _gatewayType;

        public DefaultInvoiceBuilder(IServiceProvider services)
        {
            Services = services;

            _additionalData = new Dictionary<string, object>();
        }

        public IServiceProvider Services { get; }

        public virtual IInvoiceBuilder SetTrackingNumberProvider(ITrackingNumberProvider provider)
        {
            _trackingNumberProvider = provider;

            return this;
        }

        public virtual IInvoiceBuilder SetAmount(Money amount)
        {
            _amount = amount;

            return this;
        }

        public virtual IInvoiceBuilder SetCallbackUrl(CallbackUrl callbackUrl)
        {
            _url = callbackUrl;

            return this;
        }

        public virtual IInvoiceBuilder AddAdditionalData(string key, object value)
        {
            _additionalData.Add(key, value);

            return this;
        }

        public virtual IInvoiceBuilder SetGatewayType(Type gatewayType)
        {
            GatewayHelper.IsGateway(gatewayType, throwException: true);

            _gatewayType = gatewayType;

            return this;
        }

        public virtual async Task<Invoice> BuildAsync(CancellationToken cancellationToken = default)
        {
            if (_trackingNumberProvider == null)
            {
                throw new Exception("No TrackingNumberProvider is set. A TrackingNumberProvider is needed for generating a tracking number.");
            }

            var trackingNumber = await _trackingNumberProvider.ProvideAsync(cancellationToken).ConfigureAwaitFalse();

            return new Invoice(
                trackingNumber,
                _amount,
                _url,
                _gatewayType,
                _additionalData
            );
        }
    }
}
