// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Gateway.Saman.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;

namespace Parbad.Gateway.Saman
{
    [Gateway(Name)]
    public class SamanGateway : GatewayBase<SamanGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly SamanGatewayOptions _gatewayOptions;
        private readonly MessagesOptions _messageOptions;

        public const string Name = "Saman";

        public SamanGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<SamanGatewayAccount> accountProvider,
            IOptions<SamanGatewayOptions> gatewayOptions,
            IOptions<MessagesOptions> messageOptions) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _gatewayOptions = gatewayOptions.Value;
            _messageOptions = messageOptions.Value;
        }

        /// <inheritdoc />
        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var httpContext = _httpContextAccessor.HttpContext;

            var result = await SamanHelper.CreateRequest(
                invoice,
                httpContext,
                account,
                _httpClient,
                _gatewayOptions,
                _messageOptions,
                cancellationToken);

            return result;
        }

        /// <inheritdoc />
        public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var callbackResult = await SamanHelper.CreateCallbackResultAsync(
                    _httpContextAccessor.HttpContext.Request,
                    _messageOptions,
                    cancellationToken)
                .ConfigureAwaitFalse();

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var data = SamanHelper.CreateVerifyData(callbackResult, account);

            var responseMessage = await _httpClient
                .PostXmlAsync(SamanHelper.GetVerificationUrl(context, _gatewayOptions), data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return SamanHelper.CreateVerifyResult(response, context, callbackResult, _messageOptions);
        }

        /// <inheritdoc />
        public override async Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var data = SamanHelper.CreateRefundData(context, amount, account);

            var responseMessage = await _httpClient
                .PostXmlAsync(SamanHelper.GetVerificationUrl(context, _gatewayOptions), data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return SamanHelper.CreateRefundResult(response, _messageOptions);
        }
    }
}
