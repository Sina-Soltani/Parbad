using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Tests.Helpers;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parbad.Tests
{
    [TestClass]
    public class GatewayTransporterTests
    {
        private HttpContext _httpContext;
        private ServiceProvider _services;

        [TestInitialize]
        public void Setup()
        {
            var httpContextAccessor = MockHelpers.MockHttpContextAccessor();

            _httpContext = httpContextAccessor.HttpContext;
            _services = new ServiceCollection()
                        .Configure<ParbadOptions>(_ => { })
                        .BuildServiceProvider();
            _httpContext.RequestServices = _services;
        }

        [TestCleanup]
        public Task Cleanup()
        {
            return _services.DisposeAsync().AsTask();
        }

        [TestMethod]
        public async Task Transporter_Redirects_Correctly()
        {
            const string expectedUrl = "http://test.com";

            var descriptor = GatewayTransporterDescriptor.CreateRedirect(expectedUrl);

            IGatewayTransporter transporter = new DefaultGatewayTransporter(_httpContext, descriptor);

            await transporter.TransportAsync();

            Assert.AreEqual((int)HttpStatusCode.Redirect, _httpContext.Response.StatusCode);
            Assert.IsFalse(string.IsNullOrEmpty(_httpContext.Response.Headers[HeaderNames.Location].ToString()));
            Assert.AreEqual(expectedUrl, _httpContext.Response.Headers[HeaderNames.Location].ToString());
        }

        [TestMethod]
        public async Task Transporter_Posts_Correctly()
        {
            const string expectedUrl = "http://test.com";

            var descriptor = GatewayTransporterDescriptor.CreatePost(expectedUrl, new List<KeyValuePair<string, string>>());

            IGatewayTransporter transporter = new DefaultGatewayTransporter(_httpContext, descriptor);

            await transporter.TransportAsync();

            Assert.AreEqual((int)HttpStatusCode.OK, _httpContext.Response.StatusCode);
            Assert.IsNotNull(_httpContext.Response.Body);
        }
    }
}
