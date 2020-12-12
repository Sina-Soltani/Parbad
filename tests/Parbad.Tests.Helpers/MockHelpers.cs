using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Net.Http;

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

        public static IHttpClientFactory MockHttpClientFactory(Action<MockHttpMessageHandler> configureHttpClient)
        {
            var mockHttp = new MockHttpMessageHandler();
            configureHttpClient(mockHttp);

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(mockHttp.ToHttpClient);

            return httpClientFactoryMock.Object;
        }
    }
}
