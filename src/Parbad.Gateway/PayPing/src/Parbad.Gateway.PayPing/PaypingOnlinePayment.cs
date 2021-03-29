﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Parbad.Abstraction;
using Parbad.Exceptions;
using Parbad.Internal;
using Parbad.Options;
using Parbad.PaymentTokenProviders;
using Parbad.Storage.Abstractions;

namespace Parbad.Gateway.PayPing
{
    public class PayPingOnlinePayment : DefaultOnlinePayment
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PayPingOnlinePayment(IHttpContextAccessor httpContextAccessor,IServiceProvider services, IStorageManager storageManager, IPaymentTokenProvider tokenProvider, IGatewayProvider gatewayProvider, IOptions<ParbadOptions> options, ILogger<DefaultOnlinePayment> logger) : base(services, storageManager, tokenProvider, gatewayProvider, options, logger)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            invoice.GatewayName = PayPingGateway.Name;
            return await base.RequestAsync(invoice, cancellationToken);
        }

        public override async Task<IPaymentFetchResult> FetchAsync(CancellationToken cancellationToken = default)
        {
            var body =await _httpContextAccessor.HttpContext.Request.ReadFormAsync(cancellationToken);
            var refId = StringValues.Empty;
            body.TryGetValue("refid", out refId);
            return await base.FetchAsync(long.Parse(refId), cancellationToken);

        }
    }
}