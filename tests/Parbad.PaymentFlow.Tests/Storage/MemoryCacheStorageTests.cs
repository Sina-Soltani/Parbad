using Parbad.Storage.Abstractions.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Parbad.Builder;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Cache.MemoryCache;

namespace Parbad.PaymentFlow.Tests.Storage;

/// <summary>
/// <see cref="MemoryCacheStorage"/>, registered with <c>UseMemoryCache</c>. Every instance that uses the same
/// <see cref="IMemoryCache"/> works on the same data, so each instance stands for a concurrent request.
/// </summary>
[TestFixture]
public class MemoryCacheStorageTests : CacheStorageTests
{
    protected override int ParallelCreateCount => 2000;

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddParbad().ConfigureStorage(builder => builder.UseMemoryCache());
    }

    [Test]
    public async Task Storages_that_use_the_same_memory_cache_see_each_others_data()
    {
        var first = CreateStorage();
        var second = CreateStorage();
        var constructedDirectly = new MemoryCacheStorage(
            Services.GetRequiredService<IMemoryCache>(),
            Services.GetRequiredService<IOptions<MemoryCacheStorageOptions>>());

        Assert.That(second, Is.Not.SameAs(first));

        var payment = NewPayment(1001);
        await first.CreatePaymentAsync(payment);

        AssertSamePayment(payment, await second.GetPaymentByTrackingNumberAsync(1001));
        AssertSamePayment(payment, await constructedDirectly.GetPaymentByTokenAsync(payment.Token));

        payment.Status = PaymentStatus.Paid;
        await second.UpdatePaymentAsync(payment);

        AssertSamePayment(payment, await first.GetPaymentByTrackingNumberAsync(1001));

        var transaction = NewTransaction(payment.Id);
        await constructedDirectly.CreateTransactionAsync(transaction);

        AssertSameTransaction(transaction, (await first.GetTransactionsAsync(payment)).Single());

        await second.DeletePaymentAsync(payment);

        Assert.That(await first.DoesPaymentExistAsync(1001), Is.False);
        Assert.That((await constructedDirectly.GetTransactionsAsync(payment)).Count, Is.EqualTo(0));
    }

    [Test]
    public async Task Parallel_creates_from_separate_storages_are_all_stored_with_unique_ids()
    {
        var payments = Enumerable.Range(1, ParallelCreateCount).Select(i => NewPayment(1000 + i)).ToList();

        // Each task resolves its own storage while the cache is still empty, so this also covers
        // the creation of the shared collection.
        await Task.WhenAll(payments.Select(payment => Task.Run(async () =>
        {
            IStorage storage = CreateStorage();

            await storage.CreatePaymentAsync(payment);
            await storage.CreateTransactionAsync(NewTransaction(payment.Id));
            Assert.That(await CreateStorage().GetPaymentByTrackingNumberAsync(payment.TrackingNumber), Is.Not.Null);
        })));

        await AssertAllStoredAsync(CreateStorage(), payments);
    }
}
