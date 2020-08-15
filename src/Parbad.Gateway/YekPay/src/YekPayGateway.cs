// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Parbad.Abstraction;
using Parbad.Gateway.YekPay.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;

namespace Parbad.Gateway.YekPay
{
    /// <summary>
    /// Sepehr Gateway.
    /// </summary>
    [Gateway(Name)]
    public class YekPayGateway : GatewayBase<YekPayGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly ParbadOptions _options;

        public const string Name = "YekPay";

        private static JsonSerializerSettings DefaultSerializerSettings => new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        /// <summary>
        /// Initializes an instance of <see cref="YekPayGateway"/>.
        /// </summary>
        public YekPayGateway(
            IGatewayAccountProvider<YekPayGatewayAccount> accountProvider,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<ParbadOptions> options) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _options = options.Value;
        }

        /// <inheritdoc />
        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var data = YekPayHelper.CreateRequestData(invoice, account);

            var responseMessage = await _httpClient
                .PostJsonAsync(YekPayHelper.ApiRequestUrl, data, DefaultSerializerSettings, cancellationToken)
                .ConfigureAwaitFalse();

            return await YekPayHelper.CreateRequestResult(responseMessage, _httpContextAccessor.HttpContext, account, _options.Messages);
        }

        /// <inheritdoc />
        public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var data = YekPayHelper.CreateVerifyData(context.Transactions, account);

            var responseMessage = await _httpClient
                .PostJsonAsync(YekPayHelper.ApiVerifyUrl, data, DefaultSerializerSettings, cancellationToken)
                .ConfigureAwaitFalse();

            return await YekPayHelper.CreateVerifyResult(responseMessage, _options.Messages);
        }

        /// <inheritdoc />
        public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            return PaymentRefundResult.Failed("The Refund operation is not supported by this gateway.").ToInterfaceAsync();
        }
    }
}
