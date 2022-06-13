// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Gateway.ZarinPal.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.Gateway.ZarinPal
{
    [Gateway(Name)]
    public class ZarinPalGateway : GatewayBase<ZarinPalGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly ZarinPalGatewayOptions _gatewayOptions;
        private readonly MessagesOptions _messagesOptions;
        private readonly IParbadLogger<ZarinPalGateway> _logger;

        public const string Name = "ZarinPal";

        public ZarinPalGateway(
            IGatewayAccountProvider<ZarinPalGatewayAccount> accountProvider,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<ZarinPalGatewayOptions> gatewayOptions,
            IOptions<MessagesOptions> messagesOptions,
            IParbadLogger<ZarinPalGateway> logger) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient(this);
            _gatewayOptions = gatewayOptions.Value;
            _messagesOptions = messagesOptions.Value;
        }

        /// <inheritdoc />
        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var requestModel = ZarinPalHelper.CreateRequestModel(account, invoice);

            var apiUrl = ZarinPalHelper.GetApiRequestUrl(account.IsSandbox, _gatewayOptions);

            var responseMessage = await PostAsJson(_httpClient, apiUrl, requestModel);

            if (!responseMessage.IsSuccessStatusCode)
            {
                await Log(responseMessage);

                return PaymentRequestResult.Failed($"ZarinPal HTTP request failed. Status code {responseMessage.StatusCode}", account.Name);
            }

            var resultModel = await ReadFromJsonAsync<ZarinPalRequestResultModel>(responseMessage);

            var result = ZarinPalHelper.CreateRequestResult(resultModel.Data, _httpContextAccessor.HttpContext, account, _gatewayOptions, _messagesOptions);

            return result;
        }

        public override async Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var callbackResult = await ZarinPalHelper.CreateCallbackResultAsync(_httpContextAccessor.HttpContext.Request,
                                                                                _messagesOptions,
                                                                                cancellationToken);

            PaymentFetchResult result;

            if (callbackResult.IsSucceed)
            {
                result = PaymentFetchResult.ReadyForVerifying();
            }
            else
            {
                result = PaymentFetchResult.Failed(callbackResult.Message);
            }

            result.GatewayResponseCode = callbackResult.Status.ToString();

            return result;
        }

        /// <inheritdoc />
        public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var callbackResult = await ZarinPalHelper.CreateCallbackResultAsync(_httpContextAccessor.HttpContext.Request,
                                                                                _messagesOptions,
                                                                                cancellationToken);

            if (!callbackResult.IsSucceed)
            {
                return PaymentVerifyResult.Failed(callbackResult.Message);
            }

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var verificationModel = ZarinPalHelper.CreateVerificationModel(account, callbackResult, context.Payment.Amount);

            var apiUrl = ZarinPalHelper.GetApiVerificationUrl(account.IsSandbox, _gatewayOptions);

            var responseMessage = await PostAsJson(_httpClient, apiUrl, verificationModel);

            if (!responseMessage.IsSuccessStatusCode)
            {
                await Log(responseMessage);

                return new PaymentVerifyResult
                       {
                           Status = PaymentVerifyResultStatus.Failed,
                           GatewayAccountName = account.Name,
                           Message = $"ZarinPal HTTP request failed. Status code {responseMessage.StatusCode}"
                       };
            }

            var resultModel = await ReadFromJsonAsync<ZarinPalVerificationResultModel>(responseMessage);

            var result = ZarinPalHelper.CreateVerifyResult(resultModel.Data, account, _messagesOptions);

            return result;
        }

        /// <inheritdoc />
        public override async Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var transaction = context.Transactions.SingleOrDefault(_ => _.Type == TransactionType.Request);

            if (transaction == null)
            {
                return CreateRefundFailedResult(amount, account, $"No transaction of type '{TransactionType.Request}' found for the given invoice.");
            }

            var authority = ZarinPalHelper.GetAuthorityFromAdditionalData(transaction);

            if (string.IsNullOrEmpty(authority))
            {
                return CreateRefundFailedResult(amount, account, $"Authority not found in the Transaction record.");
            }

            var refundModel = ZarinPalHelper.CreateRefundModel(authority, account);

            var apiUrl = ZarinPalHelper.GetApiRefundUrl(account.IsSandbox, _gatewayOptions);

            var responseMessage = await PostAsJson(_httpClient, apiUrl, refundModel);

            if (!responseMessage.IsSuccessStatusCode)
            {
                await Log(responseMessage);

                return new PaymentRefundResult
                       {
                           Status = PaymentRefundResultStatus.Failed,
                           GatewayAccountName = account.Name,
                           Message = $"ZarinPal HTTP request failed. Status code {responseMessage.StatusCode}"
                       };
            }

            var resultModel = await ReadFromJsonAsync<ZarinPalRefundResultModel>(responseMessage);

            var result = ZarinPalHelper.CreateRefundResult(resultModel.Data, account, _messagesOptions);

            return result;
        }

        private static PaymentRefundResult CreateRefundFailedResult(Money amount, GatewayAccount account, string message)
        {
            return new PaymentRefundResult
                   {
                       Status = PaymentRefundResultStatus.Failed,
                       Message = message,
                       Amount = amount,
                       GatewayAccountName = account.Name
                   };
        }

        private static Task<HttpResponseMessage> PostAsJson(HttpClient httpClient, string url, object model)
        {
            var json = JsonConvert.SerializeObject(model,
                                                   new JsonSerializerSettings
                                                   {
                                                       ContractResolver = new CamelCasePropertyNamesContractResolver()
                                                   });

            return httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        }

        private static async Task<ZarinPalResultModel<T>> ReadFromJsonAsync<T>(HttpResponseMessage httpResponseMessage) where T : class
        {
            var json = await httpResponseMessage.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ZarinPalResultModel<T>>(json);
        }
        
        private async Task Log(HttpResponseMessage responseMessage)
        {
            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            _logger.LogError("ZarinPal HTTP request failed. Check the status code {StatusCode} and the HTTP response content {ResponseContent}", responseMessage.StatusCode, responseContent);
        }
    }
}
