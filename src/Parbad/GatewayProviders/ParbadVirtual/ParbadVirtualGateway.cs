// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Data.Domain.Payments;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;

namespace Parbad.GatewayProviders.ParbadVirtual
{
    [Gateway(Name)]
    public class ParbadVirtualGateway : IGateway
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<ParbadVirtualGatewayOptions> _options;
        private readonly IOptions<MessagesOptions> _messageOptions;

        public const string Name = "ParbadVirtual";

        public ParbadVirtualGateway(
            IHttpContextAccessor httpContextAccessor,
            IOptions<ParbadVirtualGatewayOptions> options,
            IOptions<MessagesOptions> messageOptions)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = options;
            _messageOptions = messageOptions;
        }

        public virtual Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            var request = _httpContextAccessor.HttpContext.Request;

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

            return PaymentRequestResult.Succeed(transporter).ToInterfaceAsync();
        }

        public virtual Task<IPaymentVerifyResult> VerifyAsync(Payment payment, CancellationToken cancellationToken = default)
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

        public virtual Task<IPaymentRefundResult> RefundAsync(Payment payment, Money amount, CancellationToken cancellationToken = default)
        {
            return PaymentRefundResult.Succeed().ToInterfaceAsync();
        }
    }
}
