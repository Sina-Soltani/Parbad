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

namespace Parbad.GatewayProviders.Parsian
{
    [Gateway(Name)]
    public class ParsianGateway : IGateway
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IGatewayAccountProvider<ParsianGatewayAccount> _accountProvider;
        private readonly IOptions<MessagesOptions> _messageOptions;

        public const string Name = "Parsian";

        public ParsianGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<ParsianGatewayAccount> accountProvider,
            IOptions<MessagesOptions> messageOptions)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _accountProvider = accountProvider;
            _messageOptions = messageOptions;
        }

        /// <inheritdoc />
        public virtual async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice.GetAccountName()).ConfigureAwaitFalse();

            var data = ParsianHelper.CreateRequestData(account, invoice);

            var responseMessage = await _httpClient
                .PostXmlAsync(ParsianHelper.RequestServiceUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return ParsianHelper.CreateRequestResult(response, _httpContextAccessor, account, _messageOptions.Value);
        }

        /// <inheritdoc />
        public virtual async Task<IPaymentVerifyResult> VerifyAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            var callbackResult = ParsianHelper.CreateCallbackResult(payment, _httpContextAccessor.HttpContext.Request, _messageOptions.Value);

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            var account = await GetAccountAsync(payment.GatewayAccountName).ConfigureAwaitFalse();

            var data = ParsianHelper.CreateVerifyData(account, callbackResult);

            var responseMessage = await _httpClient
                .PostXmlAsync(ParsianHelper.VerifyServiceUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return ParsianHelper.CreateVerifyResult(response, callbackResult, _messageOptions.Value);
        }

        /// <inheritdoc />
        public virtual async Task<IPaymentRefundResult> RefundAsync(Payment payment, Money amount, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            var account = await GetAccountAsync(payment.GatewayAccountName).ConfigureAwaitFalse();

            var data = ParsianHelper.CreateRefundData(account, payment, amount);

            var responseMessage = await _httpClient
                .PostXmlAsync(ParsianHelper.RefundServiceUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return ParsianHelper.CreateRefundResult(response, _messageOptions.Value);
        }

        private async Task<ParsianGatewayAccount> GetAccountAsync(string accountName)
        {
            var accounts = await _accountProvider.LoadAccountsAsync();

            return accounts.GetOrDefault(accountName);
        }
    }
}
