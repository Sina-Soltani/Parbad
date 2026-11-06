using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Parbad.Exceptions;
using Parbad.Net;

namespace Parbad.PaymentFlow.Tests.Core;

/// <summary>
/// The shared reader for gateway responses: a readable answer is returned, anything else throws
/// <see cref="GatewayResponseException"/>, which is an <see cref="HttpRequestException"/>.
/// </summary>
[TestFixture]
public class GatewayResponseTests
{
    private const string HtmlErrorPage = "<html><head><title>502 Bad Gateway</title></head><body><h1>502 Bad Gateway</h1></body></html>";

    private sealed class Model
    {
        public int Code { get; set; }
    }

    [Test]
    public async Task Json_body_is_deserialized_even_with_an_error_status_code()
    {
        var model = await Response("{\"code\":53}", "application/json", HttpStatusCode.BadRequest).ReadJsonAsync<Model>();

        Assert.That(model.Code, Is.EqualTo(53));
    }

    [TestCase(HtmlErrorPage, "text/html")]
    [TestCase("", "application/json")]
    [TestCase("null", "application/json")]
    [TestCase("{\"code\":", "application/json")]
    public async Task Unreadable_json_throws_a_gateway_response_exception(string body, string contentType)
    {
        var exception = await Assert.CatchAsync<GatewayResponseException>(() => Response(body, contentType, HttpStatusCode.BadGateway).ReadJsonAsync<Model>());

        Assert.That(exception, Is.InstanceOf<HttpRequestException>(), "Callers that handle network errors must handle it too.");
        Assert.That(exception.ResponseStatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
        Assert.That(exception.ContentType, Is.EqualTo(contentType));
    }

    [Test]
    public async Task Json_string_is_returned_unchanged()
    {
        var body = await Response("[1,2]", "application/json").ReadJsonStringAsync();

        Assert.That(body, Is.EqualTo("[1,2]"));
    }

    [Test]
    public async Task Html_is_not_accepted_as_a_json_string()
    {
        await Assert.CatchAsync<GatewayResponseException>(() => Response(HtmlErrorPage, "text/html", HttpStatusCode.BadGateway).ReadJsonStringAsync());
    }

    [TestCase("<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body /></soap:Envelope>")]
    [TestCase("<?xml version=\"1.0\"?>\r\n<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Body><s:Fault /></s:Body></s:Envelope>")]
    public async Task Soap_envelope_including_a_fault_is_returned(string body)
    {
        Assert.That(await Response(body, "text/xml", HttpStatusCode.InternalServerError).ReadSoapAsync(), Is.EqualTo(body));
    }

    [TestCase(HtmlErrorPage)]
    [TestCase("")]
    [TestCase("Service Unavailable")]
    [TestCase("<!DOCTYPE Envelope [<!ENTITY x \"y\">]><Envelope>&x;</Envelope>")]
    public async Task Anything_but_a_soap_envelope_throws(string body)
    {
        await Assert.CatchAsync<GatewayResponseException>(() => Response(body, "text/html", HttpStatusCode.BadGateway).ReadSoapAsync());
    }

    private static HttpResponseMessage Response(string body, string contentType, HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new(statusCode) { Content = new StringContent(body, Encoding.UTF8, contentType) };
}
