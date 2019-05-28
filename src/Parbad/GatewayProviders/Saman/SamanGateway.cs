// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Data.Domain.Payments;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;

namespace Parbad.GatewayProviders.Saman
{
    [Gateway(Name)]
    public class SamanGateway : Gateway<SamanGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IOptions<MessagesOptions> _messageOptions;

        public const string Name = "Saman";

        public SamanGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<SamanGatewayAccount> accountProvider,
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

            return SamanHelper.CreateRequestResult(invoice, _httpContextAccessor, account);
        }

        /// <inheritdoc />
        public override async Task<IPaymentVerifyResult> VerifyAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            var callbackResult = SamanHelper.CreateCallbackResult(_httpContextAccessor.HttpContext.Request, _messageOptions.Value);

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            var account = await GetAccountAsync(payment).ConfigureAwaitFalse();

            var data = SamanHelper.CreateVerifyData(callbackResult, account);

            var responseMessage = await _httpClient
                .PostXmlAsync(SamanHelper.WebServiceUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return SamanHelper.CreateVerifyResult(response, payment, callbackResult, _messageOptions.Value);
        }

        /// <inheritdoc />
        public override async Task<IPaymentRefundResult> RefundAsync(Payment payment, Money amount, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));
            if (amount == null) throw new ArgumentNullException(nameof(amount));

            var account = await GetAccountAsync(payment).ConfigureAwaitFalse();

            var data = SamanHelper.CreateRefundData(payment, amount, account);

            var responseMessage = await _httpClient
                .PostXmlAsync(SamanHelper.WebServiceUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return SamanHelper.CreateRefundResult(response, _messageOptions.Value);
        }
    }
}
