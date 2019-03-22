// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Data.Domain.Payments;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using Parbad.Properties;
using Parbad.GatewayProviders.Melli.Models;

namespace Parbad.GatewayProviders.Melli
{
    [Gateway(Name)]
    public class MelliGateway : IGateway
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IOptions<MelliGatewayOptions> _options;
        private readonly IOptions<MessagesOptions> _messageOptions;

        public const string Name = "Melli";

        public MelliGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<MelliGatewayOptions> options,
            IOptions<MessagesOptions> messageOptions)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _options = options;
            _messageOptions = messageOptions;
        }

        public virtual async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var data = MelliHelper.CreateRequestData(invoice, _options.Value);

            var result = await PostJsonAsync<MelliApiRequestResult>(MelliHelper.ServiceRequestUrl, data, cancellationToken).ConfigureAwaitFalse();

            return MelliHelper.CreateRequestResult(result, _httpContextAccessor, _messageOptions.Value);
        }

        public virtual async Task<IPaymentVerifyResult> VerifyAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            var data = MelliHelper.CreateCallbackResult(
                payment,
                _httpContextAccessor.HttpContext.Request,
                _options.Value,
                _messageOptions.Value);

            if (!data.IsSucceed)
            {
                return data.Result;
            }

            var result = await PostJsonAsync<MelliApiVerifyResult>(MelliHelper.ServiceVerifyUrl, data.JsonDataToVerify, cancellationToken).ConfigureAwaitFalse();

            return MelliHelper.CreateVerifyResult(data.Token, result, _messageOptions.Value);
        }

        public virtual Task<IPaymentRefundResult> RefundAsync(Payment payment, Money amount, CancellationToken cancellationToken = default)
        {
            return PaymentRefundResult.Failed(Resources.RefundNotSupports).ToInterfaceAsync();
        }

        private async Task<T> PostJsonAsync<T>(string url, object data, CancellationToken cancellationToken = default)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

            var responseMessage = await _httpClient.PostJsonAsync(url, data, cancellationToken).ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return JsonConvert.DeserializeObject<T>(response);
        }
    }
}
