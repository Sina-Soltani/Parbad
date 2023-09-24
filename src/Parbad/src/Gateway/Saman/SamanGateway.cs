// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Gateway.Saman.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Gateway.Saman.Internal.Models;

namespace Parbad.Gateway.Saman;

[Gateway(Name)]
public class SamanGateway : GatewayBase<SamanGatewayAccount>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _httpClient;
    private readonly SamanGatewayOptions _gatewayOptions;
    private readonly MessagesOptions _messageOptions;

    public const string Name = "Saman";

    public SamanGateway(IHttpContextAccessor httpContextAccessor,
                        IHttpClientFactory httpClientFactory,
                        IGatewayAccountProvider<SamanGatewayAccount> accountProvider,
                        IOptions<SamanGatewayOptions> gatewayOptions,
                        IOptions<MessagesOptions> messageOptions) : base(accountProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _httpClient = httpClientFactory.CreateClient(this);
        _gatewayOptions = gatewayOptions.Value;
        _messageOptions = messageOptions.Value;
    }

    /// <inheritdoc />
    public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        if (invoice == null) throw new ArgumentNullException(nameof(invoice));

        var account = await GetAccountAsync(invoice);

        var tokenRequestModel = SamanHelper.CreateTokenRequestModel(invoice, account);

        var responseModel = await _httpClient.PostJsonAsync<SamanTokenResponse>(_gatewayOptions.ApiTokenUrl,
                                                                                tokenRequestModel,
                                                                                cancellationToken: cancellationToken);

        return SamanHelper.CreatePaymentRequestResult(responseModel,
                                                      account,
                                                      _httpContextAccessor.HttpContext,
                                                      _gatewayOptions,
                                                      _messageOptions);
    }

    /// <inheritdoc />
    public override async Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var callbackResultResponse = await SamanHelper.BindCallbackResponse(_httpContextAccessor.HttpContext.Request, cancellationToken);

        var account = await GetAccountAsync(context.Payment);

        return SamanHelper.CreateFetchResult(callbackResultResponse, context, account, _messageOptions);
    }

    /// <inheritdoc />
    public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var callbackResponse = await SamanHelper.BindCallbackResponse(_httpContextAccessor.HttpContext.Request, cancellationToken);

        var account = await GetAccountAsync(context.Payment);

        var fetchResult = SamanHelper.CreateFetchResult(callbackResponse, context, account, _messageOptions);

        if (!fetchResult.IsSucceed)
        {
            return PaymentVerifyResult.Failed(fetchResult.Message);
        }

        var verificationRequest = SamanHelper.CreateVerificationRequest(callbackResponse, account);

        var verificationResponse = await _httpClient.PostJsonAsync<SamanVerificationAndRefundResponse>(_gatewayOptions.ApiVerificationUrl,
                                                                                                       verificationRequest,
                                                                                                       cancellationToken: cancellationToken);

        return SamanHelper.CreateVerifyResult(account,
                                              callbackResponse,
                                              verificationResponse,
                                              context,
                                              _messageOptions);
    }

    /// <inheritdoc />
    public override async Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

        var request = SamanHelper.CreateReverseRequest(context, account);

        var response = await _httpClient.PostJsonAsync<SamanVerificationAndRefundResponse>(_gatewayOptions.ApiReverseUrl,
                                                                                           request,
                                                                                           cancellationToken: cancellationToken);

        return SamanHelper.CreateRefundResult(response);
    }
}
