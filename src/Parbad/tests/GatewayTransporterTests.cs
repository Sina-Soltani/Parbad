using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Parbad.Internal;
using Parbad.Tests.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parbad.Tests
{
    [TestClass]
    public class GatewayTransporterTests
    {
        private HttpContext _httpContext;

        [TestInitialize]
        public void Setup()
        {
            var httpContextAccessor = MockHelpers.MockHttpContextAccessor();

            _httpContext = httpContextAccessor.HttpContext;

            // The transporter reads ParbadOptions (for the CSP nonce) from the request services.
            _httpContext.RequestServices = new ServiceCollection().AddOptions().BuildServiceProvider();
            _httpContext.Response.Body = new MemoryStream();
        }

        [TestMethod]
        public async Task Transporter_Redirects_Correctly()
        {
            const string expectedUrl = "http://test.com";

            var descriptor = GatewayTransporterDescriptor.CreateRedirect(expectedUrl);

            IGatewayTransporter transporter = new DefaultGatewayTransporter(_httpContext, descriptor);

            await transporter.TransportAsync();

            Assert.AreEqual((int)HttpStatusCode.Redirect, _httpContext.Response.StatusCode);
            Assert.IsNotNull(_httpContext.Response.Headers[HeaderNames.Location]);
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

            var html = Encoding.UTF8.GetString(((MemoryStream)_httpContext.Response.Body).ToArray());
            StringAssert.Contains(html, $"action=\"{expectedUrl}\"");
        }
    }
}
