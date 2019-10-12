// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.GatewayBuilders;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;

namespace Parbad.GatewayProviders.ParbadVirtual
{
    [Gateway(Name)]
    public class ParbadVirtualGateway : Gateway<ParbadVirtualGatewayAccount>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<ParbadVirtualGatewayOptions> _options;
        private readonly IOptions<MessagesOptions> _messageOptions;

        public const string Name = "ParbadVirtual";

        public ParbadVirtualGateway(
            IHttpContextAccessor httpContextAccessor,
            IOptions<ParbadVirtualGatewayOptions> options,
            IGatewayAccountProvider<ParbadVirtualGatewayAccount> accountProvider,
            IOptions<MessagesOptions> messageOptions) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = options;
            _messageOptions = messageOptions;
        }

        /// <inheritdoc />
        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            var request = _httpContextAccessor.HttpContext.Request;

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var url = $"{request.Scheme}" +
                      "://" +
                      $"{request.Host.ToUriComponent()}" +
                      $"{_options.Value.GatewayPath}";

            var transporter = new GatewayPost(
                _httpContextAccessor,
                url,
                new Dictionary<string, string>
                {
                    {"CommandType",  "request"},
                    {"trackingNumber", invoice.TrackingNumber.ToString() },
                    {"amount",  ((long)invoice.Amount).ToString()},
                    {"redirectUrl", invoice.CallbackUrl }
                });

            return PaymentRequestResult.Succeed(transporter, account.Name);
        }

        /// <inheritdoc />
        public override Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (!_httpContextAccessor.HttpContext.Request.TryGetParam("Result", out var result))
            {
                return PaymentVerifyResult.Failed(_messageOptions.Value.InvalidDataReceivedFromGateway).ToInterfaceAsync();
            }

            _httpContextAccessor.HttpContext.Request.TryGetParam("TransactionCode", out var transactionCode);

            var isSucceed = result.Equals("true", StringComparison.OrdinalIgnoreCase);

            var message = isSucceed ? _messageOptions.Value.PaymentSucceed : _messageOptions.Value.PaymentFailed;

            return new PaymentVerifyResult
            {
                IsSucceed = isSucceed,
                TransactionCode = transactionCode,
                Message = message
            }.ToInterfaceAsync();
        }

        /// <inheritdoc />
        public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            return PaymentRefundResult.Succeed().ToInterfaceAsync();
        }
    }
}
