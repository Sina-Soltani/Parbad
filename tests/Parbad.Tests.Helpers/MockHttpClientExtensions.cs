using System;
using RichardSzalay.MockHttp;
using System.Net.Http;
using Newtonsoft.Json;

namespace Parbad.Tests.Helpers
{
    public static class MockHttpClientExtensions
    {
        public static MockedRequest WithHttpMethod(this MockedRequest mockedRequest, HttpMethod httpMethod)
        {
            return mockedRequest.With(message => message.Method == httpMethod);
        }

        public static MockedRequest WithJsonBody<TJsonModel>(this MockedRequest mockedRequest, Func<TJsonModel, bool> isJsonValid) where TJsonModel : class
        {
            return mockedRequest.With(message =>
            {
                if (message.Content == null) return false;

                var content = message.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var model = JsonConvert.DeserializeObject<TJsonModel>(content);

                var isValid = isJsonValid(model);

                return isValid;
            });
        }
    }
}
