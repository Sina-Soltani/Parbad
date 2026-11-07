using System;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Parbad.Builder;
using Parbad.GatewayBuilders;
using Parbad.PaymentFlow.Tests.Infrastructure;

namespace Parbad.PaymentFlow.Tests.Core;

/// <summary>
/// Parbad without dependency injection in the application (e.g. ASP.NET WebForms): <c>ParbadBuilder.Build()</c>
/// and <see cref="StaticOnlinePayment"/>.
/// </summary>
[TestFixture]
public class StaticOnlinePaymentTests
{
    /// <summary>Stands in for a service that must live for one request, such as the EF Core DbContext.</summary>
    private sealed class ScopedResource : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose() => IsDisposed = true;
    }

    [Test]
    public void Each_scope_has_its_own_scoped_services_and_releases_them_when_disposed()
    {
        var accessor = Build();

        var first = accessor.CreateScope();
        var second = accessor.CreateScope();

        Assert.That(first.OnlinePayment, Is.Not.Null);

        var firstResource = first.Services.GetRequiredService<ScopedResource>();
        var secondResource = second.Services.GetRequiredService<ScopedResource>();

        Assert.That(secondResource, Is.Not.SameAs(firstResource), "Two operations share a service that must live for one operation.");

        first.Dispose();

        Assert.That(firstResource.IsDisposed, Is.True, "The scope's services were not released.");
        Assert.That(secondResource.IsDisposed, Is.False);

        second.Dispose();
    }

    [Test]
    public void Static_scope_is_created_from_the_built_services()
    {
        Build();

        using var scope = StaticOnlinePayment.CreateScope();

        var onlinePayment = scope.OnlinePayment;

        Assert.That(onlinePayment, Is.Not.Null);
        Assert.That(scope.OnlinePayment, Is.SameAs(onlinePayment), "One scope should use one IOnlinePayment.");
    }

    private static IOnlinePaymentAccessor Build()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<ScopedResource>();
        services.AddSingleton(new FakeGatewayBehaviour());

        var httpContextAccessor = new TestHttpContextAccessor();

        return ParbadBuilder.CreateDefaultBuilder(services)
                            .ConfigureGateways(gateways => gateways
                                                           .AddGateway<FakeGateway>()
                                                           .WithAccounts<FakeGatewayAccount>(accounts => accounts.AddInMemory(_ => { })))
                            .ConfigureHttpContext(builder => builder.AddHttpContext(_ => httpContextAccessor, ServiceLifetime.Singleton))
                            .ConfigureStorage(builder => builder.AddStorage(new InMemoryStorage()))
                            .Build();
    }
}
