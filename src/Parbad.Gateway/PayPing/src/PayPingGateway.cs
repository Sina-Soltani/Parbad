// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Gateway.PayPing.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.PayPing
{
    /// <summary>
    /// PayPing Gateway.
    /// </summary>
    [Gateway(Name)]
    public class PayPingGateway : GatewayBase<PayPingGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly PayPingGatewayOptions _pingGatewayOptions;
        private readonly ParbadOptions _options;

        public const string Name = "PayPing";

        /// <summary>
        /// Initializes an instance of <see cref="PayPingGateway"/>.
        /// </summary>
        public PayPingGateway(
            IGatewayAccountProvider<PayPingGatewayAccount> accountProvider,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<PayPingGatewayOptions> gatewayOptions,
            IOptions<ParbadOptions> options) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(this);
            _pingGatewayOptions = gatewayOptions.Value;
            _options = options.Value;
        }

        /// <inheritdoc />
        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", account.AccessToken);

            var body = new CreatePayRequestModel
            {
                Amount = invoice.Amount,
                Description = invoice.GetPayPingRequest()?.Description,
                PayerIdentity = invoice.GetPayPingRequest()?.Mobile,
                PayerName = invoice.GetPayPingRequest()?.PayerName,
                ReturnUrl = invoice.CallbackUrl,
                ClientRefId = invoice.TrackingNumber.ToString()
            };

            //Send Create pay Request
            var response = await _httpClient.PostJsonAsync(_pingGatewayOptions.ApiRequestUrl, body, cancellationToken);

            //Check if we ran into an Issue
            response.EnsureSuccessStatusCode();

            //Get Response data 
            var responseBody = await response.Content.ReadAsStringAsync();

            //Convert Response data to Model and get PayCode
            var createPayResult = JsonConvert.DeserializeObject<CreatePayResponseModel>(responseBody);

            //Redirect User to gateway with the Code

            var url = _pingGatewayOptions.PaymentPageUrl.ToggleStringAtEnd("/", shouldHave: true) + createPayResult.Code;

            return PaymentRequestResult.SucceedWithRedirect(account.Name, _httpContextAccessor.HttpContext, url);
        }

        /// <inheritdoc />
        public override async Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var callbackResult = await PayPingGatewayHelper.GetCallbackResult(
                _httpContextAccessor.HttpContext.Request,
                context,
                _options.Messages,
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

            var callbackResult = await PayPingGatewayHelper.GetCallbackResult(
                _httpContextAccessor.HttpContext.Request,
                context,
                _options.Messages,
                cancellationToken);

            if (!callbackResult.IsSucceed)
            {
                return PaymentVerifyResult.Failed(callbackResult.Message);
            }

            var verificationModel = PayPingGatewayHelper.CreateVerificationModel(context, callbackResult);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", account.AccessToken);

            //Send Verify pay Request
            var response = await _httpClient.PostJsonAsync(_pingGatewayOptions.ApiVerificationUrl, verificationModel, cancellationToken);

            //Check if we ran into an Issue
            if (!response.IsSuccessStatusCode)
            {
                return PaymentVerifyResult.Failed(_options.Messages.PaymentFailed);
            }

            //Get Response data 
            var responseBody = await response.Content.ReadAsStringAsync();

            var responseModel = JsonConvert.DeserializeObject<VerifyPayResponseModel>(responseBody);

            if (responseModel.Amount != (long)context.Payment.Amount)
            {
                var message = $"{_options.Messages.PaymentFailed} Amount is not valid.";

                return PaymentVerifyResult.Failed(message);
            }

            return PaymentVerifyResult.Succeed(callbackResult.RefId, _options.Messages.PaymentSucceed);
        }

        /// <inheritdoc />
        public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            return PaymentRefundResult.Failed("The Refund operation is not supported by this gateway.").ToInterfaceAsync();
        }
    }
}
