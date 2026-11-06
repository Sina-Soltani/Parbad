using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Cache.MemoryCache;

namespace Parbad.PaymentFlow.Tests.Storage;

/// <summary>
/// <see cref="MemoryCacheStorage"/>, registered with <c>UseMemoryCache</c>. Every instance that uses the same
/// <see cref="IMemoryCache"/> works on the same data, so each instance stands for a concurrent request.
/// </summary>
[TestClass]
public class MemoryCacheStorageTests : CacheStorageTests
{
    protected override int ParallelCreateCount => 2000;

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddParbad().ConfigureStorage(builder => builder.UseMemoryCache());
    }

    [TestMethod]
    public async Task Storages_that_use_the_same_memory_cache_see_each_others_data()
    {
        var first = CreateStorage();
        var second = CreateStorage();
        var constructedDirectly = new MemoryCacheStorage(
            Services.GetRequiredService<IMemoryCache>(),
            Services.GetRequiredService<IOptions<MemoryCacheStorageOptions>>());

        Assert.AreNotSame(first, second);

        var payment = NewPayment(1001);
        await first.CreatePaymentAsync(payment);

        AssertSamePayment(payment, await second.GetPaymentByTrackingNumberAsync(1001));
        AssertSamePayment(payment, await constructedDirectly.GetPaymentByTokenAsync(payment.Token));

        payment.IsPaid = true;
        payment.IsCompleted = true;
        await second.UpdatePaymentAsync(payment);

        AssertSamePayment(payment, await first.GetPaymentByTrackingNumberAsync(1001));

        var transaction = NewTransaction(payment.Id);
        await constructedDirectly.CreateTransactionAsync(transaction);

        AssertSameTransaction(transaction, (await first.GetTransactionsAsync(payment)).Single());

        await second.DeletePaymentAsync(payment);

        Assert.IsFalse(await first.DoesPaymentExistAsync(1001));
        Assert.AreEqual(0, (await constructedDirectly.GetTransactionsAsync(payment)).Count);
    }

    [TestMethod]
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
            Assert.IsNotNull(await CreateStorage().GetPaymentByTrackingNumberAsync(payment.TrackingNumber));
        })));

        await AssertAllStoredAsync(CreateStorage(), payments);
    }
}
