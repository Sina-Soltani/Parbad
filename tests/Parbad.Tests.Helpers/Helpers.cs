using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
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

namespace Parbad.Tests.Helpers
{
    public static class Helpers
    {
        public static async Task TestGatewayAsync<TGateway>(
            string gatewayName,
            Func<IGatewayBuilder, IGatewayConfigurationBuilder<TGateway>> configureGateways,
            Action<MockHttpMessageHandler> configureHttpClient,
            Action<HttpContext> configureHttpContext,
            Action<IPaymentRequestResult> onRequestResult,
            Action<IPaymentFetchResult> onFetchResult,
            Action<IPaymentVerifyResult> onVerifyResult,
            Action<IPaymentRefundResult> onRefundResult = null)
            where TGateway : class, IGateway
        {
            var httpContextAccessor = MockHttpContextAccessor();
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

            var requestResult = await onlinePayment.RequestAsync(gatewayName, 1, 1000, "http://www.mysite.com");

            onRequestResult(requestResult);

            var storageManager = serviceProvider.GetRequiredService<IStorageManager>();

            var payment = await storageManager.GetPaymentByTrackingNumberAsync(1);

            var queries = httpContext.Request
                .Query
                .ToDictionary(_ => _.Key, _ => _.Value);

            queries.Add("paymentToken", payment.Token);
            httpContext.Request.Query = new QueryCollection(queries);

            var invoice = await onlinePayment.FetchAsync();

            onFetchResult(invoice);

            var verificationResult = await onlinePayment.VerifyAsync(invoice);

            onVerifyResult(verificationResult);

            if (onRefundResult != null)
            {
                var refundResult = await onlinePayment.RefundCompletelyAsync(verificationResult);

                onRefundResult?.Invoke(refundResult);
            }
        }

        public static IHttpContextAccessor MockHttpContextAccessor(
            Dictionary<string, StringValues> queries = null,
            Dictionary<string, StringValues> form = null)
        {
            var httpContext = new DefaultHttpContext();

            if (queries != null)
            {
                httpContext.Request.Query = new QueryCollection(queries);
            }

            if (form != null)
            {
                httpContext.Request.Form = new FormCollection(form);
            }

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(_ => _.HttpContext).Returns(httpContext);

            return httpContextAccessorMock.Object;
        }

        public static HttpMessageHandler MockHttpMessageHandler(HttpResponseMessage response)
        {
            var httpClientHandlerMock = new Mock<HttpMessageHandler>();
            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            return httpClientHandlerMock.Object;
        }

        public static IHttpClientFactory MockHttpClientFactory(Func<HttpResponseMessage> response)
        {
            var httpClientHandlerMock = new Mock<HttpMessageHandler>();
            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(httpClientHandlerMock.Object);
            httpClient.BaseAddress = new Uri("http://www.test.com/");

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            return httpClientFactoryMock.Object;
        }

        public static IGatewayAccountProvider<T> MockAccountProvider<T>(T account) where T : GatewayAccount
        {
            var accountCollectionMock = new Mock<IGatewayAccountCollection<T>>();

            accountCollectionMock
                .Setup(_ => _.Get(It.IsAny<string>()))
                .Returns(account);

            accountCollectionMock
                .Setup(_ => _.GetDefaultAccount())
                .Returns(account);

            var providerMock = new Mock<IGatewayAccountProvider<T>>();

            providerMock
                .Setup(_ => _.LoadAccountsAsync())
                .ReturnsAsync(() => accountCollectionMock.Object);

            return providerMock.Object;
        }
    }
}
