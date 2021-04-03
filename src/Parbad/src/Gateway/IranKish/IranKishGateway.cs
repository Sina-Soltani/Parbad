// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Gateway.IranKish.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using Parbad.Properties;

namespace Parbad.Gateway.IranKish
{
    [Gateway(Name)]
    public class IranKishGateway : GatewayBase<IranKishGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IranKishGatewayOptions _gatewayOptions;
        private readonly IOptions<MessagesOptions> _messageOptions;

        public const string Name = "IranKish";

        public IranKishGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<IranKishGatewayAccount> accountProvider,
            IOptions<IranKishGatewayOptions> gatewayOptions,
            IOptions<MessagesOptions> messageOptions) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _gatewayOptions = gatewayOptions.Value;
            _messageOptions = messageOptions;
        }

        /// <inheritdoc />
        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var data = IranKishHelper.CreateRequestData(invoice, account);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(IranKishHelper.HttpRequestHeader.Key, IranKishHelper.HttpRequestHeader.Value);

            var responseMessage = await _httpClient
                .PostXmlAsync(_gatewayOptions.ApiTokenUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return IranKishHelper.CreateRequestResult(response, account, _gatewayOptions, _httpContextAccessor.HttpContext, _messageOptions.Value);
        }

        /// <inheritdoc />
        public override async Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var callbackResult = await IranKishHelper.CreateCallbackResultAsync(
                context,
                account,
                _httpContextAccessor.HttpContext.Request,
                _messageOptions.Value,
                cancellationToken);

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

            var callbackResult = await IranKishHelper.CreateCallbackResultAsync(
                context,
                account,
                _httpContextAccessor.HttpContext.Request,
                _messageOptions.Value,
                cancellationToken);

            if (!callbackResult.IsSucceed)
            {
                return PaymentVerifyResult.Failed(callbackResult.Message);
            }

            var data = IranKishHelper.CreateVerifyData(callbackResult, account);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(IranKishHelper.HttpVerifyHeader.Key, IranKishHelper.HttpVerifyHeader.Value);

            var responseMessage = await _httpClient
                .PostXmlAsync(_gatewayOptions.ApiVerificationUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return IranKishHelper.CreateVerifyResult(response, context, callbackResult, _messageOptions.Value);
        }

        /// <inheritdoc />
        public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            return PaymentRefundResult.Failed(Resources.RefundNotSupports).ToInterfaceAsync();
        }
    }
}
