// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Parbad.Abstraction;
using Parbad.Gateway.Zibal.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.Zibal
{
    [Gateway(Name)]
    public class ZibalGateway : GatewayBase<ZibalGatewayAccount>
    {
        public const string Name = "Zibal";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly ZibalGatewayOptions _gatewayOptions;
        private readonly ParbadOptions _options;

        private static JsonSerializerSettings DefaultSerializerSettings => new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public ZibalGateway(
            IGatewayAccountProvider<ZibalGatewayAccount> accountProvider,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<ZibalGatewayOptions> gatewayOptions,
            IOptions<ParbadOptions> options) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(Name);
            _gatewayOptions = gatewayOptions.Value;
            _options = options.Value;
        }

        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var data = ZibalHelper.CreateRequestData(invoice, account);

            var responseMessage = await _httpClient
                .PostJsonAsync(_gatewayOptions.ApiRequestUrl, data, DefaultSerializerSettings, cancellationToken)
                .ConfigureAwaitFalse();

            return await ZibalHelper.CreateRequestResult(responseMessage, _httpContextAccessor.HttpContext, account, _gatewayOptions, _options.Messages);
        }

        public override Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ZibalHelper.CreateFetchResult(_httpContextAccessor.HttpContext.Request,
                                                                        context,
                                                                        _options.Messages,
                                                                        cancellationToken);
        }

        public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var data = ZibalHelper.CreateVerifyData(context.Transactions, account);

            var responseMessage = await _httpClient
                .PostJsonAsync(_gatewayOptions.ApiVerificationUrl, data, DefaultSerializerSettings, cancellationToken)
                .ConfigureAwaitFalse();

            return await ZibalHelper.CreateVerifyResult(responseMessage, _options.Messages);
        }

        public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            return PaymentRefundResult.Failed("The Refund operation is not supported by this gateway.").ToInterfaceAsync();
        }
    }
}
