using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Parbad.Storage.Abstractions.Models;
using Parbad.Storage.EntityFrameworkCore.Context;
using Parbad.Storage.EntityFrameworkCore.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace Parbad.Storage.EntityFrameworkCore.Tests;

/// <summary>
/// The storage on a relational database (SQLite in memory), for the code that runs SQL. The InMemory provider used
/// by <see cref="EntityFrameworkCoreStorageTests"/> runs no SQL.
/// </summary>
[TestFixture]
public class EntityFrameworkCoreRelationalStorageTests
{
    private SqliteConnection _connection;
    private DbContextOptions<ParbadDataContext> _contextOptions;

    [SetUp]
    public void Setup()
    {
        // The database lives as long as this connection is open.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<ParbadDataContext>().UseSqlite(_connection).Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    [TearDown]
    public void Cleanup() => _connection.Dispose();

    [Test]
    public async Task Payment_Completion_Keeps_A_Paid_Result_And_Lets_A_Paid_Result_Replace_An_Unpaid_One()
    {
        var payment = new Payment { TrackingNumber = 1, Amount = 1000, Token = "token", GatewayName = "gateway" };

        await Storage().CreatePaymentAsync(payment);

        Assert.That(await TryCompleteAsync(payment.Id, isPaid: false, transactionCode: null), Is.True, "An open payment could not be completed.");
        Assert.That(await TryCompleteAsync(payment.Id, isPaid: false, "RRN-2"), Is.False, "An unpaid result replaced another result.");
        Assert.That(await TryCompleteAsync(payment.Id, isPaid: true, "RRN-3"), Is.True, "A paid result could not replace an unpaid one.");
        Assert.That(await TryCompleteAsync(payment.Id, isPaid: true, "RRN-4"), Is.False, "A paid payment was changed.");
        Assert.That(await TryCompleteAsync(payment.Id, isPaid: false, transactionCode: null), Is.False, "A paid payment was changed.");

        await using var verifyContext = CreateContext();
        var stored = await Storage().GetPaymentByTrackingNumberAsync(1);

        Assert.That(stored.Status, Is.EqualTo(PaymentStatus.Paid));
        Assert.That(stored.TransactionCode, Is.EqualTo("RRN-3"));
        Assert.That(stored.BankReference, Is.EqualTo("RRN-3"));
        Assert.That(verifyContext.Payments.Single().IsPaid, Is.True, "is_paid is not kept in step with the status.");
        Assert.That(verifyContext.Payments.Single().UpdatedOn, Is.Not.Null);
    }

    [Test]
    public async Task Completing_An_Unknown_Payment_Throws()
    {
        await Assert.ThrowsAsync<System.InvalidOperationException>(() => TryCompleteAsync(42, isPaid: true, "RRN"));
    }

    [Test]
    public async Task A_Payment_Stored_Before_4_0_Keeps_Its_State()
    {
        // Rows written by Parbad 3.x have the new status column at its default (Requested) after the migration.
        await using (var context = CreateContext())
        {
            context.Payments.Add(new PaymentEntity
                                 {
                                     TrackingNumber = 1, Amount = 1000, Token = "token", GatewayName = "gateway", TransactionCode = "RRN",
                                     IsCompleted = true, IsPaid = true, CreatedOn = System.DateTime.UtcNow
                                 });
            await context.SaveChangesAsync();
        }

        var stored = await Storage().GetPaymentByTrackingNumberAsync(1);

        Assert.That(stored!.Status, Is.EqualTo(PaymentStatus.Paid));
        Assert.That(await TryCompleteAsync(stored.Id, isPaid: false, transactionCode: null), Is.False, "A payment paid before 4.0 was changed.");
        Assert.That((await Storage().FindPaidPaymentByBankReferenceAsync("gateway", "RRN", exceptTrackingNumber: 99))?.TrackingNumber, Is.EqualTo(1));
    }

    [Test]
    public async Task Cancelled_Payment_Is_Stored_As_Cancelled()
    {
        var payment = new Payment { TrackingNumber = 1, Amount = 1000, Token = "token", GatewayName = "gateway" };
        await Storage().CreatePaymentAsync(payment);

        Assert.That(await Storage().TryCompletePaymentAsync(payment.Id, PaymentStatus.Cancelled, transactionCode: null, bankReference: null), Is.True);

        var stored = await Storage().GetPaymentByTrackingNumberAsync(1);
        Assert.That(stored!.Status, Is.EqualTo(PaymentStatus.Cancelled));
        Assert.That(stored.IsCompleted && !stored.IsPaid, Is.True);
    }

    [Test]
    public void A_Bank_Reference_Is_Unique_Per_Gateway()
    {
        using var context = CreateContext();

        var index = context.Model.FindEntityType(typeof(PaymentEntity))!
                           .GetIndexes()
                           .SingleOrDefault(index => index.Properties.Select(property => property.Name)
                                                          .SequenceEqual(new[] { nameof(PaymentEntity.GatewayName), nameof(PaymentEntity.BankReference) }));

        Assert.That(index, Is.Not.Null);
        Assert.That(index.IsUnique, Is.True);
    }

    private async Task<bool> TryCompleteAsync(long paymentId, bool isPaid, string transactionCode)
    {
        return await Storage().TryCompletePaymentAsync(paymentId,
                                                       isPaid ? PaymentStatus.Paid : PaymentStatus.Failed,
                                                       transactionCode,
                                                       isPaid ? transactionCode : null);
    }

    private ParbadDataContext CreateContext() => new(_contextOptions);

    private EntityFrameworkCoreStorage<ParbadDataContext> Storage() => new(new TestDbContextFactory<ParbadDataContext>(CreateContext));
}