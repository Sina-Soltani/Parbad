using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Gateway.ParbadVirtual;
using Parbad.Gateway.ParbadVirtual.MiddlewareInvoker;
using Parbad.Internal;
using Parbad.Options;
using Parbad.PaymentFlow.Tests.Infrastructure;

namespace Parbad.PaymentFlow.Tests.Core;

/// <summary>
/// The HTML that Parbad writes to the browser: the auto-post form of <see cref="DefaultGatewayTransporter"/> and the
/// Virtual Gateway pages.
/// </summary>
[TestClass]
public class HtmlRenderingTests
{
    private const string Payload = "\"><script>alert(1)</script>";
    private const string EncodedPayload = "&quot;&gt;&lt;script&gt;alert(1)&lt;/script&gt;";
    private const string Nonce = "r4nd0m-n0nce";
    private const string GatewayPath = "/virtual-gateway";

    private ParbadTestHost _host = null!;

    [TestInitialize]
    public void Initialize()
    {
        _host = ParbadTestHost.Create(
            gateways => gateways.AddParbadVirtual().WithOptions(options => options.GatewayPath = GatewayPath),
            services => services.Configure<ParbadOptions>(options => options.NonceFactory = _ => Nonce));
    }

    [TestCleanup]
    public Task Cleanup() => _host.DisposeAsync().AsTask();

    // ---------- Auto-post form ----------

    [TestMethod]
    public async Task Post_form_encodes_the_url_and_the_fields()
    {
        var descriptor = GatewayTransporterDescriptor.CreatePost(
            "https://bank.test/pay?x=" + Payload,
            new[] { new KeyValuePair<string, string>("field" + Payload, "value" + Payload) });

        var body = await TransportAsync(descriptor);

        StringAssert.Contains(body, $"action=\"https://bank.test/pay?x={EncodedPayload}\"");
        StringAssert.Contains(body, $"name=\"field{EncodedPayload}\"");
        StringAssert.Contains(body, $"value=\"value{EncodedPayload}\"");
        Assert.IsFalse(body.Contains("<script>alert(1)"), body);
    }

    [TestMethod]
    public async Task Post_form_is_valid_markup_with_a_noscript_fallback_and_the_nonce()
    {
        var descriptor = GatewayTransporterDescriptor.CreatePost(
            "https://bank.test/pay",
            new[] { new KeyValuePair<string, string>("token", "abc") });

        var body = await TransportAsync(descriptor);

        Assert.IsTrue(body.StartsWith("<!DOCTYPE html>"), body);
        StringAssert.Contains(body, "<meta charset=\"utf-8\">");
        StringAssert.Contains(body, "<form id=\"paymentForm\" action=\"https://bank.test/pay\" method=\"post\"><input");
        StringAssert.Matches(body, new Regex("<noscript><button type=\"submit\">[^<]+</button></noscript></form>"));
        StringAssert.Contains(body, $"<script type=\"text/javascript\" nonce=\"{Nonce}\">document.getElementById('paymentForm').submit();</script>");
    }

    // ---------- CORS headers ----------

    [TestMethod]
    public async Task Post_transport_adds_no_cors_headers()
    {
        await TransportAsync(GatewayTransporterDescriptor.CreatePost("https://bank.test/pay", new List<KeyValuePair<string, string>>()));

        AssertNoCorsHeaders(_host.HttpContextAccessor.HttpContext!.Response);
    }

    [TestMethod]
    public async Task Redirect_transport_adds_no_cors_headers()
    {
        await TransportAsync(GatewayTransporterDescriptor.CreateRedirect("https://bank.test/pay"));

        var response = _host.HttpContextAccessor.HttpContext!.Response;
        Assert.AreEqual(StatusCodes.Status302Found, response.StatusCode);
        AssertNoCorsHeaders(response);
    }

    // ---------- Virtual Gateway ----------

    [TestMethod]
    [DataRow("Request", "javascript:alert(1)")]
    [DataRow("Pay", "javascript:alert(1)")]
    [DataRow("Request", "JavaScript:alert(1)")]
    [DataRow("Request", "data:text/html,<script>alert(1)</script>")]
    [DataRow("Request", "/payment/callback")]
    public async Task Virtual_gateway_rejects_a_redirect_url_that_is_not_absolute_http(string commandType, string redirectUrl)
    {
        var (response, body) = await InvokeVirtualGatewayAsync(commandType, redirectUrl);

        Assert.AreEqual(StatusCodes.Status400BadRequest, response.StatusCode);
        Assert.IsFalse(body.Contains(redirectUrl), body);
        Assert.IsFalse(body.Contains("<form"), body);
    }

    [TestMethod]
    [DataRow("Request")]
    [DataRow("Pay")]
    public async Task Virtual_gateway_encodes_the_redirect_url(string commandType)
    {
        var (response, body) = await InvokeVirtualGatewayAsync(commandType, "https://shop.test/callback?x=" + Payload);

        Assert.AreEqual(StatusCodes.Status200OK, response.StatusCode);
        StringAssert.Contains(body, $"\"https://shop.test/callback?x={EncodedPayload}\"");
        Assert.IsFalse(body.Contains("<script>alert(1)"), body);
        StringAssert.Contains(body, $"<script type=\"text/javascript\"  nonce=\"{Nonce}\">");
        AssertNoCorsHeaders(response);
    }

    private async Task<string> TransportAsync(GatewayTransporterDescriptor descriptor)
    {
        var httpContext = NewRequest("GET", "https://shop.test/checkout", null);

        await new DefaultGatewayTransporter(httpContext, descriptor).TransportAsync();

        return ReadBody(httpContext.Response);
    }

    private async Task<(HttpResponse Response, string Body)> InvokeVirtualGatewayAsync(string commandType, string redirectUrl)
    {
        var httpContext = NewRequest("POST", "https://shop.test" + GatewayPath, new Dictionary<string, string>
        {
            ["commandType"] = commandType,
            ["trackingNumber"] = "1001",
            ["amount"] = "1250000",
            ["redirectUrl"] = redirectUrl,
            ["isPaid"] = "true"
        });

        // Resolved after the request is set: the invoker reads the HTTP context when it is created.
        var invoker = _host.Services.GetRequiredService<IParbadVirtualGatewayMiddlewareInvoker>();

        await invoker.InvokeAsync();

        return (httpContext.Response, ReadBody(httpContext.Response));
    }

    private HttpContext NewRequest(string method, string url, IEnumerable<KeyValuePair<string, string>>? form)
    {
        var httpContext = TestHttpContextAccessor.CreateRequest(method, url, form, _host.Services);
        httpContext.Response.Body = new MemoryStream();

        _host.HttpContextAccessor.HttpContext = httpContext;

        return httpContext;
    }

    private static string ReadBody(HttpResponse response) =>
        Encoding.UTF8.GetString(((MemoryStream)response.Body).ToArray());

    private static void AssertNoCorsHeaders(HttpResponse response)
    {
        var corsHeaders = response.Headers.Keys.Where(key => key.StartsWith("Access-Control-")).ToList();

        Assert.AreEqual(0, corsHeaders.Count, "Unexpected headers: " + string.Join(", ", corsHeaders));
        Assert.AreEqual("no-store, no-cache", response.Headers["Cache-Control"].ToString());
    }
}
