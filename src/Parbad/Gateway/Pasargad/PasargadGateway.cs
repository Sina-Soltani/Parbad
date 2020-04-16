// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Gateway.Pasargad.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;

namespace Parbad.Gateway.Pasargad
{
    [Gateway(Name)]
    public class PasargadGateway : GatewayBase<PasargadGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IOptions<MessagesOptions> _messageOptions;

        public const string Name = "Pasargad";

        public PasargadGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<PasargadGatewayAccount> accountProvider,
            IOptions<MessagesOptions> messageOptions) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _messageOptions = messageOptions;
        }

        /// <inheritdoc />
        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            return PasargadHelper.CreateRequestResult(invoice, _httpContextAccessor, account);
        }

        /// <inheritdoc />
        public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var callbackResult = await PasargadHelper.CreateCallbackResult(
                _httpContextAccessor.HttpContext.Request,
                _messageOptions.Value,
                cancellationToken)
                .ConfigureAwaitFalse();

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            var responseMessage = await _httpClient.PostFormAsync(
                PasargadHelper.CheckPaymentPageUrl,
                callbackResult.CallbackCheckData,
                cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var checkCallbackResult = PasargadHelper.CreateCheckCallbackResult(
                response,
                account,
                callbackResult,
                _messageOptions.Value);

            if (!checkCallbackResult.IsSucceed)
            {
                return checkCallbackResult.Result;
            }

            var data = PasargadHelper.CreateVerifyData(context, account, callbackResult);

            responseMessage = await _httpClient.PostFormAsync(
                PasargadHelper.VerifyPaymentPageUrl,
                data,
                cancellationToken)
                .ConfigureAwaitFalse();

            response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return PasargadHelper.CreateVerifyResult(response, callbackResult, _messageOptions.Value);
        }

        /// <inheritdoc />
        public override async Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var data = PasargadHelper.CreateRefundData(context, amount, account);

            var responseMessage = await _httpClient.PostFormAsync(
                PasargadHelper.RefundPaymentPageUrl,
                data,
                cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return PasargadHelper.CreateRefundResult(response, _messageOptions.Value);
        }
    }
}
