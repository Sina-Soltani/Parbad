// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Http;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Parbad.Abstraction;
using Parbad.Exceptions;
using Parbad.Gateway.ZarinPal.Internal;
using Parbad.Gateway.ZarinPal.Models;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;

namespace Parbad.Gateway.ZarinPal;

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

        var response = await ReadAsync<ZarinPalRequestResultModelData>(responseMessage);

        if (response.IsError)
        {
            return PaymentRequestResult.Failed(response.ErrorMessage, account.Name, response.ErrorCode?.ToString());
        }

        var result = ZarinPalHelper.CreateRequestResult(response.Data, _httpContextAccessor.GetRequiredHttpContext(), account, _gatewayOptions, _messagesOptions);

        return result;
    }

    public override async Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var callbackResult = await ZarinPalHelper.CreateCallbackResultAsync(_httpContextAccessor.GetRequiredHttpContext().Request,
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

        return result with { GatewayResponseCode = callbackResult.Status };
    }

    /// <inheritdoc />
    public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var callbackResult = await ZarinPalHelper.CreateCallbackResultAsync(_httpContextAccessor.GetRequiredHttpContext().Request,
                                                                            _messagesOptions,
                                                                            cancellationToken);

        if (!callbackResult.IsSucceed)
        {
            return PaymentVerifyResult.Failed(callbackResult.Message);
        }

        var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

        var verificationModel = ZarinPalHelper.CreateVerificationModel(account, context, context.Payment.Amount);

        var apiUrl = ZarinPalHelper.GetApiVerificationUrl(account.IsSandbox, _gatewayOptions);

        var responseMessage = await PostAsJson(_httpClient, apiUrl, verificationModel);

        var response = await ReadAsync<ZarinPalOriginalVerificationResult>(responseMessage);

        if (response.IsError)
        {
            return new PaymentVerifyResult
                   {
                       Status = PaymentVerifyResultStatus.Failed,
                       GatewayAccountName = account.Name,
                       GatewayResponseCode = response.ErrorCode?.ToString(),
                       Message = response.ErrorMessage
                   };
        }

        var result = ZarinPalHelper.CreateVerifyResult(response.Data, account, _messagesOptions);

        return result;
    }

    /// <inheritdoc />
    public override async Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        // The refund request Parbad sends to ZarinPal has no amount: it always refunds the whole payment.
        if (amount.Value != context.Payment.Amount)
        {
            return PaymentRefundResult.Failed(_messagesOptions.PartialRefundNotSupported);
        }

        var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

        var authority = ZarinPalHelper.GetAuthorityFromAdditionalData(context);

        var refundModel = ZarinPalHelper.CreateRefundModel(authority, account);

        var apiUrl = ZarinPalHelper.GetApiRefundUrl(account.IsSandbox, _gatewayOptions);

        var responseMessage = await PostAsJson(_httpClient, apiUrl, refundModel);

        var response = await ReadAsync<ZarinPalRefundResultModelData>(responseMessage);

        if (response.IsError)
        {
            return new PaymentRefundResult
                   {
                       Status = PaymentRefundResultStatus.Failed,
                       GatewayAccountName = account.Name,
                       GatewayResponseCode = response.ErrorCode?.ToString(),
                       Message = response.ErrorMessage
                   };
        }

        var result = ZarinPalHelper.CreateRefundResult(response.Data, account, _messagesOptions);

        return result;
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

    private async Task<ZarinPalResponse<TData>> ReadAsync<TData>(HttpResponseMessage responseMessage) where TData : class
    {
        try
        {
            var response = await ZarinPalResponse<TData>.ReadAsync(responseMessage, _messagesOptions).ConfigureAwaitFalse();

            if (response.IsError) await Log(responseMessage);

            return response;
        }
        catch (GatewayResponseException)
        {
            await Log(responseMessage);

            throw;
        }
    }

    private async Task Log(HttpResponseMessage responseMessage)
    {
        var responseContent = await responseMessage.Content.ReadAsStringAsync();

        _logger.LogError("ZarinPal HTTP request failed. Check the status code {StatusCode} and the HTTP response content {ResponseContent}",
                         responseMessage.StatusCode,
                         responseContent);
    }
}
