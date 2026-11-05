using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.Gateway.Mellat;
using Parbad.Gateway.Pasargad;
using Parbad.GatewayBuilders;

namespace Parbad.PaymentFlow.Tests.Core;

/// <summary>
/// The HTTP clients Parbad uses to call the banks (ANALYSIS-01, H-3). These tests build the services themselves,
/// because <see cref="Infrastructure.ParbadTestHost"/> replaces every client's handler and timeout.
/// </summary>
[TestClass]
public class GatewayHttpClientTests
{
    [TestMethod]
    public void Gateway_client_has_a_30_second_timeout_by_default()
    {
        using var services = Build(gateways => gateways.AddMellat());

        Assert.AreEqual(TimeSpan.FromSeconds(30), CreateClient(services, nameof(MellatGateway)).Timeout);
    }

    [TestMethod]
    public void Gateway_client_uses_the_configured_gateway_timeout()
    {
        using var services = Build(gateways => gateways.AddMellat(), options => options.GatewayTimeout = TimeSpan.FromSeconds(12));

        Assert.AreEqual(TimeSpan.FromSeconds(12), CreateClient(services, nameof(MellatGateway)).Timeout);
    }

    [TestMethod]
    public void Timeout_configured_for_one_gateway_takes_precedence()
    {
        using var services = Build(gateways => gateways
                                               .AddMellat()
                                               .WithHttpClient(client => client.ConfigureHttpClient(httpClient => httpClient.Timeout = TimeSpan.FromSeconds(77))));

        Assert.AreEqual(TimeSpan.FromSeconds(77), CreateClient(services, nameof(MellatGateway)).Timeout);
    }

    [TestMethod]
    public void Typed_gateway_client_gets_the_default_timeout_and_its_own_configuration()
    {
        // Pasargad calls the bank through a typed client named after its API interface.
        using var services = Build(gateways => gateways.AddPasargad().WithOptions(options => options.ApiBaseUrl = "https://bank.test/"));

        var client = CreateClient(services, "IPasargadApi");

        Assert.AreEqual(TimeSpan.FromSeconds(30), client.Timeout);
        Assert.IsNotNull(client.BaseAddress, "The gateway's own configuration of the client was lost.");
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
