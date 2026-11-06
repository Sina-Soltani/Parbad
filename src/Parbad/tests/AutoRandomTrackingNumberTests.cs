using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Cache.MemoryCache;
using Parbad.TrackingNumberProviders;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Parbad.Tests;

[TestFixture]
public class AutoRandomTrackingNumberTests
{
    private IStorage _storage;
    private IOptions<AutoRandomTrackingNumberOptions> _options;
    private AutoRandomTrackingNumber _instance;
    private Invoice _invoice;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection()
                      .AddMemoryCache()
                      .BuildServiceProvider();

        var memoryCache = services.GetRequiredService<IMemoryCache>();
        var memoryCacheOptions = new OptionsWrapper<MemoryCacheStorageOptions>(new MemoryCacheStorageOptions());
        _storage = new MemoryCacheStorage(memoryCache, memoryCacheOptions);

        _options = new OptionsWrapper<AutoRandomTrackingNumberOptions>(new AutoRandomTrackingNumberOptions());

        _instance = new AutoRandomTrackingNumber(_storage, _options);

        _invoice = new Invoice();
    }

    [TearDown]
    public async Task TrackingNumber_Must_Be_A_Positive_Number()
    {
        await _instance.FormatAsync(_invoice);

        Assert.That(_invoice.TrackingNumber > 0, Is.True);
    }

    [Test]
    public async Task TrackingNumber_Must_Be_Greater_Than_MinimumNumber()
    {
        const long expectedMinimumNumber = 5;

        _options.Value.MinimumValue = expectedMinimumNumber;

        await _instance.FormatAsync(_invoice);

        Assert.That(_invoice.TrackingNumber >= _options.Value.MinimumValue, Is.True);
    }
}