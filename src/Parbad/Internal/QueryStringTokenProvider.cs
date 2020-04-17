// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Http;
using Parbad.PaymentTokenProviders;

namespace Parbad.Internal
{
    public abstract class QueryStringTokenProvider : IPaymentTokenProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<QueryStringPaymentTokenOptions> _options;

        protected QueryStringTokenProvider(IHttpContextAccessor httpContextAccessor, IOptions<QueryStringPaymentTokenOptions> options)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = options;
        }

        public virtual async Task<string> ProvideTokenAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var token = await GenerateTokenAsync(invoice, cancellationToken).ConfigureAwaitFalse();

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new Exception();
            }

            invoice.CallbackUrl = invoice.CallbackUrl.AddQueryString(GetQueryName(), token);

            return token;
        }

        public virtual Task<string> RetrieveTokenAsync(CancellationToken cancellationToken = default)
        {
            _httpContextAccessor.HttpContext.Request.Query.TryGetValue(
                GetQueryName(),
                out var paymentToken);

            return Task.FromResult<string>(paymentToken);
        }

        protected abstract Task<string> GenerateTokenAsync(Invoice invoice, CancellationToken cancellationToken = default);

        protected virtual string GetQueryName()
        {
            return string.IsNullOrEmpty(_options.Value.QueryName)
                ? QueryStringPaymentTokenOptions.DefaultQueryName
                : _options.Value.QueryName;
        }
    }
}
