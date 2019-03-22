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

namespace Parbad.GatewayProviders.Mellat
{
    [Gateway(Name)]
    public class MellatGateway : IGateway
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IOptions<MellatGatewayOptions> _options;
        private readonly IOptions<MessagesOptions> _messagesOptions;

        public const string Name = "Mellat";

        public MellatGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<MellatGatewayOptions> options,
            IOptions<MessagesOptions> messagesOptions)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _options = options;
            _messagesOptions = messagesOptions;
        }

        public virtual async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var data = MellatHelper.CreateRequestData(invoice, _options.Value);

            var responseMessage = await _httpClient
                .PostXmlAsync(GetWebServiceUrl(), data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return MellatHelper.CreateRequestResult(response, _httpContextAccessor, _messagesOptions.Value);
        }

        public virtual async Task<IPaymentVerifyResult> VerifyAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            var callbackResult = MellatHelper.CrateCallbackResult(_httpContextAccessor.HttpContext.Request, _messagesOptions.Value);

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            var data = MellatHelper.CreateVerifyData(payment, _options.Value, callbackResult);

            var responseMessage = await _httpClient
                .PostXmlAsync(GetWebServiceUrl(), data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            var verifyResult = MellatHelper.CheckVerifyResult(response, callbackResult, _messagesOptions.Value);

            if (!verifyResult.IsSucceed)
            {
                return verifyResult.Result;
            }

            data = MellatHelper.CreateSettleData(payment, callbackResult, _options.Value);

            responseMessage = await _httpClient
                .PostXmlAsync(GetWebServiceUrl(), data, cancellationToken)
                .ConfigureAwaitFalse();

            response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return MellatHelper.CreateSettleResult(response, callbackResult, _messagesOptions.Value);
        }

        public virtual async Task<IPaymentRefundResult> RefundAsync(Payment payment, Money amount, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));
            if (amount == null) throw new ArgumentNullException(nameof(amount));

            var data = MellatHelper.CreateRefundData(payment, _options.Value);

            var responseMessage = await _httpClient
                .PostXmlAsync(GetWebServiceUrl(), data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return MellatHelper.CreateRefundResult(response, _messagesOptions.Value);
        }

        private string GetWebServiceUrl()
        {
            return _options.Value.UseTestTerminal ? MellatHelper.TestWebServiceUrl : MellatHelper.WebServiceUrl;
        }
    }
}
