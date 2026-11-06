using KellermanSoftware.CompareNetObjects;
using Microsoft.EntityFrameworkCore;
using Parbad.Storage.Abstractions.Models;
using Parbad.Storage.EntityFrameworkCore.Context;
using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Parbad.Storage.EntityFrameworkCore.Tests;

[TestFixture]
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

    [SetUp]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<ParbadDataContext>()
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .Options;

        _storage = new EntityFrameworkCoreStorage<ParbadDataContext>(new TestDbContextFactory<ParbadDataContext>(() => new ParbadDataContext(contextOptions)));
    }

    [Test]
    public async Task Largest_tracking_number_is_null_without_payments_and_the_maximum_otherwise()
    {
        Assert.That(await _storage.GetLargestTrackingNumberAsync(), Is.Null);

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

        Assert.That(await _storage.GetLargestTrackingNumberAsync(), Is.EqualTo(42));
    }

    [Test]
    public async Task Create_Payment_Works()
    {
        await _storage.CreatePaymentAsync(PaymentTestData);

        var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

        Assert.That(payment, Is.Not.Null);
        Assert.That(await _storage.DoesPaymentExistAsync(PaymentTestData.TrackingNumber), Is.True);

        payment.ShouldCompare(PaymentTestData);
    }

    [Test]
    public async Task Update_Payment_Works()
    {
        await _storage.CreatePaymentAsync(PaymentTestData);

        var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

        Assert.That(payment, Is.Not.Null);

        payment.TrackingNumber = 2;
        payment.Amount = 2000;
        payment.Token = "NewToken";
        payment.TransactionCode = "NewCode";
        payment.GatewayName = "NewGateway";
        payment.GatewayAccountName = "NewAccount";
        payment.Status = PaymentStatus.Paid;

        await _storage.UpdatePaymentAsync(payment);

        var newPayment = await _storage.GetPaymentByTrackingNumberAsync(payment.TrackingNumber);

        Assert.That(newPayment, Is.Not.Null);
        newPayment.ShouldCompare(payment);
    }

    [Test]
    public async Task Delete_Payment_Works()
    {
        await _storage.CreatePaymentAsync(PaymentTestData);

        var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

        await _storage.DeletePaymentAsync(payment);

        Assert.That(await _storage.DoesPaymentExistAsync(PaymentTestData.TrackingNumber), Is.False);
    }

    [Test]
    public async Task Create_Transaction_Works()
    {
        await _storage.CreatePaymentAsync(PaymentTestData);

        var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

        Assert.That(payment, Is.Not.Null);

        TransactionTestData.PaymentId = payment.Id;

        await _storage.CreateTransactionAsync(TransactionTestData);

        var transaction = (await _storage.GetTransactionsAsync(payment)).SingleOrDefault();

        Assert.That(transaction, Is.Not.Null);
        Assert.That((await _storage.GetTransactionsAsync(payment)).Count, Is.EqualTo(1));

        transaction.ShouldCompare(TransactionTestData);
    }

    [Test]
    public async Task Update_Transaction_Works()
    {
        await _storage.CreatePaymentAsync(PaymentTestData);

        var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

        Assert.That(payment, Is.Not.Null);

        TransactionTestData.PaymentId = payment.Id;

        await _storage.CreateTransactionAsync(TransactionTestData);

        var transaction = (await _storage.GetTransactionsAsync(payment)).SingleOrDefault();

        Assert.That(transaction, Is.Not.Null);

        transaction.Amount = 2000;
        transaction.IsSucceed = true;
        transaction.Message = "NewMessage";
        transaction.Type = TransactionType.Verify;
        transaction.AdditionalData = "NewAdditionalData";

        await _storage.UpdateTransactionAsync(transaction);

        var newTransaction = (await _storage.GetTransactionsAsync(payment)).SingleOrDefault();

        Assert.That(newTransaction, Is.Not.Null);
        Assert.That((await _storage.GetTransactionsAsync(payment)).Count, Is.EqualTo(1));

        newTransaction.ShouldCompare(transaction);
    }

    [Test]
    public async Task Delete_Transaction_Works()
    {
        await _storage.CreatePaymentAsync(PaymentTestData);

        var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

        Assert.That(payment, Is.Not.Null);

        TransactionTestData.PaymentId = payment.Id;

        await _storage.CreateTransactionAsync(TransactionTestData);

        var transaction = (await _storage.GetTransactionsAsync(payment)).SingleOrDefault();

        await _storage.DeleteTransactionAsync(transaction);

        Assert.That((await _storage.GetTransactionsAsync(payment)).Count, Is.EqualTo(0));
    }

    [Test]
    public async Task Paid_Payment_Lookup_Finds_Only_Paid_Payments_Of_The_Same_Gateway_Except_The_Given_One()
    {
        await _storage.CreatePaymentAsync(NewPayment(1, "RRN-1", "gateway", isPaid: true));
        await _storage.CreatePaymentAsync(NewPayment(2, "RRN-2", "gateway", isPaid: false));
        await _storage.CreatePaymentAsync(NewPayment(3, "RRN-3", "other", isPaid: true));

        var found = await _storage.FindPaidPaymentByBankReferenceAsync("gateway", "RRN-1", exceptTrackingNumber: 99);

        Assert.That(found?.TrackingNumber, Is.EqualTo(1));
        Assert.That(await _storage.FindPaidPaymentByBankReferenceAsync("gateway", "RRN-1", exceptTrackingNumber: 1), Is.Null, "The excluded payment was returned.");
        Assert.That(await _storage.FindPaidPaymentByBankReferenceAsync("gateway", "RRN-2", exceptTrackingNumber: 99), Is.Null, "An unpaid payment was returned.");
        Assert.That(await _storage.FindPaidPaymentByBankReferenceAsync("gateway", "RRN-3", exceptTrackingNumber: 99), Is.Null, "A payment of another gateway was returned.");
    }

    [Test]
    public async Task Paid_Payment_Lookup_Compares_The_Transaction_Code_Of_Payments_Without_A_Bank_Reference()
    {
        // A payment stored before Parbad 4.0 has no bank reference.
        var legacy = NewPayment(1, "RRN-1", "gateway", isPaid: true);
        legacy.BankReference = null;
        await _storage.CreatePaymentAsync(legacy);

        Assert.That((await _storage.FindPaidPaymentByBankReferenceAsync("gateway", "RRN-1", exceptTrackingNumber: 99))?.TrackingNumber, Is.EqualTo(1));
    }

    [Test]
    public async Task Status_RefundedAmount_And_BankReference_Are_Stored()
    {
        var payment = NewPayment(1, "RRN-1", "gateway", isPaid: true);
        payment.Status = PaymentStatus.PartiallyRefunded;
        payment.RefundedAmount = 400;
        await _storage.CreatePaymentAsync(payment);

        var stored = await _storage.GetPaymentByTrackingNumberAsync(1);

        Assert.That(stored!.Status, Is.EqualTo(PaymentStatus.PartiallyRefunded));
        Assert.That(stored.RefundedAmount, Is.EqualTo(400));
        Assert.That(stored.BankReference, Is.EqualTo("RRN-1"));
        Assert.That(stored.IsPaid && stored.IsCompleted, Is.True);
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