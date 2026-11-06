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
using NUnit.Framework;

namespace Parbad.Tests;

[TestFixture]
public class GatewayTransporterTests
{
    private HttpContext _httpContext;

    [SetUp]
    public void Setup()
    {
        var httpContextAccessor = MockHelpers.MockHttpContextAccessor();

        _httpContext = httpContextAccessor.HttpContext;

        // The transporter reads ParbadOptions (for the CSP nonce) from the request services.
        _httpContext.RequestServices = new ServiceCollection().AddOptions().BuildServiceProvider();
        _httpContext.Response.Body = new MemoryStream();
    }

    [Test]
    public async Task Transporter_Redirects_Correctly()
    {
        const string expectedUrl = "http://test.com";

        var descriptor = GatewayTransporterDescriptor.CreateRedirect(expectedUrl);

        IGatewayTransporter transporter = new DefaultGatewayTransporter(_httpContext, descriptor);

        await transporter.TransportAsync();

        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.Redirect));
        Assert.That(_httpContext.Response.Headers[HeaderNames.Location].ToString(), Is.EqualTo(expectedUrl));
    }

    [Test]
    public async Task Transporter_Posts_Correctly()
    {
        const string expectedUrl = "http://test.com";

        var descriptor = GatewayTransporterDescriptor.CreatePost(expectedUrl, new List<KeyValuePair<string, string>>());

        IGatewayTransporter transporter = new DefaultGatewayTransporter(_httpContext, descriptor);

        await transporter.TransportAsync();

        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));

        var html = Encoding.UTF8.GetString(((MemoryStream)_httpContext.Response.Body).ToArray());
        Assert.That(html, Does.Contain($"action=\"{expectedUrl}\""));
    }
}