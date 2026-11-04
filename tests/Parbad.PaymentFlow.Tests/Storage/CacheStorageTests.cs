using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.PaymentFlow.Tests.Storage;

/// <summary>
/// Behaviour that the cache storages must share with a database: updates persist every field, stored records
/// cannot be changed through object references, tracking numbers and tokens are unique, and concurrent use
/// loses nothing. Issue ids refer to ANALYSIS-03-STORAGE-AND-HOSTING.md (S-3 to S-5).
/// <para>
/// A subclass registers one storage. <see cref="CreateStorage"/> resolves a new (transient) instance, as each
/// request would; a record is only considered persisted when a new instance can read it.
/// </para>
/// </summary>
public abstract class CacheStorageTests
{
    protected ServiceProvider Services { get; private set; } = null!;

    /// <summary>Number of payments created in parallel by <see cref="Parallel_creates_on_one_storage_are_all_stored_with_unique_ids"/>.</summary>
    protected abstract int ParallelCreateCount { get; }

    /// <summary>Registers the storage under test with <c>AddParbad().ConfigureStorage(...)</c>.</summary>
    protected abstract void ConfigureServices(IServiceCollection services);

    protected IStorage CreateStorage() => Services.GetRequiredService<IStorage>();

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();

        ConfigureServices(services);

        Services = services.BuildServiceProvider();
    }

    [TestCleanup]
    public Task Cleanup() => Services.DisposeAsync().AsTask();

    [TestMethod]
    public async Task Updating_a_payment_from_a_detached_copy_persists_every_field()
    {
        var storage = CreateStorage();
        var created = NewPayment(1001);
        await storage.CreatePaymentAsync(created);

        // S-3: only Token, TrackingNumber and TransactionCode used to be copied.
        var detached = new Payment
        {
            Id = created.Id,
            TrackingNumber = 1002,
            Amount = 2500,
            Token = "new-token",
            TransactionCode = "new-code",
            GatewayName = "new-gateway",
            GatewayAccountName = "new-account",
            IsCompleted = true,
            IsPaid = true
        };

        await storage.UpdatePaymentAsync(detached);

        var stored = await CreateStorage().GetPaymentByTrackingNumberAsync(1002);

        AssertSamePayment(detached, stored);
    }

    [TestMethod]
    public async Task Updating_a_transaction_from_a_detached_copy_persists_every_field()
    {
        var storage = CreateStorage();
        var payment = NewPayment(1001);
        await storage.CreatePaymentAsync(payment);
        var created = NewTransaction(payment.Id);
        await storage.CreateTransactionAsync(created);

        // S-3: only IsSucceed used to be copied.
        var detached = new Transaction
        {
            Id = created.Id,
            PaymentId = payment.Id,
            Amount = 2500,
            Type = TransactionType.Verify,
            IsSucceed = true,
            Message = "new-message",
            AdditionalData = "new-data"
        };

        await storage.UpdateTransactionAsync(detached);

        var stored = (await CreateStorage().GetTransactionsAsync(payment)).Single();

        AssertSameTransaction(detached, stored);
    }

    [TestMethod]
    public async Task Created_records_get_an_id_and_are_not_changed_by_mutating_the_original_objects()
    {
        var storage = CreateStorage();
        var payment = NewPayment(1001);
        await storage.CreatePaymentAsync(payment);
        var transaction = NewTransaction(payment.Id);
        await storage.CreateTransactionAsync(transaction);

        Assert.AreNotEqual(0, payment.Id, "The created payment did not get an id.");
        Assert.AreNotEqual(0, transaction.Id, "The created transaction did not get an id.");

        var expectedPayment = Copy(payment);
        var expectedTransaction = Copy(transaction);

        payment.IsPaid = true;
        payment.Amount = 1;
        transaction.IsSucceed = true;
        transaction.Message = "changed";

        AssertSamePayment(expectedPayment, await storage.GetPaymentByTrackingNumberAsync(1001));
        AssertSameTransaction(expectedTransaction, (await storage.GetTransactionsAsync(expectedPayment)).Single());
    }

    [TestMethod]
    public async Task Mutating_records_returned_by_reads_does_not_change_stored_data()
    {
        var storage = CreateStorage();
        var payment = NewPayment(1001);
        await storage.CreatePaymentAsync(payment);
        await storage.CreateTransactionAsync(NewTransaction(payment.Id));

        var expectedPayment = Copy((await storage.GetPaymentByTrackingNumberAsync(1001))!);
        var expectedTransaction = Copy((await storage.GetTransactionsAsync(payment)).Single());

        Mutate((await storage.GetPaymentByTrackingNumberAsync(1001))!);
        Mutate((await storage.GetPaymentByTokenAsync(payment.Token))!);
        Mutate(storage.Payments.Single());
        Mutate((await storage.GetTransactionsAsync(payment)).Single());
        Mutate(storage.Transactions.Single());

        AssertSamePayment(expectedPayment, await storage.GetPaymentByTrackingNumberAsync(1001));
        AssertSamePayment(expectedPayment, await storage.GetPaymentByTokenAsync(payment.Token));
        AssertSamePayment(expectedPayment, storage.Payments.Single());
        AssertSameTransaction(expectedTransaction, (await storage.GetTransactionsAsync(payment)).Single());
        AssertSameTransaction(expectedTransaction, storage.Transactions.Single());

        static void Mutate(object record)
        {
            switch (record)
            {
                case Payment p:
                    p.IsPaid = true;
                    p.IsCompleted = true;
                    p.Amount = 1;
                    p.TrackingNumber = 9999;
                    p.Token = "mutated";
                    break;
                case Transaction t:
                    t.IsSucceed = true;
                    t.Amount = 1;
                    t.Message = "mutated";
                    break;
            }
        }
    }

    [TestMethod]
    public async Task Creating_a_payment_with_an_existing_tracking_number_is_rejected()
    {
        var storage = CreateStorage();
        await storage.CreatePaymentAsync(NewPayment(1001));

        var duplicate = NewPayment(1001);
        duplicate.Token = "other-token";

        var exception = await AssertThrowsInvalidOperationAsync(() => storage.CreatePaymentAsync(duplicate));

        StringAssert.Contains(exception.Message, "tracking number 1001");
        Assert.AreEqual(1, CreateStorage().Payments.Count());
        Assert.IsNotNull(await CreateStorage().GetPaymentByTrackingNumberAsync(1001));
    }

    [TestMethod]
    public async Task Creating_a_payment_with_an_existing_token_is_rejected()
    {
        var storage = CreateStorage();
        var original = NewPayment(1001);
        await storage.CreatePaymentAsync(original);

        var duplicate = NewPayment(1002);
        duplicate.Token = original.Token;

        var exception = await AssertThrowsInvalidOperationAsync(() => storage.CreatePaymentAsync(duplicate));

        StringAssert.Contains(exception.Message, "token");
        Assert.AreEqual(1, CreateStorage().Payments.Count());
        Assert.IsNotNull(await CreateStorage().GetPaymentByTokenAsync(original.Token));
    }

    [TestMethod]
    public async Task Updating_a_payment_to_the_tracking_number_or_token_of_another_payment_is_rejected()
    {
        var storage = CreateStorage();
        var first = NewPayment(1001);
        var second = NewPayment(1002);
        await storage.CreatePaymentAsync(first);
        await storage.CreatePaymentAsync(second);

        var sameTrackingNumber = Copy(second);
        sameTrackingNumber.TrackingNumber = first.TrackingNumber;

        var sameToken = Copy(second);
        sameToken.Token = first.Token;

        await AssertThrowsInvalidOperationAsync(() => storage.UpdatePaymentAsync(sameTrackingNumber));
        await AssertThrowsInvalidOperationAsync(() => storage.UpdatePaymentAsync(sameToken));

        // A payment may keep its own tracking number and token.
        var unchangedKeys = Copy(second);
        unchangedKeys.IsPaid = true;
        await storage.UpdatePaymentAsync(unchangedKeys);

        AssertSamePayment(first, await CreateStorage().GetPaymentByTrackingNumberAsync(first.TrackingNumber));
        AssertSamePayment(unchangedKeys, await CreateStorage().GetPaymentByTrackingNumberAsync(second.TrackingNumber));
    }

    [TestMethod]
    public async Task Parallel_creates_on_one_storage_are_all_stored_with_unique_ids()
    {
        var storage = CreateStorage();
        var payments = Enumerable.Range(1, ParallelCreateCount).Select(i => NewPayment(1000 + i)).ToList();

        // S-4: concurrent writes and reads on a plain List<T> lost most of the records.
        await Task.WhenAll(payments.Select(payment => Task.Run(async () =>
        {
            await storage.CreatePaymentAsync(payment);
            await storage.CreateTransactionAsync(NewTransaction(payment.Id));
            Assert.IsNotNull(await storage.GetPaymentByTrackingNumberAsync(payment.TrackingNumber));
        })));

        await AssertAllStoredAsync(CreateStorage(), payments);
    }

    [TestMethod]
    public async Task Paid_payment_lookup_finds_only_paid_payments_of_the_same_gateway_except_the_given_one()
    {
        var storage = CreateStorage();
        var lookup = storage as IPaidPaymentLookup;
        Assert.IsNotNull(lookup, $"{storage.GetType().Name} does not implement {nameof(IPaidPaymentLookup)}.");

        var paid = NewPayment(1);
        paid.TransactionCode = "RRN-1";
        paid.IsCompleted = paid.IsPaid = true;

        var unpaid = NewPayment(2);
        unpaid.TransactionCode = "RRN-2";
        unpaid.IsCompleted = true;

        var otherGateway = NewPayment(3);
        otherGateway.TransactionCode = "RRN-3";
        otherGateway.GatewayName = "other";
        otherGateway.IsCompleted = otherGateway.IsPaid = true;

        foreach (var payment in new[] { paid, unpaid, otherGateway }) await storage.CreatePaymentAsync(payment);

        lookup = (IPaidPaymentLookup)CreateStorage();

        AssertSamePayment(paid, await lookup.FindPaidPaymentByTransactionCodeAsync("GATEWAY", "RRN-1", exceptTrackingNumber: 99));
        Assert.IsNull(await lookup.FindPaidPaymentByTransactionCodeAsync("gateway", "RRN-1", exceptTrackingNumber: 1), "The excluded payment was returned.");
        Assert.IsNull(await lookup.FindPaidPaymentByTransactionCodeAsync("gateway", "RRN-2", exceptTrackingNumber: 99), "An unpaid payment was returned.");
        Assert.IsNull(await lookup.FindPaidPaymentByTransactionCodeAsync("gateway", "RRN-3", exceptTrackingNumber: 99), "A payment of another gateway was returned.");
    }

    protected static async Task AssertAllStoredAsync(IStorage storage, IReadOnlyCollection<Payment> payments)
    {
        Assert.AreEqual(payments.Count, payments.Select(payment => payment.Id).Distinct().Count(), "Payment ids are not unique.");
        Assert.AreEqual(payments.Count, storage.Payments.Count());

        var transactionIds = storage.Transactions.Select(transaction => transaction.Id).ToList();
        Assert.AreEqual(payments.Count, transactionIds.Distinct().Count(), "Transaction ids are not unique, or transactions are missing.");

        foreach (var payment in payments)
        {
            AssertSamePayment(payment, await storage.GetPaymentByTrackingNumberAsync(payment.TrackingNumber));
            Assert.AreEqual(1, (await storage.GetTransactionsAsync(payment)).Count, $"Payment {payment.TrackingNumber} has no transaction.");
        }
    }

    protected static Payment NewPayment(long trackingNumber) => new()
    {
        TrackingNumber = trackingNumber,
        Amount = 1000,
        Token = $"token-{trackingNumber}",
        TransactionCode = "code",
        GatewayName = "gateway",
        GatewayAccountName = "account"
    };

    protected static Transaction NewTransaction(long paymentId) => new()
    {
        PaymentId = paymentId,
        Amount = 1000,
        Type = TransactionType.Request,
        IsSucceed = false,
        Message = "message",
        AdditionalData = "data"
    };

    protected static void AssertSamePayment(Payment expected, Payment? actual)
    {
        Assert.IsNotNull(actual, $"Payment {expected.TrackingNumber} was not found.");
        Assert.AreEqual(expected.Id, actual.Id, nameof(Payment.Id));
        Assert.AreEqual(expected.TrackingNumber, actual.TrackingNumber, nameof(Payment.TrackingNumber));
        Assert.AreEqual(expected.Amount, actual.Amount, nameof(Payment.Amount));
        Assert.AreEqual(expected.Token, actual.Token, nameof(Payment.Token));
        Assert.AreEqual(expected.TransactionCode, actual.TransactionCode, nameof(Payment.TransactionCode));
        Assert.AreEqual(expected.GatewayName, actual.GatewayName, nameof(Payment.GatewayName));
        Assert.AreEqual(expected.GatewayAccountName, actual.GatewayAccountName, nameof(Payment.GatewayAccountName));
        Assert.AreEqual(expected.IsCompleted, actual.IsCompleted, nameof(Payment.IsCompleted));
        Assert.AreEqual(expected.IsPaid, actual.IsPaid, nameof(Payment.IsPaid));
    }

    protected static void AssertSameTransaction(Transaction expected, Transaction? actual)
    {
        Assert.IsNotNull(actual, $"Transaction {expected.Id} was not found.");
        Assert.AreEqual(expected.Id, actual.Id, nameof(Transaction.Id));
        Assert.AreEqual(expected.PaymentId, actual.PaymentId, nameof(Transaction.PaymentId));
        Assert.AreEqual(expected.Amount, actual.Amount, nameof(Transaction.Amount));
        Assert.AreEqual(expected.Type, actual.Type, nameof(Transaction.Type));
        Assert.AreEqual(expected.IsSucceed, actual.IsSucceed, nameof(Transaction.IsSucceed));
        Assert.AreEqual(expected.Message, actual.Message, nameof(Transaction.Message));
        Assert.AreEqual(expected.AdditionalData, actual.AdditionalData, nameof(Transaction.AdditionalData));
    }

    // Assert.ThrowsExceptionAsync is removed in MSTest 4.
    protected static async Task<InvalidOperationException> AssertThrowsInvalidOperationAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (InvalidOperationException exception)
        {
            return exception;
        }

        Assert.Fail("Expected an InvalidOperationException.");

        return null!;
    }

    private static Payment Copy(Payment payment) => new()
    {
        Id = payment.Id,
        TrackingNumber = payment.TrackingNumber,
        Amount = payment.Amount,
        Token = payment.Token,
        TransactionCode = payment.TransactionCode,
        GatewayName = payment.GatewayName,
        GatewayAccountName = payment.GatewayAccountName,
        IsCompleted = payment.IsCompleted,
        IsPaid = payment.IsPaid
    };

    private static Transaction Copy(Transaction transaction) => new()
    {
        Id = transaction.Id,
        PaymentId = transaction.PaymentId,
        Amount = transaction.Amount,
        Type = transaction.Type,
        IsSucceed = transaction.IsSucceed,
        Message = transaction.Message,
        AdditionalData = transaction.AdditionalData
    };
}
