// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Parbad.Abstraction;
using Parbad.Gateway.PayPing.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using Parbad.Storage.Abstractions;

namespace Parbad.Gateway.PayPing
{
    /// <summary>
    /// PayPing Gateway.
    /// </summary>
    [Gateway(Name)]
    public class PayPingGateway : GatewayBase<PayPingGatewayAccount>
    {
        private readonly IStorageManager _storageManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PayPingGateway> _logger;
        private readonly HttpClient _httpClient;
        private readonly PayPingGatewayOptions _pingGatewayOptions;
        private readonly ParbadOptions _options;

        public const string Name = "PayPing";

        private static JsonSerializerSettings DefaultSerializerSettings => new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        /// <summary>
        /// Initializes an instance of <see cref="PayPingGateway"/>.
        /// </summary>
        public PayPingGateway(
            IStorageManager storageManager,
            IGatewayAccountProvider<PayPingGatewayAccount> accountProvider,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            ILogger<PayPingGateway> logger,
            IOptions<PayPingGatewayOptions> gatewayOptions,
            IOptions<ParbadOptions> options) : base(accountProvider)
        {
            _storageManager = storageManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient(this);
            _pingGatewayOptions = gatewayOptions.Value;
            _options = options.Value;
        }

        /// <inheritdoc />
        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice,
            CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            _httpClient.DefaultRequestHeaders.Authorization
                = new AuthenticationHeaderValue("Bearer", account.BearerToken);

            var body = new CreatePayRequestModel()
            {
                Amount = (int) invoice.Amount.Value,
                Description = invoice.GetPayPingRequest()?.Description,
                PayerIdentity = invoice.GetPayPingRequest()?.Mobile,
                PayerName = invoice.GetPayPingRequest()?.PayerName,
                ReturnUrl = invoice.CallbackUrl.Url,
                ClientRefId = invoice.TrackingNumber.ToString()
            };


            //Send Create pay Request
            var response = await _httpClient.PostJsonAsync(_pingGatewayOptions.ApiRequestUrl, body, cancellationToken);

            //Check if we ran into an Issue
            response.EnsureSuccessStatusCode();

            //Get Response data 
            string responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(responseBody);

            //Convert Response data to Model and get PayCode
            var createPayResult = JsonConvert.DeserializeObject<CreatePayResponseModel>(responseBody);

            //Redirect User to GateWay with our PayCodeS

            var url = string.Format(_pingGatewayOptions.PaymentPageUrl, createPayResult.Code);
            return PaymentRequestResult.SucceedWithRedirect(account.Name, _httpContextAccessor.HttpContext, url);
        }

        /// <inheritdoc />
        public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context,
            CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            _httpClient.DefaultRequestHeaders.Authorization
                = new AuthenticationHeaderValue("Bearer", account.BearerToken);


            var body = await _httpContextAccessor.HttpContext.Request.ReadFormAsync(cancellationToken);
            var refId = StringValues.Empty;

            long trackingNumber;
            try
            {
                body.TryGetValue("refid", out refId);

                var clientRefId = StringValues.Empty;
                body.TryGetValue("clientrefid", out clientRefId);

                long.TryParse(clientRefId, out trackingNumber);
            }
            catch (Exception e)
            {
                return PaymentVerifyResult.Failed($"Verify Payment With RefId : {refId} Faild");
            }

            var verifyModel = new VerifyPayRequestModel()
            {
                Amount = (int) context.Payment.Amount,
                RefId = refId
            };

            //Send Verify pay Request
            var response =
                await _httpClient.PostJsonAsync(_pingGatewayOptions.ApiVerificationUrl, verifyModel, cancellationToken);

            //Check if we ran into an Issue
            if (!response.IsSuccessStatusCode)
                return PaymentVerifyResult.Failed($"Verify Payment With RefId : {refId.ToString()} Faild");


            //Get Response data 
            string responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(responseBody);

            var paymentDetails = JsonConvert.DeserializeObject<VerifyPayResponseModel>(responseBody);

            if (paymentDetails.Amount == context.Payment.Amount) // Just for Ensure For Verify Acknowledge
            {
                return PaymentVerifyResult.Succeed(context.Payment.TrackingNumber.ToString(),
                    _options.Messages.PaymentSucceed);
            }

            return PaymentVerifyResult.Failed(
                $"Check This Payment With PayPing Amount Miss Match => RefId : {refId.ToString()}");
        }

        /// <inheritdoc />
        public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount,
            CancellationToken cancellationToken = default)
        {
            return PaymentRefundResult.Failed("The Refund operation is not supported by this gateway.")
                .ToInterfaceAsync();
        }
    }
}
