using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.Storage.Cache.DistributedCache;

namespace Parbad.PaymentFlow.Tests.Storage;

/// <summary>
/// <see cref="DistributedCacheStorage"/>, registered with <c>UseDistributedCache</c> over a
/// <c>MemoryDistributedCache</c>. Instances that write at the same time overwrite each other's data, so
/// the tests use one instance for the writes; a new instance reads what was saved to the cache.
/// </summary>
[TestClass]
public class DistributedCacheStorageTests : CacheStorageTests
{
    // Every save serializes the whole dataset, so a large count only makes the test slow.
    protected override int ParallelCreateCount => 200;

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
        services.AddParbad().ConfigureStorage(builder => builder.UseDistributedCache());
    }

    [TestMethod]
    public async Task Storage_is_registered_as_the_distributed_cache_storage()
    {
        Assert.IsInstanceOfType(CreateStorage(), typeof(DistributedCacheStorage));

        var payment = NewPayment(1001);
        await CreateStorage().CreatePaymentAsync(payment);

        AssertSamePayment(payment, CreateStorage().Payments.Single());
    }
}
