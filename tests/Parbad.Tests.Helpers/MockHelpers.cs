using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Moq.Protected;
using Parbad.Abstraction;
using Parbad.GatewayBuilders;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Tests.Helpers
{
    public static class MockHelpers
    {
        public static IHttpContextAccessor MockHttpContextAccessor(
            Dictionary<string, StringValues> queries = null,
            Dictionary<string, StringValues> form = null)
        {
            var httpContext = new DefaultHttpContext();

            if (queries != null)
            {
                //httpContext.Request.Query = new QueryCollection(queries);
            }

            if (form != null)
            {
                //httpContext.Request.Form = new FormCollection(form);
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
