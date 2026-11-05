using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.GatewayBuilders;
using Parbad.PaymentFlow.Tests.Infrastructure;

namespace Parbad.PaymentFlow.Tests.Core;

/// <summary>
/// Parbad without dependency injection in the application (e.g. ASP.NET WebForms): <c>ParbadBuilder.Build()</c>
/// and <see cref="StaticOnlinePayment"/>.
/// </summary>
[TestClass]
public class StaticOnlinePaymentTests
{
    /// <summary>Stands in for a service that must live for one request, such as the EF Core DbContext.</summary>
    private sealed class ScopedResource : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose() => IsDisposed = true;
    }

    [TestMethod]
    public void Each_scope_has_its_own_scoped_services_and_releases_them_when_disposed()
    {
        var accessor = Build();

        var first = accessor.CreateScope();
        var second = accessor.CreateScope();

        Assert.IsNotNull(first.OnlinePayment);

        var firstResource = first.Services.GetRequiredService<ScopedResource>();
        var secondResource = second.Services.GetRequiredService<ScopedResource>();

        Assert.AreNotSame(firstResource, secondResource, "Two operations share a service that must live for one operation.");

        first.Dispose();

        Assert.IsTrue(firstResource.IsDisposed, "The scope's services were not released.");
        Assert.IsFalse(secondResource.IsDisposed);

        second.Dispose();
    }

    [TestMethod]
    public void Static_scope_is_created_from_the_built_services()
    {
        Build();

        using var scope = StaticOnlinePayment.CreateScope();

        Assert.IsNotNull(scope.OnlinePayment);
        Assert.AreSame(scope.OnlinePayment, scope.OnlinePayment, "One scope should use one IOnlinePayment.");
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
