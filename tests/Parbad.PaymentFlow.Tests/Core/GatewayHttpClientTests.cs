using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Parbad.Builder;
using Parbad.Gateway.Mellat;
using Parbad.Gateway.Pasargad;
using Parbad.GatewayBuilders;

namespace Parbad.PaymentFlow.Tests.Core;

/// <summary>
/// The HTTP clients Parbad uses to call the banks. These tests build the services themselves,
/// because <see cref="Infrastructure.ParbadTestHost"/> replaces every client's handler and timeout.
/// </summary>
[TestFixture]
public class GatewayHttpClientTests
{
    [Test]
    public void Gateway_client_has_a_30_second_timeout_by_default()
    {
        using var services = Build(gateways => gateways.AddMellat());

        Assert.That(CreateClient(services, nameof(MellatGateway)).Timeout, Is.EqualTo(TimeSpan.FromSeconds(30)));
    }

    [Test]
    public void Gateway_client_uses_the_configured_gateway_timeout()
    {
        using var services = Build(gateways => gateways.AddMellat(), options => options.GatewayTimeout = TimeSpan.FromSeconds(12));

        Assert.That(CreateClient(services, nameof(MellatGateway)).Timeout, Is.EqualTo(TimeSpan.FromSeconds(12)));
    }

    [Test]
    public void Timeout_configured_for_one_gateway_takes_precedence()
    {
        using var services = Build(gateways => gateways
                                               .AddMellat()
                                               .WithHttpClient(client => client.ConfigureHttpClient(httpClient => httpClient.Timeout = TimeSpan.FromSeconds(77))));

        Assert.That(CreateClient(services, nameof(MellatGateway)).Timeout, Is.EqualTo(TimeSpan.FromSeconds(77)));
    }

    [Test]
    public void Typed_gateway_client_gets_the_default_timeout_and_its_own_configuration()
    {
        // Pasargad calls the bank through a typed client named after its API interface.
        using var services = Build(gateways => gateways.AddPasargad().WithOptions(options => options.ApiBaseUrl = "https://bank.test/"));

        var client = CreateClient(services, "IPasargadApi");

        Assert.That(client.Timeout, Is.EqualTo(TimeSpan.FromSeconds(30)));
        Assert.That(client.BaseAddress, Is.Not.Null, "The gateway's own configuration of the client was lost.");
    }

    private static ServiceProvider Build(Action<IGatewayBuilder> configureGateways, Action<Options.ParbadOptions>? configureOptions = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var parbad = services.AddParbad().ConfigureGateways(configureGateways);

        if (configureOptions != null) parbad.ConfigureOptions(configureOptions);

        return services.BuildServiceProvider();
    }

    private static HttpClient CreateClient(IServiceProvider services, string name) =>
        services.GetRequiredService<IHttpClientFactory>().CreateClient(name);
}
