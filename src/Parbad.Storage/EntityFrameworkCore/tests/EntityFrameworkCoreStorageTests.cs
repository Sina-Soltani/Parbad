using KellermanSoftware.CompareNetObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Parbad.Storage.Abstractions.Models;
using Parbad.Storage.EntityFrameworkCore.Context;
using Parbad.Storage.EntityFrameworkCore.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parbad.Storage.EntityFrameworkCore.Tests
{
    [TestClass]
    public class EntityFrameworkCoreStorageTests
    {
        private ParbadDataContext _context;
        private EntityFrameworkCoreStorage _storage;

        private static readonly Payment PaymentTestData = new()
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

        private static readonly Transaction TransactionTestData = new()
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
            var contextOptions = new DbContextOptionsBuilder<ParbadDataContext>()
                                 .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                 .Options;

            var efCoreOptions = new OptionsWrapper<EntityFrameworkCoreOptions>(new EntityFrameworkCoreOptions());

            _context = new ParbadDataContext(contextOptions, efCoreOptions);

            _storage = new EntityFrameworkCoreStorage(_context);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await _context.DisposeAsync();
        }

        [TestMethod]
        public async Task Create_Payment_Works()
        {
            await _storage.CreatePaymentAsync(PaymentTestData);

            var payment = _storage.Payments.SingleOrDefault();

            Assert.IsNotNull(payment);
            Assert.AreEqual(1, _storage.Payments.Count());

            payment.ShouldCompare(PaymentTestData);
        }

        [TestMethod]
        public async Task Update_Payment_Works()
        {
            await _storage.CreatePaymentAsync(PaymentTestData);

            var payment = _storage.Payments.SingleOrDefault();

            Assert.IsNotNull(payment);

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
            newPayment.ShouldCompare(payment);
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

            Assert.IsNotNull(payment);

            TransactionTestData.PaymentId = payment.Id;

            await _storage.CreateTransactionAsync(TransactionTestData);

            var transaction = _storage.Transactions.SingleOrDefault();

            Assert.IsNotNull(transaction);
            Assert.AreEqual(1, _storage.Transactions.Count());

            transaction.ShouldCompare(TransactionTestData);
        }

        [TestMethod]
        public async Task Update_Transaction_Works()
        {
            await _storage.CreatePaymentAsync(PaymentTestData);

            var payment = _storage.Payments.SingleOrDefault();

            Assert.IsNotNull(payment);

            TransactionTestData.PaymentId = payment.Id;

            await _storage.CreateTransactionAsync(TransactionTestData);

            var transaction = _storage.Transactions.SingleOrDefault();

            Assert.IsNotNull(transaction);

            transaction.Amount = 2000;
            transaction.IsSucceed = true;
            transaction.Message = "NewMessage";
            transaction.Type = TransactionType.Verify;
            transaction.AdditionalData = "NewAdditionalData";

            await _storage.UpdateTransactionAsync(transaction);

            var newTransaction = _storage.Transactions.SingleOrDefault();

            Assert.IsNotNull(newTransaction);
            Assert.AreEqual(1, _storage.Transactions.Count());

            newTransaction.ShouldCompare(transaction);
        }

        [TestMethod]
        public async Task Delete_Transaction_Works()
        {
            await _storage.CreatePaymentAsync(PaymentTestData);

            var payment = _storage.Payments.SingleOrDefault();

            Assert.IsNotNull(payment);

            TransactionTestData.PaymentId = payment.Id;

            await _storage.CreateTransactionAsync(TransactionTestData);

            var transaction = _storage.Transactions.SingleOrDefault();

            await _storage.DeleteTransactionAsync(transaction);

            Assert.AreEqual(0, _storage.Transactions.Count());
        }

        [TestMethod]
        public async Task Paid_Payment_Lookup_Finds_Only_Paid_Payments_Of_The_Same_Gateway_Except_The_Given_One()
        {
            await _storage.CreatePaymentAsync(NewPayment(1, "RRN-1", "gateway", isPaid: true));
            await _storage.CreatePaymentAsync(NewPayment(2, "RRN-2", "gateway", isPaid: false));
            await _storage.CreatePaymentAsync(NewPayment(3, "RRN-3", "other", isPaid: true));

            var found = await _storage.FindPaidPaymentByTransactionCodeAsync("gateway", "RRN-1", exceptTrackingNumber: 99);

            Assert.AreEqual(1, found?.TrackingNumber);
            Assert.IsNull(await _storage.FindPaidPaymentByTransactionCodeAsync("gateway", "RRN-1", exceptTrackingNumber: 1), "The excluded payment was returned.");
            Assert.IsNull(await _storage.FindPaidPaymentByTransactionCodeAsync("gateway", "RRN-2", exceptTrackingNumber: 99), "An unpaid payment was returned.");
            Assert.IsNull(await _storage.FindPaidPaymentByTransactionCodeAsync("gateway", "RRN-3", exceptTrackingNumber: 99), "A payment of another gateway was returned.");
        }

        private static Payment NewPayment(long trackingNumber, string transactionCode, string gatewayName, bool isPaid) => new()
        {
            TrackingNumber = trackingNumber,
            Amount = 1000,
            Token = $"token-{trackingNumber}",
            TransactionCode = transactionCode,
            GatewayName = gatewayName,
            GatewayAccountName = "test",
            IsCompleted = true,
            IsPaid = isPaid
        };
    }
}
