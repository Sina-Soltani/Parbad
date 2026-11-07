using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Parbad.Storage.Abstractions.Models;
using Parbad.Storage.Cache.MemoryCache;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Parbad.Storage.Cache.Tests;

[TestFixture]
public class MemoryCacheTests
{
    private ServiceProvider _services;
    private MemoryCacheStorage _storage;

    private static Payment PaymentTestData => new Payment
                                              {
                                                  TrackingNumber = 1,
                                                  Amount = 1000,
                                                  Token = "token",
                                                  TransactionCode = "test",
                                                  GatewayName = "gateway",
                                                  GatewayAccountName = "test",
                                                  Status = PaymentStatus.Requested
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

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection()
                    .AddMemoryCache()
                    .Configure<MemoryCacheStorageOptions>(_ => { })
                    .BuildServiceProvider();

        var memoryCache = _services.GetRequiredService<IMemoryCache>();
        var options = _services.GetRequiredService<IOptions<MemoryCacheStorageOptions>>();

        _storage = new MemoryCacheStorage(memoryCache, options);
    }

    [TearDown]
    public async Task Cleanup()
    {
        await _services.DisposeAsync();
    }

    [Test]
    public async Task Create_Payment_Works()
    {
        await _storage.CreatePaymentAsync(PaymentTestData);

        var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

        Assert.That(payment, Is.Not.Null);
        Assert.That(await _storage.DoesPaymentExistAsync(PaymentTestData.TrackingNumber), Is.True);

        Assert.That(payment.Id, Is.EqualTo(1));
        Assert.That(payment.TrackingNumber, Is.EqualTo(PaymentTestData.TrackingNumber));
        Assert.That(payment.Amount, Is.EqualTo(PaymentTestData.Amount));
        Assert.That(payment.TransactionCode, Is.EqualTo(PaymentTestData.TransactionCode));
        Assert.That(payment.GatewayName, Is.EqualTo(PaymentTestData.GatewayName));
        Assert.That(payment.GatewayAccountName, Is.EqualTo(PaymentTestData.GatewayAccountName));
        Assert.That(payment.Token, Is.EqualTo(PaymentTestData.Token));
        Assert.That(payment.IsPaid, Is.EqualTo(PaymentTestData.IsPaid));
        Assert.That(payment.IsCompleted, Is.EqualTo(PaymentTestData.IsCompleted));
    }

    [Test]
    public async Task Update_Payment_Works()
    {
        await _storage.CreatePaymentAsync(PaymentTestData);

        var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);
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
        Assert.That(newPayment.Id, Is.EqualTo(1));
        Assert.That(newPayment.TrackingNumber, Is.EqualTo(payment.TrackingNumber));
        Assert.That(newPayment.Amount, Is.EqualTo(payment.Amount));
        Assert.That(newPayment.TransactionCode, Is.EqualTo(payment.TransactionCode));
        Assert.That(newPayment.GatewayName, Is.EqualTo(payment.GatewayName));
        Assert.That(newPayment.GatewayAccountName, Is.EqualTo(payment.GatewayAccountName));
        Assert.That(newPayment.Token, Is.EqualTo(payment.Token));
        Assert.That(newPayment.IsPaid, Is.EqualTo(payment.IsPaid));
        Assert.That(newPayment.IsCompleted, Is.EqualTo(payment.IsCompleted));
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

        TransactionTestData.PaymentId = payment.Id;

        await _storage.CreateTransactionAsync(TransactionTestData);

        var transaction = (await _storage.GetTransactionsAsync(payment)).SingleOrDefault();

        Assert.That(transaction, Is.Not.Null);
        Assert.That((await _storage.GetTransactionsAsync(payment)).Count, Is.EqualTo(1));

        Assert.That(transaction.Id, Is.EqualTo(1));
        Assert.That(transaction.PaymentId, Is.EqualTo(payment.Id));
        Assert.That(transaction.Amount, Is.EqualTo(TransactionTestData.Amount));
        Assert.That(transaction.AdditionalData, Is.EqualTo(TransactionTestData.AdditionalData));
        Assert.That(transaction.IsSucceed, Is.EqualTo(TransactionTestData.IsSucceed));
        Assert.That(transaction.Type, Is.EqualTo(TransactionTestData.Type));
        Assert.That(transaction.Message, Is.EqualTo(TransactionTestData.Message));
    }

    [Test]
    public async Task Update_Transaction_Works()
    {
        await _storage.CreatePaymentAsync(PaymentTestData);

        var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

        TransactionTestData.PaymentId = payment.Id;

        await _storage.CreateTransactionAsync(TransactionTestData);

        var transaction = (await _storage.GetTransactionsAsync(payment)).SingleOrDefault();
        transaction.Amount = 2000;
        transaction.IsSucceed = true;
        transaction.Message = "NewMessage";
        transaction.Type = TransactionType.Verify;
        transaction.AdditionalData = "NewAdditionalData";

        await _storage.UpdateTransactionAsync(transaction);

        var newTransaction = (await _storage.GetTransactionsAsync(payment)).SingleOrDefault();

        Assert.That(newTransaction, Is.Not.Null);
        Assert.That((await _storage.GetTransactionsAsync(payment)).Count, Is.EqualTo(1));

        Assert.That(transaction.Id, Is.EqualTo(1));
        Assert.That(newTransaction.PaymentId, Is.EqualTo(transaction.Id));
        Assert.That(newTransaction.Amount, Is.EqualTo(transaction.Amount));
        Assert.That(newTransaction.AdditionalData, Is.EqualTo(transaction.AdditionalData));
        Assert.That(newTransaction.IsSucceed, Is.EqualTo(transaction.IsSucceed));
        Assert.That(newTransaction.Type, Is.EqualTo(transaction.Type));
        Assert.That(newTransaction.Message, Is.EqualTo(transaction.Message));
    }

    [Test]
    public async Task Delete_Transaction_Works()
    {
        await _storage.CreatePaymentAsync(PaymentTestData);

        var payment = await _storage.GetPaymentByTrackingNumberAsync(PaymentTestData.TrackingNumber);

        TransactionTestData.PaymentId = payment.Id;

        await _storage.CreateTransactionAsync(TransactionTestData);

        var transaction = (await _storage.GetTransactionsAsync(payment)).SingleOrDefault();

        await _storage.DeleteTransactionAsync(transaction);

        Assert.That((await _storage.GetTransactionsAsync(payment)).Count, Is.EqualTo(0));
    }
}
