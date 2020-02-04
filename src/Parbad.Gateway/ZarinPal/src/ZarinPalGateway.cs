// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Gateway.ZarinPal.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;

namespace Parbad.Gateway.ZarinPal
{
    [Gateway(Name)]
    public class ZarinPalGateway : GatewayBase<ZarinPalGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IOptions<MessagesOptions> _messagesOptions;

        public const string Name = "ZarinPal";

        public ZarinPalGateway(
            IGatewayAccountProvider<ZarinPalGatewayAccount> accountProvider,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
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

            var data = ZarinPalHelper.CreateRequestData(account, invoice);

            var responseMessage = await _httpClient
                .PostXmlAsync(ZarinPalHelper.GetWebServiceUrl(account.IsSandbox), data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return ZarinPalHelper.CreateRequestResult(response, _httpContextAccessor, account, _messagesOptions.Value);
        }

        /// <inheritdoc />
        public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var callbackResult = ZarinPalHelper.CreateCallbackResult(_httpContextAccessor.HttpContext.Request);

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var data = ZarinPalHelper.CreateVerifyData(account, callbackResult, context.Payment.Amount);

            var responseMessage = await _httpClient
                .PostXmlAsync(ZarinPalHelper.GetWebServiceUrl(account.IsSandbox), data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return ZarinPalHelper.CreateVerifyResult(response, _messagesOptions.Value);
        }

        /// <inheritdoc />
        public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            return PaymentRefundResult.Failed("The Refund operation is not supported by this gateway.").ToInterfaceAsync();
        }
    }
}
