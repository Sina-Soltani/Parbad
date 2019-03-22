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
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;

namespace Parbad.GatewayProviders.Pasargad
{
    [Gateway(Name)]
    public class PasargadGateway : IGateway
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IOptions<PasargadGatewayOptions> _options;
        private readonly IOptions<MessagesOptions> _messageOptions;

        public const string Name = "Pasargad";

        public PasargadGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<PasargadGatewayOptions> options,
            IOptions<MessagesOptions> messageOptions)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _options = options;
            _messageOptions = messageOptions;
        }

        public virtual Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            return PasargadHelper.CreateRequestResult(invoice, _httpContextAccessor, _options.Value)
                .ToInterfaceAsync();
        }

        public virtual async Task<IPaymentVerifyResult> VerifyAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            var callbackResult = PasargadHelper.CreateCallbackResult(_httpContextAccessor.HttpContext.Request, _messageOptions.Value);

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            var responseMessage = await _httpClient.PostFormAsync(
                PasargadHelper.CheckPaymentPageUrl,
                callbackResult.CallbackCheckData,
                cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            var checkCallbackResult = PasargadHelper.CreateCheckCallbackResult(
                response,
                _options.Value,
                callbackResult,
                _messageOptions.Value);

            if (!checkCallbackResult.IsSucceed)
            {
                return checkCallbackResult.Result;
            }

            var data = PasargadHelper.CreateVerifyData(payment, _options.Value, callbackResult);

            responseMessage = await _httpClient.PostFormAsync(
                PasargadHelper.VerifyPaymentPageUrl,
                data,
                cancellationToken)
                .ConfigureAwaitFalse();

            response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return PasargadHelper.CreateVerifyResult(response, callbackResult, _messageOptions.Value);
        }

        public virtual async Task<IPaymentRefundResult> RefundAsync(Payment payment, Money amount, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));
            if (amount == null) throw new ArgumentNullException(nameof(amount));

            var data = PasargadHelper.CreateRefundData(payment, amount, _options.Value);

            var responseMessage = await _httpClient.PostFormAsync(
                PasargadHelper.RefundPaymentPageUrl,
                data,
                cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return PasargadHelper.CreateRefundResult(response, _messageOptions.Value);
        }
    }
}
