// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Gateway.Tara.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.Tara
{
    [Gateway(Name)]
    public class TaraGateway : GatewayBase<TaraGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly TaraGatewayOptions _gatewayOptions;
        private readonly MessagesOptions _messageOptions;

        public const string Name = "Tara";

        public TaraGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<TaraGatewayAccount> accountProvider,
            IOptions<TaraGatewayOptions> gatewayOptions,
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

            return await TaraHelper.CreateRequestResult(
                _httpClient,
                _httpContextAccessor.HttpContext,
                _gatewayOptions,
                account,
                invoice,
                _messageOptions,
                cancellationToken).ConfigureAwaitFalse();
        }

        /// <inheritdoc />
        public override async Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var callbackResult = await TaraHelper.CreateCallbackResultAsync(
                _httpContextAccessor.HttpContext.Request,
                context,
                _messageOptions,
                cancellationToken).ConfigureAwaitFalse();

            IPaymentFetchResult result;

            if (callbackResult.IsSucceed)
            {
                result = PaymentFetchResult.ReadyForVerifying();
            }
            else
            {
                result = PaymentFetchResult.Failed(callbackResult.Message);
            }

            return result;
        }

        /// <inheritdoc />
        public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var callbackResult = await TaraHelper.CreateCallbackResultAsync(
                _httpContextAccessor.HttpContext.Request,
                context,
                _messageOptions,
                cancellationToken).ConfigureAwaitFalse();

            if (!callbackResult.IsSucceed)
            {
                return PaymentVerifyResult.Failed(callbackResult.Message);
            }

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            return await TaraHelper.CreateVerifyResult(
                _httpClient,
                _gatewayOptions,
                account,
                callbackResult,
                _messageOptions,
                cancellationToken).ConfigureAwaitFalse();
        }

        /// <inheritdoc />
        public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            // Tara gateway doesn't support refund operation according to the documentation
            return Task.FromResult<IPaymentRefundResult>(PaymentRefundResult.Failed("Refund operation is not supported by Tara gateway."));
        }
    }
}
