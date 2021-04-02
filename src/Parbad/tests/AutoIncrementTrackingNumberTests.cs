using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Parbad.Abstraction;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Abstractions.Models;
using Parbad.Storage.Cache.MemoryCache;
using Parbad.TrackingNumberProviders;
using System.Threading.Tasks;

namespace Parbad.Tests
{
    public class AutoIncrementTrackingNumberTests
    {
        private IStorage _storage;
        private IOptions<AutoTrackingNumberOptions> _options;
        private AutoIncrementTrackingNumber _instance;
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

            _options = new OptionsWrapper<AutoTrackingNumberOptions>(new AutoTrackingNumberOptions());

            _instance = new AutoIncrementTrackingNumber(_storage, _options);

            _invoice = new Invoice();
        }

        [Test]
        public async Task TrackingNumber_Must_Be_A_Positive_Number()
        {
            await _instance.FormatAsync(_invoice);

            Assert.IsTrue(_invoice.TrackingNumber > 0);
        }

        [Test]
        public async Task NewTrackingNumber_Must_Be_Greater_Than_Latest_Number()
        {
            var expectedNumber = AutoTrackingNumberOptions.DefaultMinimumNumber + 1;

            await _storage.CreatePaymentAsync(new Payment
            {
                TrackingNumber = AutoTrackingNumberOptions.DefaultMinimumNumber
            });

            await _instance.FormatAsync(_invoice);

            Assert.AreEqual(expectedNumber, _invoice.TrackingNumber);
        }

        [Test]
        public async Task TrackingNumber_Must_Be_Equal_With_MinimumNumber()
        {
            const long expectedMinimumNumber = 5;

            _options.Value.MinimumValue = expectedMinimumNumber;

            await _instance.FormatAsync(_invoice);

            Assert.AreEqual(expectedMinimumNumber, _invoice.TrackingNumber);
        }

        [Test]
        public async Task TrackingNumber_Must_Be_Less_Than_MaximumNumber()
        {
            const long expectedMinimumNumber = 5;

            _options.Value.MinimumValue = expectedMinimumNumber;

            await _instance.FormatAsync(_invoice);

            Assert.AreEqual(expectedMinimumNumber, _invoice.TrackingNumber);
        }
    }
}
