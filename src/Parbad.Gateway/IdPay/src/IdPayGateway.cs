// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Parbad.Abstraction;
using Parbad.Gateway.IdPay.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;

namespace Parbad.Gateway.IdPay
{
    [Gateway(Name)]
    public class IdPayGateway : GatewayBase<IdPayGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IOptions<MessagesOptions> _messagesOptions;

        public const string Name = "IdPay";

        private JsonSerializerSettings DefaultSerializerSettings => new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public IdPayGateway(
            IGatewayAccountProvider<IdPayGatewayAccount> accountProvider,
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

            IdPayHelper.AssignHeaders(_httpClient.DefaultRequestHeaders, account);

            var data = IdPayHelper.CreateRequestData(invoice);

            var responseMessage = await _httpClient
                .PostJsonAsync(IdPayHelper.RequestUrl, data, DefaultSerializerSettings, cancellationToken)
                .ConfigureAwaitFalse();

            return await IdPayHelper.CreateRequestResult(responseMessage, _httpContextAccessor, account);
        }

        /// <inheritdoc />
        public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var callbackResult = await IdPayHelper.CreateCallbackResultAsync(
                    context,
                    _httpContextAccessor.HttpContext.Request,
                    _messagesOptions.Value,
                    cancellationToken)
                .ConfigureAwaitFalse();

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            IdPayHelper.AssignHeaders(_httpClient.DefaultRequestHeaders, account);

            var data = IdPayHelper.CreateVerifyData(context, callbackResult);

            var responseMessage = await _httpClient
                .PostJsonAsync(IdPayHelper.VerifyUrl, data, DefaultSerializerSettings, cancellationToken)
                .ConfigureAwaitFalse();

            return await IdPayHelper.CreateVerifyResult(responseMessage, _messagesOptions.Value);
        }

        /// <inheritdoc />
        public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            return PaymentRefundResult.Failed("The Refund operation is not supported by this gateway.").ToInterfaceAsync();
        }
    }
}
