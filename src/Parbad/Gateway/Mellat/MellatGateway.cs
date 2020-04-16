// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Gateway.Mellat.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;

namespace Parbad.Gateway.Mellat
{
    [Gateway(Name)]
    public class MellatGateway : GatewayBase<MellatGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IOptions<MessagesOptions> _messagesOptions;

        public const string Name = "Mellat";

        public MellatGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<MellatGatewayAccount> accountProvider,
            IOptions<MessagesOptions> messagesOptions) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _messagesOptions = messagesOptions;
        }

        /// <inheritdoc />
        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var data = MellatHelper.CreateRequestData(invoice, account);

            var responseMessage = await _httpClient
                .PostXmlAsync(MellatHelper.GetWebServiceUrl(account.IsTestTerminal), data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return MellatHelper.CreateRequestResult(response, _httpContextAccessor.HttpContext, _messagesOptions.Value, account);
        }

        /// <inheritdoc />
        public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var callbackResult = await MellatHelper
                .CrateCallbackResultAsync(_httpContextAccessor.HttpContext.Request, _messagesOptions.Value, cancellationToken)
                .ConfigureAwaitFalse();

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var data = MellatHelper.CreateVerifyData(context, account, callbackResult);

            var responseMessage = await _httpClient
                .PostXmlAsync(MellatHelper.GetWebServiceUrl(account.IsTestTerminal), data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            var verifyResult = MellatHelper.CheckVerifyResult(response, callbackResult, _messagesOptions.Value);

            if (!verifyResult.IsSucceed)
            {
                return verifyResult.Result;
            }

            data = MellatHelper.CreateSettleData(context, callbackResult, account);

            responseMessage = await _httpClient
                .PostXmlAsync(MellatHelper.GetWebServiceUrl(account.IsTestTerminal), data, cancellationToken)
                .ConfigureAwaitFalse();

            response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return MellatHelper.CreateSettleResult(response, callbackResult, _messagesOptions.Value);
        }

        /// <inheritdoc />
        public override async Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var data = MellatHelper.CreateRefundData(context, account);

            var responseMessage = await _httpClient
                .PostXmlAsync(MellatHelper.GetWebServiceUrl(account.IsTestTerminal), data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return MellatHelper.CreateRefundResult(response, _messagesOptions.Value);
        }
    }
}
