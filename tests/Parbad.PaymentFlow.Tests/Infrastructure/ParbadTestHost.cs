using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Parbad.Builder;
using Parbad.GatewayBuilders;
using Parbad.PaymentTokenProviders;
using Parbad.Storage.Abstractions.Models;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Infrastructure;

/// <summary>
/// Hosts the real Parbad pipeline (<see cref="IOnlinePayment"/>, gateways, token provider) with an
/// in-memory storage, a replaceable HTTP context and a mocked HTTP layer.
/// Every <see cref="HttpClient"/> created by <see cref="IHttpClientFactory"/> is routed to <see cref="Http"/>,
/// whatever its name, so no test can reach a real bank.
/// </summary>
public sealed class ParbadTestHost : IAsyncDisposable
{
    private ParbadTestHost(ServiceProvider services, MockHttpMessageHandler http, InMemoryStorage storage, TestHttpContextAccessor httpContextAccessor)
    {
        Services = services;
        Http = http;
        Storage = storage;
        HttpContextAccessor = httpContextAccessor;
    }

    public ServiceProvider Services { get; }

    public MockHttpMessageHandler Http { get; }

    public InMemoryStorage Storage { get; }

    public TestHttpContextAccessor HttpContextAccessor { get; }

    public IOnlinePayment OnlinePayment => Services.GetRequiredService<IOnlinePayment>();

    public static ParbadTestHost Create(
        Action<IGatewayBuilder> configureGateways,
        Action<IServiceCollection>? configureServices = null)
    {
        var unexpectedCalls = new List<string>();
        var http = new MockHttpMessageHandler();

        // Answered (not thrown): MockHttp 6 would swallow the exception and the call would hang.
        http.Fallback.Respond(request =>
        {
            lock (unexpectedCalls) unexpectedCalls.Add($"{request.Method} {request.RequestUri}");

            return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound) { Content = new StringContent("No simulated endpoint.") };
        });

        var storage = new InMemoryStorage();
        var httpContextAccessor = new TestHttpContextAccessor();

        var services = new ServiceCollection();
        services.AddLogging();

        services.AddParbad()
                .ConfigureGateways(configureGateways)
                .ConfigureHttpContext(builder => builder.AddHttpContext(_ => httpContextAccessor, ServiceLifetime.Singleton))
                .ConfigureStorage(builder => builder.AddStorage(storage));

        // Registered last, so it wins over any primary handler a gateway or proxy setting configures.
        // The short timeout turns a hanging simulated call into a quick failure.
        services.ConfigureAll<HttpClientFactoryOptions>(options =>
        {
            options.HttpMessageHandlerBuilderActions.Add(builder => builder.PrimaryHandler = http);
            options.HttpClientActions.Add(client => client.Timeout = TimeSpan.FromSeconds(10));
        });

        configureServices?.Invoke(services);

        var provider = services.BuildServiceProvider();

        httpContextAccessor.HttpContext!.RequestServices = provider;

        return new ParbadTestHost(provider, http, storage, httpContextAccessor) { _unexpectedCalls = unexpectedCalls };
    }

    private List<string> _unexpectedCalls = new();

    /// <summary>HTTP calls that matched no simulated endpoint.</summary>
    public IReadOnlyList<string> UnexpectedHttpCalls
    {
        get
        {
            lock (_unexpectedCalls) return _unexpectedCalls.ToList();
        }
    }

    /// <summary>
    /// Replaces the current request with the bank's callback to <paramref name="callbackUrl"/>.
    /// </summary>
    public void ReceiveCallback(string callbackUrl, TestCallback callback)
    {
        var url = callback.Query.Count == 0
            ? callbackUrl
            : Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(callbackUrl, callback.Query.ToDictionary(pair => pair.Key, pair => (string?)pair.Value));

        HttpContextAccessor.HttpContext = TestHttpContextAccessor.CreateRequest(
            callback.Method,
            url,
            callback.Method == HttpMethods.Post ? callback.Form : null,
            Services);
    }

    /// <summary>
    /// Starts a new checkout request (the user's browser on the merchant's site).
    /// </summary>
    public void StartCheckoutRequest()
    {
        HttpContextAccessor.HttpContext = TestHttpContextAccessor.CreateRequest("POST", "https://shop.test/checkout", Array.Empty<KeyValuePair<string, string>>(), Services);
    }

    public async Task<Payment> GetPaymentAsync(long trackingNumber) =>
        await Storage.GetPaymentByTrackingNumberAsync(trackingNumber)
        ?? throw new InvalidOperationException($"Payment {trackingNumber} was not stored.");

    public async Task<string> GetCallbackUrlAsync(long trackingNumber, string invoiceCallbackUrl)
    {
        var payment = await GetPaymentAsync(trackingNumber);

        return Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(invoiceCallbackUrl, QueryStringPaymentTokenOptions.DefaultQueryName, payment.Token);
    }

    public ValueTask DisposeAsync() => Services.DisposeAsync();
}

/// <summary>
/// The request a bank sends (or makes the browser send) to the merchant's callback URL.
/// </summary>
public sealed class TestCallback
{
    public string Method { get; init; } = HttpMethods.Post;

    public IList<KeyValuePair<string, string>> Form { get; init; } = new List<KeyValuePair<string, string>>();

    public IList<KeyValuePair<string, string>> Query { get; init; } = new List<KeyValuePair<string, string>>();

    public static TestCallback Post(params (string Key, string Value)[] form) => new()
    {
        Method = HttpMethods.Post,
        Form = form.Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value)).ToList()
    };

    public static TestCallback Get(params (string Key, string Value)[] query) => new()
    {
        Method = HttpMethods.Get,
        Query = query.Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value)).ToList()
    };
}
