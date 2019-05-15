using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Data.Domain.Payments;
using Parbad.Net;
using Parbad.Options;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Internal;
using Parbad.Properties;

namespace Parbad.GatewayProviders.AsanPardakht
{
    [Gateway(Name)]
    public class AsanPardakhtGateway : IGateway
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IOptions<AsanPardakhtGatewayOptions> _options;
        private readonly IOptions<MessagesOptions> _messageOptions;

        public const string Name = "AsanPardakht";

        public AsanPardakhtGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<AsanPardakhtGatewayOptions> options,
            IOptions<MessagesOptions> messageOptions)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _options = options;
            _messageOptions = messageOptions;
        }

        /// <inheritdoc />
        public async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var data = AsanPardakhtHelper.CreateRequestData(invoice, _options.Value);

            var responseMessage = await _httpClient
                .PostXmlAsync(AsanPardakhtHelper.BaseServiceUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return AsanPardakhtHelper.CreateRequestResult(response, invoice, _options.Value, _httpContextAccessor, _messageOptions.Value);
        }

        /// <inheritdoc />
        public async Task<IPaymentVerifyResult> VerifyAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            var callbackResult = AsanPardakhtHelper.CreateCallbackResult(
                payment,
                _options.Value,
                _httpContextAccessor.HttpContext.Request,
                _messageOptions.Value);

            if (!callbackResult.IsSucceed)
            {
                return callbackResult.Result;
            }

            var data = AsanPardakhtHelper.CreateVerifyData(callbackResult, _options.Value);

            var responseMessage = await _httpClient
                .PostXmlAsync(AsanPardakhtHelper.BaseServiceUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            var verifyResult = AsanPardakhtHelper.CheckVerifyResult(response, callbackResult, _messageOptions.Value);

            if (!verifyResult.IsSucceed)
            {
                return verifyResult.Result;
            }

            data = AsanPardakhtHelper.CreateSettleData(callbackResult, _options.Value);

            responseMessage = await _httpClient
                .PostXmlAsync(AsanPardakhtHelper.BaseServiceUrl, data, cancellationToken)
                .ConfigureAwaitFalse();

            response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            return AsanPardakhtHelper.CreateSettleResult(response, callbackResult, _messageOptions.Value);
        }

        /// <inheritdoc />
        public Task<IPaymentRefundResult> RefundAsync(Payment payment, Money amount, CancellationToken cancellationToken = default)
        {
            return PaymentRefundResult.Failed(Resources.RefundNotSupports).ToInterfaceAsync();
        }
    }
}
