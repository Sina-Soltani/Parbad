using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Abstractions.Models;
using Parbad.Storage.EntityFrameworkCore.Builder;
using Parbad.Storage.EntityFrameworkCore.Context;
using Parbad.Storage.EntityFrameworkCore.Domain;

namespace Parbad.Storage.EntityFrameworkCore.Tests;

/// <summary>
/// Parbad's entities in the application's own DbContext (<c>modelBuilder.UseParbad()</c> and
/// <c>UseEfCore&lt;TContext&gt;()</c>), on SQLite.
/// </summary>
[TestFixture]
public class ApplicationDbContextTests
{
    private SqliteConnection _connection = null!;

    [SetUp]
    public void Setup()
    {
        // The database lives as long as this connection is open.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    [TearDown]
    public void Cleanup() => _connection.Dispose();

    [Test]
    public async Task Payments_are_stored_in_the_applications_database()
    {
        await using var services = BuildServices(storage => storage.UseEfCore<ShopDbContext>());
        await CreateDatabaseAsync(services);

        var storage = services.GetRequiredService<IStorage>();
        var payment = NewPayment(1001);

        await storage.CreatePaymentAsync(payment);
        await storage.CreateTransactionAsync(new Transaction { PaymentId = payment.Id, Amount = 1000, Type = TransactionType.Request });

        Assert.That(await storage.GetPaymentByTrackingNumberAsync(1001), Is.Not.Null);
        Assert.That((await storage.GetTransactionsAsync(payment)).Count, Is.EqualTo(1));

        await using var scope = services.CreateAsyncScope();
        var shop = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
        Assert.That(shop.Set<PaymentEntity>().Single().TrackingNumber, Is.EqualTo(1001), "The payment is not in the application's context.");
    }

    [Test]
    public async Task Parbad_does_not_save_changes_the_application_has_pending_on_its_own_context()
    {
        await using var services = BuildServices(storage => storage.UseEfCore<ShopDbContext>());
        await CreateDatabaseAsync(services);

        await using var scope = services.CreateAsyncScope();
        var shop = scope.ServiceProvider.GetRequiredService<ShopDbContext>();

        // The application has started an order but not saved it yet.
        shop.Orders.Add(new Order { Description = "not saved yet" });

        var storage = scope.ServiceProvider.GetRequiredService<IStorage>();
        var payment = NewPayment(1001);
        await storage.CreatePaymentAsync(payment);
        await storage.TryCompletePaymentAsync(payment.Id, PaymentStatus.Paid, "RRN", "RRN");

        await using var check = new ShopDbContext(Options());
        Assert.That(await check.Orders.CountAsync(), Is.EqualTo(0), "Parbad saved the application's pending order.");
        Assert.That((await storage.GetPaymentByTrackingNumberAsync(1001))!.IsPaid, Is.True);
    }

    [Test]
    public async Task Table_names_and_schema_come_from_UseParbad()
    {
        await using var context = new RenamedTablesDbContext(new DbContextOptionsBuilder<RenamedTablesDbContext>().UseSqlite(_connection).Options);

        var payment = context.Model.FindEntityType(typeof(PaymentEntity))!;
        var transaction = context.Model.FindEntityType(typeof(TransactionEntity))!;

        Assert.That(payment.GetTableName(), Is.EqualTo("shop_payment"));
        Assert.That(transaction.GetTableName(), Is.EqualTo("shop_transaction"));
        Assert.That(payment.GetSchema(), Is.EqualTo("billing"));
    }

    [Test]
    public async Task A_context_without_Parbads_entities_is_reported_clearly()
    {
        var services = new ServiceCollection();
        services.AddDbContextFactory<ContextWithoutParbad>(db => db.UseSqlite(_connection));
        new TestStorageBuilder(services).UseEfCore<ContextWithoutParbad>();

        await using var provider = services.BuildServiceProvider();
        var storage = provider.GetRequiredService<IStorage>();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => storage.DoesPaymentExistAsync(1));
        Assert.That(exception.Message, Does.Contain("modelBuilder.UseParbad()"));
    }

    [Test]
    public async Task A_context_registered_without_a_factory_is_reported_clearly()
    {
        var services = new ServiceCollection();
        services.AddDbContext<ShopDbContext>(db => db.UseSqlite(_connection));
        new TestStorageBuilder(services).UseEfCore<ShopDbContext>();

        await using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IStorage>());
        Assert.That(exception.Message, Does.Contain("AddDbContextFactory<ShopDbContext>"));
    }

    [Test]
    public async Task Parbads_own_context_takes_its_table_options_from_UseEfCore()
    {
        var services = new ServiceCollection();
        new TestStorageBuilder(services).UseEfCore(options =>
        {
            options.ConfigureDbContext = db => db.UseSqlite(_connection);
            options.PaymentTableOptions.Name = "parbad_payment";
        });

        await using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDbContextFactory<ParbadDataContext>>();

        await using (var context = await factory.CreateDbContextAsync())
        {
            Assert.That(context.Model.FindEntityType(typeof(PaymentEntity))!.GetTableName(), Is.EqualTo("parbad_payment"));
            await context.Database.EnsureCreatedAsync();
        }

        var storage = provider.GetRequiredService<IStorage>();
        await storage.CreatePaymentAsync(NewPayment(7));

        Assert.That(await storage.DoesPaymentExistAsync(7), Is.True);
    }

    private ServiceProvider BuildServices(Action<IStorageBuilder> configureStorage)
    {
        var services = new ServiceCollection();

        // AddDbContextFactory also registers the context itself as a scoped service.
        services.AddDbContextFactory<ShopDbContext>(db => db.UseSqlite(_connection));

        configureStorage(new TestStorageBuilder(services));

        return services.BuildServiceProvider();
    }

    private static async Task CreateDatabaseAsync(IServiceProvider services)
    {
        await using var context = await services.GetRequiredService<IDbContextFactory<ShopDbContext>>().CreateDbContextAsync();
        await context.Database.EnsureCreatedAsync();
    }

    private DbContextOptions<ShopDbContext> Options() => new DbContextOptionsBuilder<ShopDbContext>().UseSqlite(_connection).Options;

    private static Payment NewPayment(long trackingNumber) => new()
    {
        TrackingNumber = trackingNumber,
        Amount = 1000,
        Token = $"token-{trackingNumber}",
        GatewayName = "gateway",
        GatewayAccountName = "account"
    };

    private sealed class Order
    {
        public int Id { get; set; }

        public string Description { get; set; } = string.Empty;
    }

    private sealed class ShopDbContext(DbContextOptions<ShopDbContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // SQLite has no schemas; the tables keep their names in the default one.
            modelBuilder.UseParbad(options => options.DefaultSchema = null);
        }
    }

    private sealed class RenamedTablesDbContext(DbContextOptions<RenamedTablesDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseParbad(options =>
            {
                options.DefaultSchema = "billing";
                options.PaymentTableOptions.Name = "shop_payment";
                options.TransactionTableOptions.Name = "shop_transaction";
            });
        }
    }

    private sealed class ContextWithoutParbad(DbContextOptions<ContextWithoutParbad> options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();
    }

    private sealed class TestStorageBuilder(IServiceCollection services) : IStorageBuilder
    {
        public IServiceCollection Services { get; } = services;

        public IStorageBuilder AddStorage<TStorage>(ServiceLifetime lifetime) where TStorage : class, IStorage
        {
            Services.Add(new ServiceDescriptor(typeof(IStorage), typeof(TStorage), lifetime));
            return this;
        }

        public IStorageBuilder AddStorage(IStorage storage)
        {
            Services.AddSingleton(storage);
            return this;
        }

        public IStorageBuilder AddStorage(Func<IServiceProvider, IStorage> factory, ServiceLifetime lifetime)
        {
            Services.Add(new ServiceDescriptor(typeof(IStorage), factory, lifetime));
            return this;
        }
    }
}
