// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Exceptions;
using Parbad.Gateway.PayPing.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.PayPing;

/// <summary>
/// PayPing Gateway (PayPing API v3). PayPing expects the amount in Toman; Parbad sends the invoice amount as it is.
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
                       ClientRefId = invoice.TrackingNumber.ToString(CultureInfo.InvariantCulture)
                   };

        var response = await _httpClient.PostJsonAsync(_pingGatewayOptions.ApiRequestUrl, body, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var problem = await PayPingGatewayHelper.ReadProblemAsync(response, _options.Messages).ConfigureAwaitFalse();

            return PaymentRequestResult.Failed(problem.Message, account.Name, problem.Code?.ToString(CultureInfo.InvariantCulture));
        }

        var createPayResult = await response.ReadJsonAsync<CreatePayResponseModel>().ConfigureAwaitFalse();

        if (string.IsNullOrEmpty(createPayResult.PaymentCode))
        {
            return PaymentRequestResult.Failed($"{_options.Messages.InvalidDataReceivedFromGateway}PaymentCode isn't received.", account.Name);
        }

        var url = _pingGatewayOptions.PaymentPageUrl.ToggleStringAtEnd("/", shouldHave: true) + Uri.EscapeDataString(createPayResult.PaymentCode);

        var result = PaymentRequestResult.SucceedWithRedirect(account.Name, _httpContextAccessor.HttpContext, url);

        // The callback and the verification must use the payment code PayPing issued for this invoice.
        result.DatabaseAdditionalData.Add(PayPingGatewayHelper.PaymentCodeKey, createPayResult.PaymentCode);

        return result;
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

        var response = await _httpClient.PostJsonAsync(_pingGatewayOptions.ApiVerificationUrl, verificationModel, cancellationToken);

        // 202: PayPing is still verifying; 5xx: it is unknown whether it verified. Either way the payment stays
        // open, and the verification is retried with the same payment.
        if (response.StatusCode == HttpStatusCode.Accepted || (int)response.StatusCode >= 500)
        {
            throw new GatewayResponseException(response.StatusCode,
                                               response.Content?.Headers.ContentType?.MediaType,
                                               response.StatusCode == HttpStatusCode.Accepted ? "verification in progress" : "server error");
        }

        PaymentVerifyResult result;

        if (response.IsSuccessStatusCode)
        {
            var responseModel = await response.ReadJsonAsync<VerifyPayResponseModel>().ConfigureAwaitFalse();

            result = PayPingGatewayHelper.CreateVerifyResult(responseModel, callbackResult, _options.Messages);
        }
        else
        {
            var problem = await PayPingGatewayHelper.ReadProblemAsync(response, _options.Messages).ConfigureAwaitFalse();

            // 409 with code 110: verified before (e.g. the first answer was lost). The payment code comes from the
            // database, so it is this invoice's payment.
            if (response.StatusCode == HttpStatusCode.Conflict && problem.Code == PayPingGatewayHelper.AlreadyVerifiedCode)
            {
                var verifiedTransaction = PayPingGatewayHelper.TryParseVerifyResponse(problem.Details) ?? new VerifyPayResponseModel();

                result = PayPingGatewayHelper.CreateVerifyResult(verifiedTransaction, callbackResult, _options.Messages);
            }
            else
            {
                result = PaymentVerifyResult.Failed(problem.Message);
            }

            result.GatewayResponseCode = problem.Code?.ToString(CultureInfo.InvariantCulture);
        }

        result.GatewayAccountName = account.Name;

        return result;
    }

    /// <inheritdoc />
    public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
    {
        return PaymentRefundResult.Failed("The Refund operation is not supported by this gateway.").ToInterfaceAsync();
    }
}