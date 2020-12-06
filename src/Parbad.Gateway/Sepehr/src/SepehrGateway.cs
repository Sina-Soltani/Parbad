// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Parbad.Abstraction;
using Parbad.Gateway.Sepehr.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.Sepehr
{
    /// <summary>
    /// Sepehr Gateway.
    /// </summary>
    [Gateway(Name)]
    public class SepehrGateway : GatewayBase<SepehrGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly SepehrGatewayOptions _gatewayOptions;
        private readonly ParbadOptions _options;

        public const string Name = "Sepehr";

        private static JsonSerializerSettings DefaultSerializerSettings => new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        /// <summary>
        /// Initializes an instance of <see cref="SepehrGateway"/>.
        /// </summary>
        public SepehrGateway(
            IGatewayAccountProvider<SepehrGatewayAccount> accountProvider,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<SepehrGatewayOptions> gatewayOptions,
            IOptions<ParbadOptions> options) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _gatewayOptions = gatewayOptions.Value;
            _options = options.Value;
        }

        /// <inheritdoc />
        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var data = SepehrHelper.CreateRequestData(invoice, account);

            var responseMessage = await _httpClient
                .PostJsonAsync(_gatewayOptions.ApiTokenUrl, data, DefaultSerializerSettings, cancellationToken)
                .ConfigureAwaitFalse();

            return await SepehrHelper.CreateRequestResult(responseMessage, _httpContextAccessor.HttpContext, account, _gatewayOptions, _options.Messages);
        }

        /// <inheritdoc />
        public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var callbackResult = await SepehrHelper.CreateCallbackResultAsync(
                    context,
                    _httpContextAccessor.HttpContext.Request,
                    account,
                    _options.Messages,
                    cancellationToken)
                .ConfigureAwaitFalse();

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            var data = SepehrHelper.CreateVerifyData(callbackResult, account);

            var responseMessage = await _httpClient
                .PostJsonAsync(_gatewayOptions.ApiAdviceUrl, data, DefaultSerializerSettings, cancellationToken)
                .ConfigureAwaitFalse();

            return await SepehrHelper.CreateVerifyResult(context, responseMessage, callbackResult, _options.Messages);
        }

        /// <inheritdoc />
        public override async Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var data = SepehrHelper.CreateRefundData(context, account);

            var responseMessage = await _httpClient
                .PostJsonAsync(_gatewayOptions.ApiRollbackUrl, data, DefaultSerializerSettings, cancellationToken)
                .ConfigureAwaitFalse();

            return await SepehrHelper.CreateRefundResult(context, responseMessage, _options.Messages);
        }
    }
}
