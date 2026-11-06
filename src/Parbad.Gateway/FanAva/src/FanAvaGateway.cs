// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Parbad.Abstraction;
using Parbad.Gateway.FanAva.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.FanAva;

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

        var jsonSettings = new JsonSerializerSettings
                           {
                               Converters = { new StringEnumConverter() }
                           };

        var responseMessage = await _httpClient.PostJsonAsync(_gatewayOptions.ApiTokenGenerationUrl, data, jsonSettings, cancellationToken);

        return await FanAvaHelper.CreateRequestResult(responseMessage, _httpContextAccessor.GetRequiredHttpContext(), account, _gatewayOptions);
    }

    /// <inheritdoc />
    public override async Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

        var callbackResult = await FanAvaHelper.CreateCallbackResult(
                                                                     _httpContextAccessor.GetRequiredHttpContext().Request,
                                                                     context,
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
                                                                     _httpContextAccessor.GetRequiredHttpContext().Request,
                                                                     context,
                                                                     _messageOptions,
                                                                     account,
                                                                     cancellationToken)
                                               .ConfigureAwaitFalse();

        if (!callbackResult.IsSucceed)
        {
            return PaymentVerifyResult.Failed(callbackResult.Message);
        }

        var responseMessage = await _httpClient.PostJsonAsync(
                                                              _gatewayOptions.ApiCheckPaymentUrl,
                                                              callbackResult.CallbackCheckData,
                                                              cancellationToken)
                                               .ConfigureAwaitFalse();

        var checkResult = await FanAvaHelper.CreateCheckResult(
                                                               responseMessage,
                                                               context,
                                                               callbackResult,
                                                               _messageOptions);

        if (!checkResult.IsSucceed)
        {
            return checkResult.VerifyResult;
        }

        var data = FanAvaHelper.CreateVerifyRequest(context, callbackResult);

        responseMessage = await _httpClient.PostJsonAsync(
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

        // FanAva's refund request, as Parbad sends it, has no amount: it always refunds the whole payment.
        if (amount.Value != context.Payment.Amount)
        {
            return PaymentRefundResult.Failed(_messageOptions.PartialRefundNotSupported);
        }

        var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

        var data = FanAvaHelper.CreateRefundRequest(context, account);

        if (data == null)
        {
            return PaymentRefundResult.Failed($"No successful verification with the FanAva token and reference number was found for the payment with tracking number {context.Payment.TrackingNumber}.");
        }

        var responseMessage = await _httpClient.PostJsonAsync(
                                                              _gatewayOptions.ApiRefundUrl,
                                                              data,
                                                              cancellationToken)
                                               .ConfigureAwaitFalse();

        return await FanAvaHelper.CreateRefundResult(responseMessage).ConfigureAwaitFalse();
    }
}