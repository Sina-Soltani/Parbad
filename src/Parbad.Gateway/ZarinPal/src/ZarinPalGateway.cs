// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        var result = ZarinPalHelper.CreateRequestResult(response.Data!, _httpContextAccessor.GetRequiredHttpContext(), account, _gatewayOptions, _messagesOptions);

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

        var result = ZarinPalHelper.CreateVerifyResult(response.Data!, account, _messagesOptions);

        return result;
    }

    /// <inheritdoc />
    public override async Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

        if (account.IsSandbox)
        {
            return RefundFailed(account, "ZarinPal has no sandbox for refunds.");
        }

        if (account.AuthorizationToken.IsNullOrWhiteSpace())
        {
            return RefundFailed(account, "A ZarinPal refund needs the account's AuthorizationToken: the access token of the ZarinPal panel.");
        }

        // The verify response's ref_id, which is the payment's transaction code.
        var referenceId = context.Payment.TransactionCode;

        if (referenceId.IsNullOrEmpty())
        {
            return RefundFailed(account, "The payment has no ZarinPal reference (ref_id) to refund.");
        }

        var authority = ZarinPalHelper.GetAuthorityFromAdditionalData(context);

        // 1. The terminal of the account: Terminals.key is the merchant id.
        var terminals = await SendGraphQlAsync(account, ZarinPalGraphQl.TerminalsQuery, null, cancellationToken);

        if (terminals.Error != null) return RefundFailed(account, terminals.Error);

        var terminalId = (terminals.Data!["Terminals"] as JArray)?
                         .OfType<JObject>()
                         .FirstOrDefault(terminal => string.Equals((string?)terminal["key"], account.MerchantId, StringComparison.OrdinalIgnoreCase))?["id"]?
                         .ToString();

        if (terminalId == null)
        {
            return RefundFailed(account, "The access token has no ZarinPal terminal with the account's merchant id.");
        }

        // 2. The payment ("session") with this ref_id, and this invoice's authority.
        var sessions = await SendGraphQlAsync(account,
                                              ZarinPalGraphQl.SessionsQuery,
                                              new { terminal_id = terminalId, reference_id = referenceId },
                                              cancellationToken);

        if (sessions.Error != null) return RefundFailed(account, sessions.Error);

        var sessionId = (sessions.Data!["Session"] as JArray)?
                        .OfType<JObject>()
                        .FirstOrDefault(session => session["authority"] == null || (string?)session["authority"] == authority)?["id"]?
                        .ToString();

        if (sessionId == null)
        {
            return RefundFailed(account, $"ZarinPal has no payment with the reference {referenceId} and this invoice's authority.");
        }

        // 3. The refund. Its amount may be a part of the payment.
        var refund = await SendGraphQlAsync(account,
                                            ZarinPalGraphQl.AddRefundMutation,
                                            new
                                            {
                                                session_id = sessionId,
                                                amount = (long)amount,
                                                description = $"Refund of invoice {context.Payment.TrackingNumber}",
                                                method = _gatewayOptions.RefundMethod == ZarinPalRefundMethod.Card ? "CARD" : "PAYA",
                                                reason = ToGraphQlName(_gatewayOptions.RefundReason)
                                            },
                                            cancellationToken);

        if (refund.Error != null) return RefundFailed(account, refund.Error);

        var refundStatus = (string?)refund.Data!["resource"]?["timeline"]?["refund_status"];

        if (string.Equals(refundStatus, "FAILED", StringComparison.OrdinalIgnoreCase))
        {
            return RefundFailed(account, _messagesOptions.PaymentFailed, refundStatus);
        }

        // PENDING: ZarinPal accepted the refund and pays it in the chosen method's cycle.
        return new PaymentRefundResult
               {
                   Status = PaymentRefundResultStatus.Succeed,
                   GatewayAccountName = account.Name,
                   GatewayResponseCode = refundStatus,
                   Message = string.Equals(refundStatus, "PENDING", StringComparison.OrdinalIgnoreCase)
                       ? "ZarinPal accepted the refund; it is paid to the customer in the next settlement."
                       : _messagesOptions.PaymentSucceed
               };
    }

    private Task<GraphQlResponse> SendGraphQlAsync(ZarinPalGatewayAccount account, string query, object? variables, CancellationToken cancellationToken) =>
        ZarinPalGraphQl.SendAsync(_httpClient, _gatewayOptions.ApiGraphQlUrl, account.AuthorizationToken!, query, variables, cancellationToken);

    private static PaymentRefundResult RefundFailed(ZarinPalGatewayAccount account, string message, string? gatewayResponseCode = null) =>
        PaymentRefundResult.Failed(message, gatewayResponseCode) with { GatewayAccountName = account.Name };

    private static string ToGraphQlName(ZarinPalRefundReason reason) => reason switch
    {
        ZarinPalRefundReason.DuplicateTransaction => "DUPLICATE_TRANSACTION",
        ZarinPalRefundReason.SuspiciousTransaction => "SUSPICIOUS_TRANSACTION",
        ZarinPalRefundReason.Other => "OTHER",
        _ => "CUSTOMER_REQUEST"
    };

    private static Task<HttpResponseMessage> PostAsJson(HttpClient httpClient, string url, object model)
    {
        var json = JsonConvert.SerializeObject(model,
                                               new JsonSerializerSettings
                                               {
                                                   ContractResolver = new CamelCasePropertyNamesContractResolver()
                                               });

        var request = new HttpRequestMessage(HttpMethod.Post, url)
                      {
                          Content = new StringContent(json, Encoding.UTF8, "application/json")
                      };

        // Required by ZarinPal's documentation.
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return httpClient.SendAsync(request);
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
