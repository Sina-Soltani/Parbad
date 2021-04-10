// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Gateway.AsanPardakht.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using Parbad.Properties;

namespace Parbad.Gateway.AsanPardakht
{
    [Gateway(Name)]
    public class AsanPardakhtGateway : GatewayBase<AsanPardakhtGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IAsanPardakhtCrypto _crypto;
        private readonly AsanPardakhtGatewayOptions _gatewayOptions;
        private readonly IOptions<MessagesOptions> _messageOptions;

        public const string Name = "AsanPardakht";

        public AsanPardakhtGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<AsanPardakhtGatewayAccount> accountProvider,
            IAsanPardakhtCrypto crypto,
            IOptions<AsanPardakhtGatewayOptions> gatewayOptions,
            IOptions<MessagesOptions> messageOptions) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _crypto = crypto;
            _gatewayOptions = gatewayOptions.Value;
            _messageOptions = messageOptions;
        }

        /// <inheritdoc />
        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var data = AsanPardakhtHelper.CreateRequestData(invoice, account, _crypto);

            var responseMessage = await _httpClient
                .PostXmlAsync(_gatewayOptions.ApiUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return AsanPardakhtHelper.CreateRequestResult(
                response,
                account,
                _httpContextAccessor.HttpContext,
                _gatewayOptions,
                _messageOptions.Value);
        }

        /// <inheritdoc />
        public override async Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var callbackResult = AsanPardakhtHelper.CreateCallbackResult(
                context,
                account,
                _httpContextAccessor.HttpContext.Request,
                _crypto,
                _messageOptions.Value);

            if (callbackResult.IsSucceed)
            {
                return PaymentFetchResult.ReadyForVerifying();
            }

            return PaymentFetchResult.Failed(callbackResult.Message);
        }

        /// <inheritdoc />
        public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var callbackResult = AsanPardakhtHelper.CreateCallbackResult(
                context,
                account,
                _httpContextAccessor.HttpContext.Request,
                _crypto,
                _messageOptions.Value);

            if (!callbackResult.IsSucceed)
            {
                return PaymentVerifyResult.Failed(callbackResult.Message);
            }

            var data = AsanPardakhtHelper.CreateVerifyData(callbackResult, account, _crypto);

            var responseMessage = await _httpClient
                .PostXmlAsync(_gatewayOptions.ApiUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            var verifyResult = AsanPardakhtHelper.CheckVerifyResult(response, callbackResult, _messageOptions.Value);

            if (!verifyResult.IsSucceed)
            {
                return verifyResult.Result;
            }

            data = AsanPardakhtHelper.CreateSettleData(callbackResult, account, _crypto);

            responseMessage = await _httpClient
                .PostXmlAsync(_gatewayOptions.ApiUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return AsanPardakhtHelper.CreateSettleResult(response, callbackResult, _messageOptions.Value);
        }

        /// <inheritdoc />
        public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            return PaymentRefundResult.Failed(Resources.RefundNotSupports).ToInterfaceAsync();
        }
    }
}
