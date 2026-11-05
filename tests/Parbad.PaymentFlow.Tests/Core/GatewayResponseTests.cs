using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Exceptions;
using Parbad.Net;

namespace Parbad.PaymentFlow.Tests.Core;

/// <summary>
/// The shared reader for gateway responses: a readable answer is returned, anything else throws
/// <see cref="GatewayResponseException"/>, which is an <see cref="HttpRequestException"/>.
/// </summary>
[TestClass]
public class GatewayResponseTests
{
    private const string HtmlErrorPage = "<html><head><title>502 Bad Gateway</title></head><body><h1>502 Bad Gateway</h1></body></html>";

    private sealed class Model
    {
        public int Code { get; set; }
    }

    [TestMethod]
    public async Task Json_body_is_deserialized_even_with_an_error_status_code()
    {
        var model = await Response("{\"code\":53}", "application/json", HttpStatusCode.BadRequest).ReadJsonAsync<Model>();

        Assert.AreEqual(53, model.Code);
    }

    [TestMethod]
    [DataRow(HtmlErrorPage, "text/html")]
    [DataRow("", "application/json")]
    [DataRow("null", "application/json")]
    [DataRow("{\"code\":", "application/json")]
    public async Task Unreadable_json_throws_a_gateway_response_exception(string body, string contentType)
    {
        var exception = await ThrowsAsync(() => Response(body, contentType, HttpStatusCode.BadGateway).ReadJsonAsync<Model>());

        Assert.IsInstanceOfType(exception, typeof(HttpRequestException), "Callers that handle network errors must handle it too.");
        Assert.AreEqual(HttpStatusCode.BadGateway, exception.ResponseStatusCode);
        Assert.AreEqual(contentType, exception.ContentType);
    }

    [TestMethod]
    public async Task Json_string_is_returned_unchanged()
    {
        var body = await Response("[1,2]", "application/json").ReadJsonStringAsync();

        Assert.AreEqual("[1,2]", body);
    }

    [TestMethod]
    public async Task Html_is_not_accepted_as_a_json_string()
    {
        await ThrowsAsync(() => Response(HtmlErrorPage, "text/html", HttpStatusCode.BadGateway).ReadJsonStringAsync());
    }

    [TestMethod]
    [DataRow("<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body /></soap:Envelope>")]
    [DataRow("<?xml version=\"1.0\"?>\r\n<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Body><s:Fault /></s:Body></s:Envelope>")]
    public async Task Soap_envelope_including_a_fault_is_returned(string body)
    {
        Assert.AreEqual(body, await Response(body, "text/xml", HttpStatusCode.InternalServerError).ReadSoapAsync());
    }

    [TestMethod]
    [DataRow(HtmlErrorPage)]
    [DataRow("")]
    [DataRow("Service Unavailable")]
    [DataRow("<!DOCTYPE Envelope [<!ENTITY x \"y\">]><Envelope>&x;</Envelope>")]
    public async Task Anything_but_a_soap_envelope_throws(string body)
    {
        await ThrowsAsync(() => Response(body, "text/html", HttpStatusCode.BadGateway).ReadSoapAsync());
    }

    private static HttpResponseMessage Response(string body, string contentType, HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new(statusCode) { Content = new StringContent(body, Encoding.UTF8, contentType) };

    private static async Task<GatewayResponseException> ThrowsAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (GatewayResponseException exception)
        {
            return exception;
        }

        throw new AssertFailedException($"Expected {nameof(GatewayResponseException)}.");
    }
}
