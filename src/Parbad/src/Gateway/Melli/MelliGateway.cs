// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Gateway.Melli.Internal;
using Parbad.Gateway.Melli.Internal.Models;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using Parbad.Properties;

namespace Parbad.Gateway.Melli
{
    /// <summary>
    /// Melli Gateway.
    /// </summary>
    [Gateway(Name)]
    public class MelliGateway : GatewayBase<MelliGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IMelliGatewayCrypto _crypto;
        private readonly MelliGatewayOptions _gatewayOptions;
        private readonly IOptions<MessagesOptions> _messageOptions;

        public const string Name = "Melli";

        /// <summary>
        /// Initializes an instance of <see cref="MelliGateway"/>.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="accountProvider"></param>
        /// <param name="crypto"></param>
        /// <param name="gatewayOptions"></param>
        /// <param name="messageOptions"></param>
        public MelliGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<MelliGatewayAccount> accountProvider,
            IMelliGatewayCrypto crypto,
            IOptions<MelliGatewayOptions> gatewayOptions,
            IOptions<MessagesOptions> messageOptions) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _crypto = crypto;
            _messageOptions = messageOptions;
            _gatewayOptions = gatewayOptions.Value;
        }

        /// <inheritdoc />
        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var data = MelliHelper.CreateRequestData(invoice, account, _crypto);

            var result = await PostJsonAsync<MelliApiRequestResult>(_gatewayOptions.ApiRequestUrl, data, cancellationToken).ConfigureAwaitFalse();

            return MelliHelper.CreateRequestResult(result, _httpContextAccessor.HttpContext, account, _gatewayOptions, _messageOptions.Value);
        }

        /// <inheritdoc />
        public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var data = await MelliHelper.CreateCallbackResultAsync(
                context,
                _httpContextAccessor.HttpContext.Request,
                account,
                _crypto,
                _messageOptions.Value,
                cancellationToken);

            if (!data.IsSucceed)
            {
                return data.Result;
            }

            var result = await PostJsonAsync<MelliApiVerifyResult>(_gatewayOptions.ApiVerificationUrl, data.JsonDataToVerify, cancellationToken).ConfigureAwaitFalse();

            return MelliHelper.CreateVerifyResult(result, _messageOptions.Value);
        }

        /// <inheritdoc />
        public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            return PaymentRefundResult.Failed(Resources.RefundNotSupports).ToInterfaceAsync();
        }

        private async Task<T> PostJsonAsync<T>(string url, object data, CancellationToken cancellationToken = default)
        {
            var responseMessage = await _httpClient.PostJsonAsync(url, data, cancellationToken).ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return JsonConvert.DeserializeObject<T>(response);
        }
    }
}
