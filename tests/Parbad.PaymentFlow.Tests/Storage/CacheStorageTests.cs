using Parbad.PaymentFlow.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.PaymentFlow.Tests.Storage;

/// <summary>
/// Behaviour that the cache storages must share with a database: updates persist every field, stored records
/// cannot be changed through object references, tracking numbers and tokens are unique, and concurrent use
/// loses nothing.
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

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        ConfigureServices(services);

        Services = services.BuildServiceProvider();
    }

    [TearDown]
    public async Task Cleanup() => await Services.DisposeAsync();

    [Test]
    public async Task Updating_a_payment_from_a_detached_copy_persists_every_field()
    {
        var storage = CreateStorage();
        var created = NewPayment(1001);
        await storage.CreatePaymentAsync(created);

        // Only Token, TrackingNumber and TransactionCode used to be copied.
        var detached = new Payment
        {
            Id = created.Id,
            TrackingNumber = 1002,
            Amount = 2500,
            Token = "new-token",
            TransactionCode = "new-code",
            GatewayName = "new-gateway",
            GatewayAccountName = "new-account",
            Status = PaymentStatus.Paid
        };

        await storage.UpdatePaymentAsync(detached);

        var stored = await CreateStorage().GetPaymentByTrackingNumberAsync(1002);

        AssertSamePayment(detached, stored);
    }

    [Test]
    public async Task Updating_a_transaction_from_a_detached_copy_persists_every_field()
    {
        var storage = CreateStorage();
        var payment = NewPayment(1001);
        await storage.CreatePaymentAsync(payment);
        var created = NewTransaction(payment.Id);
        await storage.CreateTransactionAsync(created);

        // Only IsSucceed used to be copied.
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

    [Test]
    public async Task Created_records_get_an_id_and_are_not_changed_by_mutating_the_original_objects()
    {
        var storage = CreateStorage();
        var payment = NewPayment(1001);
        await storage.CreatePaymentAsync(payment);
        var transaction = NewTransaction(payment.Id);
        await storage.CreateTransactionAsync(transaction);

        Assert.That(payment.Id, Is.Not.EqualTo(0), "The created payment did not get an id.");
        Assert.That(transaction.Id, Is.Not.EqualTo(0), "The created transaction did not get an id.");

        var expectedPayment = Copy(payment);
        var expectedTransaction = Copy(transaction);

        payment.Status = PaymentStatus.Paid;
        payment.Amount = 1;
        transaction.IsSucceed = true;
        transaction.Message = "changed";

        AssertSamePayment(expectedPayment, await storage.GetPaymentByTrackingNumberAsync(1001));
        AssertSameTransaction(expectedTransaction, (await storage.GetTransactionsAsync(expectedPayment)).Single());
    }

    [Test]
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
        Mutate((await storage.GetTransactionsAsync(payment)).Single());

        AssertSamePayment(expectedPayment, await storage.GetPaymentByTrackingNumberAsync(1001));
        AssertSamePayment(expectedPayment, await storage.GetPaymentByTokenAsync(payment.Token));
        AssertSameTransaction(expectedTransaction, (await storage.GetTransactionsAsync(payment)).Single());

        static void Mutate(object record)
        {
            switch (record)
            {
                case Payment p:
                    p.Status = PaymentStatus.Paid;
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

    [Test]
    public async Task Creating_a_payment_with_an_existing_tracking_number_is_rejected()
    {
        var storage = CreateStorage();
        await storage.CreatePaymentAsync(NewPayment(1001));

        var duplicate = NewPayment(1001);
        duplicate.Token = "other-token";

        var exception = await Assert.CatchAsync<InvalidOperationException>(() => storage.CreatePaymentAsync(duplicate));

        Assert.That(exception.Message, Does.Contain("tracking number 1001"));
        Assert.That(await CreateStorage().DoesPaymentExistAsync(duplicate.Token), Is.False);
        Assert.That(await CreateStorage().GetPaymentByTrackingNumberAsync(1001), Is.Not.Null);
    }

    [Test]
    public async Task Creating_a_payment_with_an_existing_token_is_rejected()
    {
        var storage = CreateStorage();
        var original = NewPayment(1001);
        await storage.CreatePaymentAsync(original);

        var duplicate = NewPayment(1002);
        duplicate.Token = original.Token;

        var exception = await Assert.CatchAsync<InvalidOperationException>(() => storage.CreatePaymentAsync(duplicate));

        Assert.That(exception.Message, Does.Contain("token"));
        Assert.That(await CreateStorage().DoesPaymentExistAsync(duplicate.TrackingNumber), Is.False);
        Assert.That(await CreateStorage().GetPaymentByTokenAsync(original.Token), Is.Not.Null);
    }

    [Test]
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

        await Assert.CatchAsync<InvalidOperationException>(() => storage.UpdatePaymentAsync(sameTrackingNumber));
        await Assert.CatchAsync<InvalidOperationException>(() => storage.UpdatePaymentAsync(sameToken));

        // A payment may keep its own tracking number and token.
        var unchangedKeys = Copy(second);
        unchangedKeys.Status = PaymentStatus.Paid;
        await storage.UpdatePaymentAsync(unchangedKeys);

        AssertSamePayment(first, await CreateStorage().GetPaymentByTrackingNumberAsync(first.TrackingNumber));
        AssertSamePayment(unchangedKeys, await CreateStorage().GetPaymentByTrackingNumberAsync(second.TrackingNumber));
    }

    [Test]
    public async Task Parallel_creates_on_one_storage_are_all_stored_with_unique_ids()
    {
        var storage = CreateStorage();
        var payments = Enumerable.Range(1, ParallelCreateCount).Select(i => NewPayment(1000 + i)).ToList();

        // Concurrent writes and reads on a plain List<T> lost most of the records.
        await Task.WhenAll(payments.Select(payment => Task.Run(async () =>
        {
            await storage.CreatePaymentAsync(payment);
            await storage.CreateTransactionAsync(NewTransaction(payment.Id));
            Assert.That(await storage.GetPaymentByTrackingNumberAsync(payment.TrackingNumber), Is.Not.Null);
        })));

        await AssertAllStoredAsync(CreateStorage(), payments);
    }

    [Test]
    public async Task Paid_payment_lookup_finds_only_paid_payments_of_the_same_gateway_except_the_given_one()
    {
        var storage = CreateStorage();
        var lookup = storage;

        var paid = NewPayment(1);
        paid.TransactionCode = "RRN-1";
        paid.Status = PaymentStatus.Paid;

        var unpaid = NewPayment(2);
        unpaid.TransactionCode = "RRN-2";
        unpaid.Status = PaymentStatus.Failed;

        var otherGateway = NewPayment(3);
        otherGateway.TransactionCode = "RRN-3";
        otherGateway.GatewayName = "other";
        otherGateway.Status = PaymentStatus.Paid;

        foreach (var payment in new[] { paid, unpaid, otherGateway }) await storage.CreatePaymentAsync(payment);

        lookup = CreateStorage();

        AssertSamePayment(paid, await lookup.FindPaidPaymentByBankReferenceAsync("GATEWAY", "RRN-1", exceptTrackingNumber: 99));
        Assert.That(await lookup.FindPaidPaymentByBankReferenceAsync("gateway", "RRN-1", exceptTrackingNumber: 1), Is.Null, "The excluded payment was returned.");
        Assert.That(await lookup.FindPaidPaymentByBankReferenceAsync("gateway", "RRN-2", exceptTrackingNumber: 99), Is.Null, "An unpaid payment was returned.");
        Assert.That(await lookup.FindPaidPaymentByBankReferenceAsync("gateway", "RRN-3", exceptTrackingNumber: 99), Is.Null, "A payment of another gateway was returned.");
    }

    [Test]
    public async Task Payment_completion_keeps_a_paid_result_and_lets_a_paid_result_replace_an_unpaid_one()
    {
        var payment = NewPayment(1);
        payment.TransactionCode = null;
        await CreateStorage().CreatePaymentAsync(payment);

        IStorage Completion() => CreateStorage();

        Assert.That(await Completion().TryCompletePaymentAsync(payment.Id, isPaid: false, transactionCode: null), Is.True, "An open payment could not be completed.");
        Assert.That(await Completion().TryCompletePaymentAsync(payment.Id, isPaid: false, "RRN-2"), Is.False, "An unpaid result replaced another result.");
        Assert.That(await Completion().TryCompletePaymentAsync(payment.Id, isPaid: true, "RRN-3"), Is.True, "A paid result could not replace an unpaid one.");
        Assert.That(await Completion().TryCompletePaymentAsync(payment.Id, isPaid: true, "RRN-4"), Is.False, "A paid payment was changed.");
        Assert.That(await Completion().TryCompletePaymentAsync(payment.Id, isPaid: false, transactionCode: null), Is.False, "A paid payment was changed.");

        var stored = await CreateStorage().GetPaymentByTrackingNumberAsync(1);
        Assert.That(stored!.IsCompleted && stored.IsPaid, Is.True);
        Assert.That(stored.TransactionCode, Is.EqualTo("RRN-3"));
    }

    [Test]
    public async Task Parallel_completions_of_one_payment_on_one_storage_let_exactly_one_succeed()
    {
        var payment = NewPayment(1);
        var storage = CreateStorage();
        await storage.CreatePaymentAsync(payment);

        var completion = storage;
        var results = await Task.WhenAll(Enumerable.Range(0, 20).Select(i => Task.Run(() =>
            completion.TryCompletePaymentAsync(payment.Id, isPaid: false, $"RRN-{i}"))));

        Assert.That(results.Count(completed => completed), Is.EqualTo(1));
    }

    /// <summary>
    /// A storage instance lives for one request, and a verification takes seconds, so another instance (another
    /// request or server) may complete the payment in the meantime.
    /// </summary>
    [Test]
    public async Task Completion_sees_a_completion_made_by_another_storage_instance_in_the_meantime()
    {
        var payment = NewPayment(1);
        await CreateStorage().CreatePaymentAsync(payment);

        var thisRequest = CreateStorage();
        Assert.That(await CreateStorage().TryCompletePaymentAsync(payment.Id, isPaid: true, "RRN-OTHER"), Is.True);

        Assert.That(await thisRequest.TryCompletePaymentAsync(payment.Id, isPaid: false, transactionCode: null), Is.False,
                    "The completion was decided on records loaded before the other instance completed the payment.");
        Assert.That((await CreateStorage().GetPaymentByTrackingNumberAsync(1))!.IsPaid, Is.True);
    }

    protected static async Task AssertAllStoredAsync(IStorage storage, IReadOnlyCollection<Payment> payments)
    {
        Assert.That(payments.Select(payment => payment.Id).Distinct().Count(), Is.EqualTo(payments.Count), "Payment ids are not unique.");
        Assert.That(await storage.GetLargestTrackingNumberAsync(), Is.EqualTo(payments.Max(payment => payment.TrackingNumber)));

        var transactionIds = new List<long>();

        foreach (var payment in payments)
        {
            AssertSamePayment(payment, await storage.GetPaymentByTrackingNumberAsync(payment.TrackingNumber));

            var transactions = await storage.GetTransactionsAsync(payment);
            Assert.That(transactions.Count, Is.EqualTo(1), $"Payment {payment.TrackingNumber} has no transaction.");
            transactionIds.AddRange(transactions.Select(transaction => transaction.Id));
        }

        Assert.That(transactionIds.Distinct().Count(), Is.EqualTo(payments.Count), "Transaction ids are not unique.");
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
        Assert.That(actual, Is.Not.Null, $"Payment {expected.TrackingNumber} was not found.");
        Assert.That(actual.Id, Is.EqualTo(expected.Id), nameof(Payment.Id));
        Assert.That(actual.TrackingNumber, Is.EqualTo(expected.TrackingNumber), nameof(Payment.TrackingNumber));
        Assert.That(actual.Amount, Is.EqualTo(expected.Amount), nameof(Payment.Amount));
        Assert.That(actual.Token, Is.EqualTo(expected.Token), nameof(Payment.Token));
        Assert.That(actual.TransactionCode, Is.EqualTo(expected.TransactionCode), nameof(Payment.TransactionCode));
        Assert.That(actual.GatewayName, Is.EqualTo(expected.GatewayName), nameof(Payment.GatewayName));
        Assert.That(actual.GatewayAccountName, Is.EqualTo(expected.GatewayAccountName), nameof(Payment.GatewayAccountName));
        Assert.That(actual.IsCompleted, Is.EqualTo(expected.IsCompleted), nameof(Payment.IsCompleted));
        Assert.That(actual.IsPaid, Is.EqualTo(expected.IsPaid), nameof(Payment.IsPaid));
    }

    protected static void AssertSameTransaction(Transaction expected, Transaction? actual)
    {
        Assert.That(actual, Is.Not.Null, $"Transaction {expected.Id} was not found.");
        Assert.That(actual.Id, Is.EqualTo(expected.Id), nameof(Transaction.Id));
        Assert.That(actual.PaymentId, Is.EqualTo(expected.PaymentId), nameof(Transaction.PaymentId));
        Assert.That(actual.Amount, Is.EqualTo(expected.Amount), nameof(Transaction.Amount));
        Assert.That(actual.Type, Is.EqualTo(expected.Type), nameof(Transaction.Type));
        Assert.That(actual.IsSucceed, Is.EqualTo(expected.IsSucceed), nameof(Transaction.IsSucceed));
        Assert.That(actual.Message, Is.EqualTo(expected.Message), nameof(Transaction.Message));
        Assert.That(actual.AdditionalData, Is.EqualTo(expected.AdditionalData), nameof(Transaction.AdditionalData));
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
        BankReference = payment.BankReference,
        Status = payment.Status,
        RefundedAmount = payment.RefundedAmount
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
