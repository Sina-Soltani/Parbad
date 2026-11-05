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
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.AsanPardakht
{
    /// <summary>
    /// AsanPardakht, through its REST API: Token → payment page → callback → TranResult → Verify → Settlement.
    /// </summary>
    [Gateway(Name)]
    public class AsanPardakhtGateway : GatewayBase<AsanPardakhtGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AsanPardakhtRestApi _api;
        private readonly AsanPardakhtGatewayOptions _gatewayOptions;
        private readonly MessagesOptions _messageOptions;

        public const string Name = "AsanPardakht";

        /// <summary>
        /// Initializes an instance of <see cref="AsanPardakhtGateway"/>.
        /// </summary>
        public AsanPardakhtGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<AsanPardakhtGatewayAccount> accountProvider,
            IOptions<AsanPardakhtGatewayOptions> gatewayOptions,
            IOptions<MessagesOptions> messageOptions) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _gatewayOptions = gatewayOptions.Value;
            _messageOptions = messageOptions.Value;
            _api = new AsanPardakhtRestApi(httpClientFactory.CreateClient(this), _gatewayOptions);
        }

        /// <summary>
        /// Initializes an instance of <see cref="AsanPardakhtGateway"/>. The crypto service is not used any more.
        /// </summary>
        [Obsolete("The crypto service is not used: since Parbad 3.12 the gateway uses AsanPardakht's REST API. This constructor will be removed in Parbad 4.0.")]
#pragma warning disable CS0618
        public AsanPardakhtGateway(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IGatewayAccountProvider<AsanPardakhtGatewayAccount> accountProvider,
            IAsanPardakhtCrypto crypto,
            IOptions<AsanPardakhtGatewayOptions> gatewayOptions,
            IOptions<MessagesOptions> messageOptions)
#pragma warning restore CS0618
            : this(httpContextAccessor, httpClientFactory, accountProvider, gatewayOptions, messageOptions)
        {
        }

        /// <inheritdoc />
        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            if (!AsanPardakhtHelper.TryGetMerchantConfigurationId(account, out var merchantConfigurationId))
            {
                return PaymentRequestResult.Failed(AsanPardakhtHelper.InvalidMerchantConfigurationIdMessage(account), account.Name);
            }

            // The guide allows a difference of up to one day from the bank's clock, so the server's clock is enough;
            // the bank's Time endpoint is optional.
            var localDate = DateTime.Now.ToString("yyyyMMdd HHmmss", CultureInfo.InvariantCulture);

            var request = AsanPardakhtHelper.CreateTokenRequest(invoice, merchantConfigurationId, localDate);

            var response = await _api.GetTokenAsync(account, request, cancellationToken).ConfigureAwaitFalse();

            return AsanPardakhtHelper.CreateRequestResult(response, account, _httpContextAccessor.HttpContext, _gatewayOptions, _messageOptions);
        }

        /// <inheritdoc />
        public override async Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            if (!AsanPardakhtHelper.TryGetMerchantConfigurationId(account, out var merchantConfigurationId))
            {
                return PaymentFetchResult.Failed(AsanPardakhtHelper.InvalidMerchantConfigurationIdMessage(account));
            }

            // The callback itself is not read: the bank is asked about this invoice directly.
            var response = await _api.GetTransactionResultAsync(account, merchantConfigurationId, context.Payment.TrackingNumber, cancellationToken)
                                     .ConfigureAwaitFalse();

            var transaction = AsanPardakhtHelper.CheckTransactionResult(response, context, _messageOptions);

            if (transaction.IsSucceed) return PaymentFetchResult.ReadyForVerifying();

            var failed = PaymentFetchResult.Failed(transaction.Message);
            failed.GatewayResponseCode = response.Code;

            return failed;
        }

        /// <inheritdoc />
        public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            if (!AsanPardakhtHelper.TryGetMerchantConfigurationId(account, out var merchantConfigurationId))
            {
                return PaymentVerifyResult.Failed(AsanPardakhtHelper.InvalidMerchantConfigurationIdMessage(account));
            }

            var response = await _api.GetTransactionResultAsync(account, merchantConfigurationId, context.Payment.TrackingNumber, cancellationToken)
                                     .ConfigureAwaitFalse();

            var transaction = AsanPardakhtHelper.CheckTransactionResult(response, context, _messageOptions);

            if (!transaction.IsSucceed)
            {
                return PaymentVerifyResult.Failed(transaction.Message, response.Code);
            }

            var payGateTranId = long.Parse(transaction.Result.PayGateTranId, NumberStyles.Integer, CultureInfo.InvariantCulture);

            // The transaction comes from TranResult for this invoice, so an "already verified" or "already settled"
            // answer (e.g. after a lost response) is about this invoice's own payment.
            var verification = await _api.VerifyAsync(account, merchantConfigurationId, payGateTranId, cancellationToken).ConfigureAwaitFalse();

            if (!verification.IsSucceed && !AsanPardakhtResultTranslator.IsAlreadyVerified(verification))
            {
                return PaymentVerifyResult.Failed(AsanPardakhtResultTranslator.Translate(AsanPardakhtOperation.Verify, verification, _messageOptions), verification.Code);
            }

            var settlement = await _api.SettleAsync(account, merchantConfigurationId, payGateTranId, cancellationToken).ConfigureAwaitFalse();

            var result = AsanPardakhtHelper.CreateVerifyResult(transaction.Result, _messageOptions);

            if (!settlement.IsSucceed && !AsanPardakhtResultTranslator.IsAlreadySettled(settlement))
            {
                // The payment is verified, so the money is captured. When settlement fails, AsanPardakht has Shaparak
                // settle it 12 hours after the transaction (guide v1.6, p.12), so the payment is still paid.
                result.GatewayResponseCode = settlement.Code;
                result.DatabaseAdditionalData.Add(AsanPardakhtHelper.SettlementResultDatabaseKey,
                                                  AsanPardakhtResultTranslator.Translate(AsanPardakhtOperation.Settlement, settlement, _messageOptions));
            }

            return result;
        }

        /// <inheritdoc />
        public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            return PaymentRefundResult.Failed("RefundNotSupports").ToInterfaceAsync();
        }
    }
}
