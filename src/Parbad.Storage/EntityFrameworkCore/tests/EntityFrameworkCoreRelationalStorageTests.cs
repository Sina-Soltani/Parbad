using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
[TestClass]
public class EntityFrameworkCoreRelationalStorageTests
{
    private SqliteConnection _connection;
    private DbContextOptions<ParbadDataContext> _contextOptions;

    [TestInitialize]
    public void Setup()
    {
        // The database lives as long as this connection is open.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<ParbadDataContext>().UseSqlite(_connection).Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    [TestCleanup]
    public void Cleanup() => _connection.Dispose();

    [TestMethod]
    public async Task Payment_Completion_Keeps_A_Paid_Result_And_Lets_A_Paid_Result_Replace_An_Unpaid_One()
    {
        var payment = new Payment { TrackingNumber = 1, Amount = 1000, Token = "token", GatewayName = "gateway" };

        await Storage().CreatePaymentAsync(payment);

        Assert.IsTrue(await TryCompleteAsync(payment.Id, isPaid: false, transactionCode: null), "An open payment could not be completed.");
        Assert.IsFalse(await TryCompleteAsync(payment.Id, isPaid: false, "RRN-2"), "An unpaid result replaced another result.");
        Assert.IsTrue(await TryCompleteAsync(payment.Id, isPaid: true, "RRN-3"), "A paid result could not replace an unpaid one.");
        Assert.IsFalse(await TryCompleteAsync(payment.Id, isPaid: true, "RRN-4"), "A paid payment was changed.");
        Assert.IsFalse(await TryCompleteAsync(payment.Id, isPaid: false, transactionCode: null), "A paid payment was changed.");

        await using var verifyContext = CreateContext();
        var stored = await Storage().GetPaymentByTrackingNumberAsync(1);

        Assert.AreEqual(PaymentStatus.Paid, stored.Status);
        Assert.AreEqual("RRN-3", stored.TransactionCode);
        Assert.AreEqual("RRN-3", stored.BankReference);
        Assert.IsTrue(verifyContext.Payments.Single().IsPaid, "is_paid is not kept in step with the status.");
        Assert.IsNotNull(verifyContext.Payments.Single().UpdatedOn);
    }

    [TestMethod]
    public async Task Completing_An_Unknown_Payment_Throws()
    {
        await Assert.ThrowsExceptionAsync<System.InvalidOperationException>(() => TryCompleteAsync(42, isPaid: true, "RRN"));
    }

    [TestMethod]
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

        Assert.AreEqual(PaymentStatus.Paid, stored!.Status);
        Assert.IsFalse(await TryCompleteAsync(stored.Id, isPaid: false, transactionCode: null), "A payment paid before 4.0 was changed.");
        Assert.AreEqual(1, (await Storage().FindPaidPaymentByBankReferenceAsync("gateway", "RRN", exceptTrackingNumber: 99))?.TrackingNumber);
    }

    [TestMethod]
    public async Task Cancelled_Payment_Is_Stored_As_Cancelled()
    {
        var payment = new Payment { TrackingNumber = 1, Amount = 1000, Token = "token", GatewayName = "gateway" };
        await Storage().CreatePaymentAsync(payment);

        Assert.IsTrue(await Storage().TryCompletePaymentAsync(payment.Id, PaymentStatus.Cancelled, transactionCode: null, bankReference: null));

        var stored = await Storage().GetPaymentByTrackingNumberAsync(1);
        Assert.AreEqual(PaymentStatus.Cancelled, stored!.Status);
        Assert.IsTrue(stored.IsCompleted && !stored.IsPaid);
    }

    [TestMethod]
    public void A_Bank_Reference_Is_Unique_Per_Gateway()
    {
        using var context = CreateContext();

        var index = context.Model.FindEntityType(typeof(PaymentEntity))!
                           .GetIndexes()
                           .SingleOrDefault(index => index.Properties.Select(property => property.Name)
                                                          .SequenceEqual(new[] { nameof(PaymentEntity.GatewayName), nameof(PaymentEntity.BankReference) }));

        Assert.IsNotNull(index);
        Assert.IsTrue(index.IsUnique);
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