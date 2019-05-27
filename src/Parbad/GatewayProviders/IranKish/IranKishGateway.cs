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
using Parbad.Properties;

namespace Parbad.GatewayProviders.IranKish
{
    [Gateway(Name)]
    public class IranKishGateway : IGateway
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IGatewayAccountProvider<IranKishGatewayAccount> _accountProvider;
        private readonly IOptions<MessagesOptions> _messageOptions;

        public const string Name = "IranKish";

        public IranKishGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<IranKishGatewayAccount> accountProvider,
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

            var data = IranKishHelper.CreateRequestData(invoice, account);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(IranKishHelper.HttpRequestHeader.Key, IranKishHelper.HttpRequestHeader.Value);

            var responseMessage = await _httpClient
                .PostXmlAsync(IranKishHelper.TokenWebServiceUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return IranKishHelper.CreateRequestResult(response, invoice, account, _httpContextAccessor, _messageOptions.Value);
        }

        /// <inheritdoc />
        public virtual async Task<IPaymentVerifyResult> VerifyAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            var account = await GetAccountAsync(payment.GatewayAccountName).ConfigureAwaitFalse();

            var callbackResult = IranKishHelper.CreateCallbackResult(
                payment,
                account,
                _httpContextAccessor.HttpContext.Request,
                _messageOptions.Value);

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            var data = IranKishHelper.CreateVerifyData(callbackResult, account);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(IranKishHelper.HttpVerifyHeader.Key, IranKishHelper.HttpVerifyHeader.Value);

            var responseMessage = await _httpClient
                .PostXmlAsync(IranKishHelper.VerifyWebServiceUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return IranKishHelper.CreateVerifyResult(response, payment, callbackResult, _messageOptions.Value);
        }

        /// <inheritdoc />
        public virtual Task<IPaymentRefundResult> RefundAsync(Payment payment, Money amount, CancellationToken cancellationToken = default)
        {
            return PaymentRefundResult.Failed(Resources.RefundNotSupports).ToInterfaceAsync();
        }

        private async Task<IranKishGatewayAccount> GetAccountAsync(string accountName)
        {
            var accounts = await _accountProvider.LoadAccountsAsync();

            return accounts.GetOrDefault(accountName);
        }
    }
}
