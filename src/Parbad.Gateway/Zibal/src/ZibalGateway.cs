using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Parbad.Abstraction;
using Parbad.Gateway.Zibal.Internal;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;

namespace Parbad.Gateway.Zibal
{
    [Gateway(Name)]
    public class ZibalGateway: GatewayBase<ZibalGatewayAccount>
    {
        public const string Name = "Zibal";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly ZibalGatewayOptions _gatewayOptions;
        private readonly ParbadOptions _options;
        private static JsonSerializerSettings DefaultSerializerSettings => new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        public ZibalGateway(
            IGatewayAccountProvider<ZibalGatewayAccount> accountProvider,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptions<ZibalGatewayOptions> gatewayOptions,
            IOptions<ParbadOptions> options) : base(accountProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient(Name);
            _gatewayOptions = gatewayOptions.Value;
            _options = options.Value;
        }
    

        public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var account = await GetAccountAsync(invoice).ConfigureAwaitFalse();

            var data = ZibalHelper.CreateRequestData(invoice, account);

            var responseMessage = await _httpClient
                .PostJsonAsync(_gatewayOptions.RequestURl, data, DefaultSerializerSettings, cancellationToken)
                .ConfigureAwaitFalse();

            return await ZibalHelper.CreateRequestResult(responseMessage, _httpContextAccessor.HttpContext, account, _gatewayOptions, _options.Messages);
        }

        public override Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            IPaymentFetchResult result = PaymentFetchResult.ReadyForVerifying();

            return Task.FromResult(result);
        }

        public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

            var data = ZibalHelper.CreateVerifyData(context.Transactions, account);

            var responseMessage = await _httpClient
                .PostJsonAsync(_gatewayOptions.VerifyURl, data, DefaultSerializerSettings, cancellationToken)
                .ConfigureAwaitFalse();

            return await ZibalHelper.CreateVerifyResult(responseMessage, _options.Messages);
        }

        public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
        {
            return PaymentRefundResult.Failed("The Refund operation is not supported by this gateway.").ToInterfaceAsync();
        }
    }
}
