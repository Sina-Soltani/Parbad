using KellermanSoftware.CompareNetObjects;
using Microsoft.EntityFrameworkCore;
using Parbad.Storage.Abstractions.Models;
using Parbad.Storage.EntityFrameworkCore.Context;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parbad.Storage.EntityFrameworkCore.Tests;

[TestClass]
public class EntityFrameworkCoreStorageTests
{
    private EntityFrameworkCoreStorage<ParbadDataContext> _storage;

    private static readonly Payment PaymentTestData = new()
                                                      {
                                                          TrackingNumber = 1,
                                                          Amount = 1000,
                                                          Token = "token",
                                                          TransactionCode = "test",
                                                          GatewayName = "gateway",
                                                          GatewayAccountName = "test",
                                                          Status = PaymentStatus.Requested
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

        _storage = new EntityFrameworkCoreStorage<ParbadDataContext>(new TestDbContextFactory<ParbadDataContext>(() => new ParbadDataContext(contextOptions)));
    }

    [TestMethod]
    public async Task Largest_tracking_number_is_null_without_payments_and_the_maximum_otherwise()
    {
        Assert.IsNull(await _storage.GetLargestTrackingNumberAsync());

        foreach (var trackingNumber in new long[] { 5, 42, 7 })
        {
            await _storage.CreatePaymentAsync(new Payment
                                              {
                                                  TrackingNumber = trackingNumber,
                                                  Amount = 1000,
                                                  Token = $"token-{trackingNumber}",
                                                  GatewayName = "gateway",
                                                  GatewayAccountName = "test"
                                              });
        }

        Assert.AreEqual(42, await _storage.GetLargestTrackingNumberAsync());
    }

    [TestMethod]
    public async Task Create_Payment_Works()
    {
        await _storage.CreatePaymentAsync(PaymentTestData);

        var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

        Assert.IsNotNull(payment);
        Assert.IsTrue(await _storage.DoesPaymentExistAsync(PaymentTestData.TrackingNumber));

        payment.ShouldCompare(PaymentTestData);
    }

    [TestMethod]
    public async Task Update_Payment_Works()
    {
        await _storage.CreatePaymentAsync(PaymentTestData);

        var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

        Assert.IsNotNull(payment);

        payment.TrackingNumber = 2;
        payment.Amount = 2000;
        payment.Token = "NewToken";
        payment.TransactionCode = "NewCode";
        payment.GatewayName = "NewGateway";
        payment.GatewayAccountName = "NewAccount";
        payment.Status = PaymentStatus.Paid;

        await _storage.UpdatePaymentAsync(payment);

        var newPayment = await _storage.GetPaymentByTrackingNumberAsync(payment.TrackingNumber);

        Assert.IsNotNull(newPayment);
        newPayment.ShouldCompare(payment);
    }

    [TestMethod]
    public async Task Delete_Payment_Works()
    {
        await _storage.CreatePaymentAsync(PaymentTestData);

        var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

        await _storage.DeletePaymentAsync(payment);

        Assert.IsFalse(await _storage.DoesPaymentExistAsync(PaymentTestData.TrackingNumber));
    }

    [TestMethod]
    public async Task Create_Transaction_Works()
    {
        await _storage.CreatePaymentAsync(PaymentTestData);

        var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

        Assert.IsNotNull(payment);

        TransactionTestData.PaymentId = payment.Id;

        await _storage.CreateTransactionAsync(TransactionTestData);

        var transaction = (await _storage.GetTransactionsAsync(payment)).SingleOrDefault();

        Assert.IsNotNull(transaction);
        Assert.AreEqual(1, (await _storage.GetTransactionsAsync(payment)).Count);

        transaction.ShouldCompare(TransactionTestData);
    }

    [TestMethod]
    public async Task Update_Transaction_Works()
    {
        await _storage.CreatePaymentAsync(PaymentTestData);

        var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

        Assert.IsNotNull(payment);

        TransactionTestData.PaymentId = payment.Id;

        await _storage.CreateTransactionAsync(TransactionTestData);

        var transaction = (await _storage.GetTransactionsAsync(payment)).SingleOrDefault();

        Assert.IsNotNull(transaction);

        transaction.Amount = 2000;
        transaction.IsSucceed = true;
        transaction.Message = "NewMessage";
        transaction.Type = TransactionType.Verify;
        transaction.AdditionalData = "NewAdditionalData";

        await _storage.UpdateTransactionAsync(transaction);

        var newTransaction = (await _storage.GetTransactionsAsync(payment)).SingleOrDefault();

        Assert.IsNotNull(newTransaction);
        Assert.AreEqual(1, (await _storage.GetTransactionsAsync(payment)).Count);

        newTransaction.ShouldCompare(transaction);
    }

    [TestMethod]
    public async Task Delete_Transaction_Works()
    {
        await _storage.CreatePaymentAsync(PaymentTestData);

        var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

        Assert.IsNotNull(payment);

        TransactionTestData.PaymentId = payment.Id;

        await _storage.CreateTransactionAsync(TransactionTestData);

        var transaction = (await _storage.GetTransactionsAsync(payment)).SingleOrDefault();

        await _storage.DeleteTransactionAsync(transaction);

        Assert.AreEqual(0, (await _storage.GetTransactionsAsync(payment)).Count);
    }

    [TestMethod]
    public async Task Paid_Payment_Lookup_Finds_Only_Paid_Payments_Of_The_Same_Gateway_Except_The_Given_One()
    {
        await _storage.CreatePaymentAsync(NewPayment(1, "RRN-1", "gateway", isPaid: true));
        await _storage.CreatePaymentAsync(NewPayment(2, "RRN-2", "gateway", isPaid: false));
        await _storage.CreatePaymentAsync(NewPayment(3, "RRN-3", "other", isPaid: true));

        var found = await _storage.FindPaidPaymentByBankReferenceAsync("gateway", "RRN-1", exceptTrackingNumber: 99);

        Assert.AreEqual(1, found?.TrackingNumber);
        Assert.IsNull(await _storage.FindPaidPaymentByBankReferenceAsync("gateway", "RRN-1", exceptTrackingNumber: 1), "The excluded payment was returned.");
        Assert.IsNull(await _storage.FindPaidPaymentByBankReferenceAsync("gateway", "RRN-2", exceptTrackingNumber: 99), "An unpaid payment was returned.");
        Assert.IsNull(await _storage.FindPaidPaymentByBankReferenceAsync("gateway", "RRN-3", exceptTrackingNumber: 99), "A payment of another gateway was returned.");
    }

    [TestMethod]
    public async Task Paid_Payment_Lookup_Compares_The_Transaction_Code_Of_Payments_Without_A_Bank_Reference()
    {
        // A payment stored before Parbad 4.0 has no bank reference.
        var legacy = NewPayment(1, "RRN-1", "gateway", isPaid: true);
        legacy.BankReference = null;
        await _storage.CreatePaymentAsync(legacy);

        Assert.AreEqual(1, (await _storage.FindPaidPaymentByBankReferenceAsync("gateway", "RRN-1", exceptTrackingNumber: 99))?.TrackingNumber);
    }

    [TestMethod]
    public async Task Status_RefundedAmount_And_BankReference_Are_Stored()
    {
        var payment = NewPayment(1, "RRN-1", "gateway", isPaid: true);
        payment.Status = PaymentStatus.PartiallyRefunded;
        payment.RefundedAmount = 400;
        await _storage.CreatePaymentAsync(payment);

        var stored = await _storage.GetPaymentByTrackingNumberAsync(1);

        Assert.AreEqual(PaymentStatus.PartiallyRefunded, stored!.Status);
        Assert.AreEqual(400, stored.RefundedAmount);
        Assert.AreEqual("RRN-1", stored.BankReference);
        Assert.IsTrue(stored.IsPaid && stored.IsCompleted);
    }

    private static Payment NewPayment(long trackingNumber, string transactionCode, string gatewayName, bool isPaid) => new()
        {
            TrackingNumber = trackingNumber,
            Amount = 1000,
            Token = $"token-{trackingNumber}",
            TransactionCode = transactionCode,
            GatewayName = gatewayName,
            GatewayAccountName = "test",
            BankReference = isPaid ? transactionCode : null,
            Status = isPaid ? PaymentStatus.Paid : PaymentStatus.Failed
        };
}