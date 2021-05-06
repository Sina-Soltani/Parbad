using KellermanSoftware.CompareNetObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Parbad.Storage.Abstractions.Models;
using Parbad.Storage.EntityFrameworkCore.Context;
using Parbad.Storage.EntityFrameworkCore.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parbad.Storage.EntityFrameworkCore.Tests
{
    public class EntityFrameworkStorageManagerTests
    {
        private ParbadDataContext _context;
        private EntityFrameworkCoreStorageManager _storageManager;

        private static readonly Payment PaymentTestData = new Payment
        {
            TrackingNumber = 1,
            Amount = 1,
            Token = "token",
            GatewayAccountName = "default",
            GatewayName = "gateway",
            IsCompleted = true,
            IsPaid = false,
            TransactionCode = "code"
        };

        [SetUp]
        public void Setup()
        {
            var contextOptions = new DbContextOptionsBuilder<ParbadDataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var efCoreOptions = new OptionsWrapper<EntityFrameworkCoreOptions>(new EntityFrameworkCoreOptions());

            _context = new ParbadDataContext(contextOptions, efCoreOptions);

            var storage = new EntityFrameworkCoreStorage(_context);

            _storageManager = new EntityFrameworkCoreStorageManager(storage);
        }

        [TearDown]
        public ValueTask Cleanup()
        {
            return _context.DisposeAsync();
        }

        [Test]
        public async Task GetPaymentByTrackingNumber_Must_Be_Equal_With_Expected_Payment_Object()
        {
            await _storageManager.CreatePaymentAsync(PaymentTestData);

            var payment = await _storageManager.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

            Assert.IsNotNull(payment);

            payment.ShouldCompare(PaymentTestData, "Payment is not equal with the expected Payment object.");
        }

        [Test]
        public async Task GetPaymentByToken_Must_Be_Equal_With_Expected_Payment_Object()
        {
            await _storageManager.CreatePaymentAsync(PaymentTestData);

            var payment = await _storageManager.GetPaymentByTokenAsync(PaymentTestData.Token);

            Assert.IsNotNull(payment);

            payment.ShouldCompare(PaymentTestData, "Payment is not equal with the expected Payment object.");
        }

        [Test]
        public async Task DoesPaymentExists_By_Token_Must_Be_True()
        {
            await _storageManager.CreatePaymentAsync(PaymentTestData);

            var exist = await _storageManager.DoesPaymentExistAsync(PaymentTestData.Token);

            Assert.IsTrue(exist);
        }

        [Test]
        public async Task DoesPaymentExists_By_TrackingNumber_Must_Be_True()
        {
            await _storageManager.CreatePaymentAsync(PaymentTestData);

            var exist = await _storageManager.DoesPaymentExistAsync(PaymentTestData.TrackingNumber);

            Assert.IsTrue(exist);
        }

        [Test]
        public async Task GetTransactions_Must_Be_Equal_With_Expected_Transactions()
        {
            await _storageManager.CreatePaymentAsync(PaymentTestData);

            var expectedTransactions = new List<Transaction>
            {
                new Transaction
                {
                    Amount = 1000,
                    IsSucceed = false,
                    Message = "test",
                    Type = TransactionType.Request,
                    AdditionalData = "test",
                    PaymentId = PaymentTestData.Id
                },
                new Transaction
                {
                    Amount = 1000,
                    IsSucceed = false,
                    Message = "test",
                    Type = TransactionType.Verify,
                    AdditionalData = "test",
                    PaymentId = PaymentTestData.Id
                },
                new Transaction
                {
                    Amount = 1000,
                    IsSucceed = false,
                    Message = "test",
                    Type = TransactionType.Refund,
                    AdditionalData = "test",
                    PaymentId = PaymentTestData.Id
                }
            };

            foreach (var transaction in expectedTransactions)
            {
                await _storageManager.CreateTransactionAsync(transaction);
            }

            var transactions = await _storageManager.GetTransactionsAsync(PaymentTestData);

            Assert.IsNotNull(transactions);
            Assert.AreEqual(expectedTransactions.Count, transactions.Count);
            transactions.ShouldCompare(expectedTransactions);
        }
    }
}
