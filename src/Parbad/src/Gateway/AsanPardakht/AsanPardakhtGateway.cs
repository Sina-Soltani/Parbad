// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Gateway.AsanPardakht.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.AsanPardakht
{
    [Gateway(Name)]
    public class AsanPardakhtGateway : GatewayBase<AsanPardakhtGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly AsanPardakhtGatewayOptions _gatewayOptions;
        private readonly MessagesOptions _messageOptions;

        public const string Name = "AsanPardakht";

        public AsanPardakhtGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<AsanPardakhtGatewayAccount> accountProvider,
            IOptions<AsanPardakhtGatewayOptions> gatewayOptions,
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

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            return await AsanPardakhtHelper.CreateRequestResult(
                _httpClient,
                invoice,
                account,
                _httpContextAccessor.HttpContext,
                _gatewayOptions,
                _messageOptions,
                cancellationToken);
        }

        /// <inheritdoc />
        public override async Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            _httpClient.Timeout = TimeSpan.FromSeconds(20);
            
            var result = await AsanPardakhtHelper.GetTransResult(context, _httpClient, account, _gatewayOptions, _messageOptions, 
                cancellationToken);

            var fetchResult = new PaymentFetchResult
            {
                Status = result.IsSucceed ? PaymentFetchResultStatus.ReadyForVerifying : PaymentFetchResultStatus.Failed,
                Message = result.FailedMessage
            };
            fetchResult.SetAsanPardakhtOriginalPaymentResult(result.TransModel);

            return fetchResult;
        }

        /// <inheritdoc />
        public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var transResult = await AsanPardakhtHelper.GetTransResult(
                context,
                _httpClient,
                account,
                _gatewayOptions,
                _messageOptions,
                cancellationToken).ConfigureAwaitFalse();

            var paymentVerifyResult = new PaymentVerifyResult();
            paymentVerifyResult.SetAsanPardakhtOriginalPaymentResult(transResult.TransModel);

            if (!transResult.IsSucceed)
            {
                paymentVerifyResult.Status = PaymentVerifyResultStatus.Failed;
                paymentVerifyResult.Message = transResult.FailedMessage;

                return paymentVerifyResult;
            }

            var verifyResult = await AsanPardakhtHelper.CompletionMethod(
                _httpClient,
                _gatewayOptions.VerifyUrl,
                transResult.TransModel.PayGateTranID,
                account,
                _gatewayOptions,
                _messageOptions,
                cancellationToken);

            if (!verifyResult.IsSucceed)
            {
                paymentVerifyResult.Status = PaymentVerifyResultStatus.Failed;
                paymentVerifyResult.Message = verifyResult.FailedMessage;

                return paymentVerifyResult;
            }

            var settleResult = await AsanPardakhtHelper.CompletionMethod(
                _httpClient,
                _gatewayOptions.SettlementUrl,
                transResult.TransModel.PayGateTranID,
                account,
                _gatewayOptions,
                _messageOptions,
                cancellationToken);

            if (!settleResult.IsSucceed)
            {
                paymentVerifyResult.Status = PaymentVerifyResultStatus.Failed;
                paymentVerifyResult.Message = settleResult.FailedMessage;

                return paymentVerifyResult;
            }

            paymentVerifyResult.Status = PaymentVerifyResultStatus.Succeed;
            paymentVerifyResult.Message = _messageOptions.PaymentSucceed;
            paymentVerifyResult.TransactionCode = transResult.TransModel.Rrn;

            return paymentVerifyResult;
        }

        /// <inheritdoc />
        public override async Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var transResult = await AsanPardakhtHelper.GetTransResult(
                context,
                _httpClient,
                account,
                _gatewayOptions,
                _messageOptions,
                cancellationToken).ConfigureAwaitFalse();

            if (!transResult.IsSucceed)
            {
                return PaymentRefundResult.Failed(transResult.FailedMessage);
            }

            var refundResult = await AsanPardakhtHelper.CompletionMethod(
                _httpClient,
                _gatewayOptions.CancelUrl,
                transResult.TransModel.PayGateTranID,
                account,
                _gatewayOptions,
                _messageOptions,
                cancellationToken);

            if (!refundResult.IsSucceed)
            {
                return PaymentRefundResult.Failed(refundResult.FailedMessage);
            }

            return PaymentRefundResult.Succeed();
        }

        ///// <inheritdoc />
        //public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        //{
        //    if (context == null) throw new ArgumentNullException(nameof(context));

        //    var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

        //    var callbackResult = await AsanPardakhtHelper.CreateCallbackResult(
        //        context,
        //        account,
        //        _httpContextAccessor.HttpContext.Request,
        //        _crypto,
        //        _messageOptions);

        //    if (!callbackResult.IsSucceed)
        //    {
        //        return PaymentVerifyResult.Failed(callbackResult.Message);
        //    }

        //    var data = await AsanPardakhtHelper.CreateVerifyData(callbackResult, account, _crypto)
        //        .ConfigureAwaitFalse();

        //    var responseMessage = await _httpClient
        //        .PostXmlAsync(_gatewayOptions.ApiUrl, data, cancellationToken)
        //        .ConfigureAwaitFalse();

        //    var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

        //    var verifyResult = AsanPardakhtHelper.CheckVerifyResult(response, callbackResult, _messageOptions);

        //    if (!verifyResult.IsSucceed)
        //    {
        //        return verifyResult.Result;
        //    }

        //    data = await AsanPardakhtHelper.CreateSettleData(callbackResult, account, _crypto)
        //        .ConfigureAwaitFalse();

        //    responseMessage = await _httpClient
        //        .PostXmlAsync(_gatewayOptions.ApiUrl, data, cancellationToken)
        //        .ConfigureAwaitFalse();

        //    response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

        //    return AsanPardakhtHelper.CreateSettleResult(response, callbackResult, _messageOptions);
        //}

        ///// <inheritdoc />
        //public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        //{
        //    return PaymentRefundResult.Failed("RefundNotSupports").ToInterfaceAsync();
        //}
    }
}
