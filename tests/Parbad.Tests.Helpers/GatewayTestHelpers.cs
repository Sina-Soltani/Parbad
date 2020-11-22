using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Moq;
using Moq.Protected;
using Parbad.Abstraction;
using Parbad.Builder;
using Parbad.GatewayBuilders;
using Parbad.Storage.Abstractions;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Parbad.InvoiceBuilder;
using Parbad.PaymentTokenProviders;

namespace Parbad.Tests.Helpers
{
    public static class GatewayTestHelpers
    {
        public static async Task TestGatewayAsync<TGateway>(
            Func<IGatewayBuilder, IGatewayConfigurationBuilder<TGateway>> configureGateways,
            Action<IInvoiceBuilder> configureInvoice,
            Action<MockHttpMessageHandler> configureHttpClient,
            Action<HttpContext> configureHttpContext,
            Action<IPaymentRequestResult> onRequestResult,
            Action<IPaymentFetchResult> onFetchResult,
            Action<IPaymentVerifyResult> onVerifyResult,
            Action<IPaymentRefundResult> onRefundResult = null)
            where TGateway : class, IGateway
        {
            var httpContextAccessor = MockHelpers.MockHttpContextAccessor();
            var httpContext = httpContextAccessor.HttpContext;
            configureHttpContext(httpContext);

            var mockHttp = new MockHttpMessageHandler();
            configureHttpClient(mockHttp);

            var services = new ServiceCollection();

            services.AddParbad()
                .ConfigureGateways(gateways =>
                {
                    configureGateways(gateways)
                        .WithHttpClient(builder => builder.ConfigurePrimaryHttpMessageHandler(() => mockHttp));
                })
                .ConfigureHttpContext(builder => builder.AddHttpContext(_ => httpContextAccessor, ServiceLifetime.Transient))
                .ConfigureStorage(builder => builder.UseMemoryCache());

            var serviceProvider = services.BuildServiceProvider();

            var onlinePayment = serviceProvider.GetRequiredService<IOnlinePayment>();

            var requestResult = await onlinePayment.RequestAsync(configureInvoice);

            LogWhenResultIsNotSucceed(requestResult);
            onRequestResult(requestResult);

            var storageManager = serviceProvider.GetRequiredService<IStorageManager>();

            var payment = await storageManager.GetPaymentByTrackingNumberAsync(requestResult.TrackingNumber);

            var queries = httpContext.Request
                .Query
                .ToDictionary(_ => _.Key, _ => _.Value);

            queries.Add(QueryStringPaymentTokenOptions.DefaultQueryName, payment.Token);

            httpContext.Request.Query = new TestableQueryCollection(queries);

            var invoice = await onlinePayment.FetchAsync();

            LogWhenResultIsNotSucceed(invoice);
            onFetchResult(invoice);

            var verificationResult = await onlinePayment.VerifyAsync(invoice);

            LogWhenResultIsNotSucceed(verificationResult);
            onVerifyResult(verificationResult);

            if (onRefundResult != null)
            {
                var refundResult = await onlinePayment.RefundCompletelyAsync(verificationResult);

                LogWhenResultIsNotSucceed(refundResult);
                onRefundResult?.Invoke(refundResult);
            }
        }

        public static void LogWhenResultIsNotSucceed(IPaymentResult result)
        {
            if (result != null && !result.IsSucceed)
            {
                Console.WriteLine($"Result message: {result.Message}");
            }
        }
    }
}
