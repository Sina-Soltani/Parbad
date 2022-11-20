using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Parbad.Storage.Abstractions.Models;
using Parbad.Storage.Cache.DistributedCache;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parbad.Storage.Cache.Tests
{
    [TestClass]
    public class DistributedMemoryCacheTests
    {
        private ServiceProvider _services;
        private DistributedCacheStorage _storage;

        private static Payment PaymentTestData => new Payment
        {
            TrackingNumber = 1,
            Amount = 1000,
            Token = "token",
            TransactionCode = "test",
            GatewayName = "gateway",
            GatewayAccountName = "test",
            IsPaid = false,
            IsCompleted = false
        };

        private static Transaction TransactionTestData => new Transaction
        {
            PaymentId = 1,
            Amount = 1000,
            IsSucceed = false,
            Message = "test",
            Type = TransactionType.Request,
            AdditionalData = "test"
        };

        [TestInitialize]
        public void Setup()
        {
            _services = new ServiceCollection()
                .AddDistributedMemoryCache()
                .Configure<DistributedCacheStorageOptions>(_ => { })
                .BuildServiceProvider();

            var memoryCache = _services.GetRequiredService<IDistributedCache>();
            var options = _services.GetRequiredService<IOptions<DistributedCacheStorageOptions>>();

            _storage = new DistributedCacheStorage(memoryCache, options);
        }

        [TestCleanup]
        public ValueTask Cleanup()
        {
            return _services.DisposeAsync();
        }

        [TestMethod]
        public async Task Create_Payment_Works()
        {
            await _storage.CreatePaymentAsync(PaymentTestData);

            var payment = _storage.Payments.SingleOrDefault();

            Assert.IsNotNull(payment);
            Assert.AreEqual(1, _storage.Payments.Count());

            Assert.AreEqual(1, payment.Id);
            Assert.AreEqual(PaymentTestData.TrackingNumber, payment.TrackingNumber);
            Assert.AreEqual(PaymentTestData.Amount, payment.Amount);
            Assert.AreEqual(PaymentTestData.TransactionCode, payment.TransactionCode);
            Assert.AreEqual(PaymentTestData.GatewayName, payment.GatewayName);
            Assert.AreEqual(PaymentTestData.GatewayAccountName, payment.GatewayAccountName);
            Assert.AreEqual(PaymentTestData.Token, payment.Token);
            Assert.AreEqual(PaymentTestData.IsPaid, payment.IsPaid);
            Assert.AreEqual(PaymentTestData.IsCompleted, payment.IsCompleted);
        }

        [TestMethod]
        public async Task Update_Payment_Works()
        {
            await _storage.CreatePaymentAsync(PaymentTestData);

            var payment = _storage.Payments.SingleOrDefault();
            payment.TrackingNumber = 2;
            payment.Amount = 2000;
            payment.Token = "NewToken";
            payment.TransactionCode = "NewCode";
            payment.GatewayName = "NewGateway";
            payment.GatewayAccountName = "NewAccount";
            payment.IsPaid = true;
            payment.IsCompleted = true;

            await _storage.UpdatePaymentAsync(payment);

            var newPayment = _storage.Payments.SingleOrDefault();

            Assert.IsNotNull(newPayment);
            Assert.AreEqual(1, newPayment.Id);
            Assert.AreEqual(payment.TrackingNumber, newPayment.TrackingNumber);
            Assert.AreEqual(payment.Amount, newPayment.Amount);
            Assert.AreEqual(payment.TransactionCode, newPayment.TransactionCode);
            Assert.AreEqual(payment.GatewayName, newPayment.GatewayName);
            Assert.AreEqual(payment.GatewayAccountName, newPayment.GatewayAccountName);
            Assert.AreEqual(payment.Token, newPayment.Token);
            Assert.AreEqual(payment.IsPaid, newPayment.IsPaid);
            Assert.AreEqual(payment.IsCompleted, newPayment.IsCompleted);
        }

        [TestMethod]
        public async Task Delete_Payment_Works()
        {
            await _storage.CreatePaymentAsync(PaymentTestData);

            var payment = _storage.Payments.SingleOrDefault();

            await _storage.DeletePaymentAsync(payment);

            Assert.AreEqual(0, _storage.Payments.Count());
        }

        [TestMethod]
        public async Task Create_Transaction_Works()
        {
            await _storage.CreatePaymentAsync(PaymentTestData);

            var payment = _storage.Payments.SingleOrDefault();

            TransactionTestData.PaymentId = payment.Id;

            await _storage.CreateTransactionAsync(TransactionTestData);

            var transaction = _storage.Transactions.SingleOrDefault();

            Assert.IsNotNull(transaction);
            Assert.AreEqual(1, _storage.Transactions.Count());

            Assert.AreEqual(1, transaction.Id);
            Assert.AreEqual(payment.Id, transaction.PaymentId);
            Assert.AreEqual(TransactionTestData.Amount, transaction.Amount);
            Assert.AreEqual(TransactionTestData.AdditionalData, transaction.AdditionalData);
            Assert.AreEqual(TransactionTestData.IsSucceed, transaction.IsSucceed);
            Assert.AreEqual(TransactionTestData.Type, transaction.Type);
            Assert.AreEqual(TransactionTestData.Message, transaction.Message);
        }

        [TestMethod]
        public async Task Update_Transaction_Works()
        {
            await _storage.CreatePaymentAsync(PaymentTestData);

            var payment = _storage.Payments.SingleOrDefault();

            TransactionTestData.PaymentId = payment.Id;

            await _storage.CreateTransactionAsync(TransactionTestData);

            var transaction = _storage.Transactions.SingleOrDefault();
            transaction.Amount = 2000;
            transaction.IsSucceed = true;
            transaction.Message = "NewMessage";
            transaction.Type = TransactionType.Verify;
            transaction.AdditionalData = "NewAdditionalData";

            await _storage.UpdateTransactionAsync(transaction);

            var newTransaction = _storage.Transactions.SingleOrDefault();

            Assert.IsNotNull(newTransaction);
            Assert.AreEqual(1, _storage.Transactions.Count());

            Assert.AreEqual(1, transaction.Id);
            Assert.AreEqual(transaction.Id, newTransaction.PaymentId);
            Assert.AreEqual(transaction.Amount, newTransaction.Amount);
            Assert.AreEqual(transaction.AdditionalData, newTransaction.AdditionalData);
            Assert.AreEqual(transaction.IsSucceed, newTransaction.IsSucceed);
            Assert.AreEqual(transaction.Type, newTransaction.Type);
            Assert.AreEqual(transaction.Message, newTransaction.Message);
        }

        [TestMethod]
        public async Task Delete_Transaction_Works()
        {
            await _storage.CreatePaymentAsync(PaymentTestData);

            var payment = _storage.Payments.SingleOrDefault();

            TransactionTestData.PaymentId = payment.Id;

            await _storage.CreateTransactionAsync(TransactionTestData);

            var transaction = _storage.Transactions.SingleOrDefault();

            await _storage.DeleteTransactionAsync(transaction);

            Assert.AreEqual(0, _storage.Transactions.Count());
        }
    }
}
