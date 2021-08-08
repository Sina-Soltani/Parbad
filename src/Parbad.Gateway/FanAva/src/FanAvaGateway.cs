using System;
using System.Net.Http;
using System.Net.Http.Json;
using Parbad.Abstraction;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Gateway.FanAva.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Options;

namespace Parbad.Gateway.FanAva
{
    [Gateway(Name)]
    public class FanAvaGateway : GatewayBase<FanAvaGatewayAccount>
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly FanAvaGatewayOptions _gatewayOptions;
        private readonly MessagesOptions _messageOptions;

        public const string Name = "FanAva";

        public FanAvaGateway(
            IGatewayAccountProvider<FanAvaGatewayAccount> accountProvider,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<FanAvaGatewayOptions> gatewayOptions,
            IOptions<MessagesOptions> messageOptions) : base(accountProvider)
        {

            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(nameof(FanAvaGateway));
            _gatewayOptions = gatewayOptions.Value;
            _messageOptions = messageOptions.Value;
        }

        /// <inheritdoc />
        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();
            var data = FanAvaHelper.CreateRequestModel(invoice, account);

            var responseMessage = await _httpClient.PostAsJsonAsync(_gatewayOptions.ApiGenerateToken, data, cancellationToken);

            return await FanAvaHelper.CreateRequestResult(responseMessage, _httpContextAccessor.HttpContext, account, _gatewayOptions);

        }
        /// <inheritdoc />
        public override async Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var callbackResult = await FanAvaHelper.CreateCallbackResult(
                    _httpContextAccessor.HttpContext.Request,
                    _messageOptions,
                    account,
                    cancellationToken)
                .ConfigureAwaitFalse();

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

            var callbackResult = await FanAvaHelper.CreateCallbackResult(
                    _httpContextAccessor.HttpContext.Request,
                    _messageOptions,
                    account,
                    cancellationToken)
                .ConfigureAwaitFalse();

            if (!callbackResult.IsSucceed)
            {
                return PaymentVerifyResult.Failed(callbackResult.Message);
            }

            var responseMessage = await _httpClient.PostAsJsonAsync(
                    _gatewayOptions.ApiCheckPaymentUrl,
                    callbackResult.CallbackCheckData,
                    cancellationToken)
                .ConfigureAwaitFalse();

            var checkResult = await FanAvaHelper.CreateCheckResult(
                responseMessage,
                account,
                callbackResult,
                _messageOptions);

            if (!checkResult.IsSucceed)
            {
                return checkResult.VerifyResult;
            }

            var data = FanAvaHelper.CreateVerifyRequest(context, callbackResult, checkResult);

            responseMessage = await _httpClient.PostAsJsonAsync(
                    _gatewayOptions.ApiVerificationUrl,
                    data,
                    cancellationToken)
                .ConfigureAwaitFalse();

            return await FanAvaHelper.CreateVerifyResult(responseMessage, callbackResult, _messageOptions);

        }

        /// <inheritdoc />
        public override async Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var callbackResult = await FanAvaHelper.CreateCallbackResult(
                    _httpContextAccessor.HttpContext.Request,
                    _messageOptions,
                    account,
                    cancellationToken)
                .ConfigureAwaitFalse();

            if (!callbackResult.IsSucceed)
            {
                return PaymentRefundResult.Failed(callbackResult.Message);
            }

            var responseMessage = await _httpClient.PostAsJsonAsync(
                    _gatewayOptions.ApiCheckPaymentUrl,
                    callbackResult.CallbackCheckData,
                    cancellationToken)
                .ConfigureAwaitFalse();

            var checkResult = await FanAvaHelper.CreateCheckResult(
                responseMessage,
                account,
                callbackResult,
                _messageOptions);

            if (!checkResult.IsSucceed)
            {
                return PaymentRefundResult.Failed(checkResult.VerifyResult.Message);
            }

            var data = FanAvaHelper.CreateVerifyRequest(context, callbackResult, checkResult);

            responseMessage = await _httpClient.PostAsJsonAsync(
                    _gatewayOptions.ApiRefundUrl,
                    data,
                    cancellationToken)
                .ConfigureAwaitFalse();

            return await FanAvaHelper.CreateRefundResult(responseMessage, callbackResult, _messageOptions);
        }
    }
}