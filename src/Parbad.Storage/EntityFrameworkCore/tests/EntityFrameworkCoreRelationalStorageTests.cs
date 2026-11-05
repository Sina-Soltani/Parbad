using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Storage.Abstractions.Models;
using Parbad.Storage.EntityFrameworkCore.Context;
using Parbad.Storage.EntityFrameworkCore.Options;
using System.Linq;
using System.Threading.Tasks;

namespace Parbad.Storage.EntityFrameworkCore.Tests
{
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

            await using (var context = CreateContext())
            {
                await new EntityFrameworkCoreStorage(context).CreatePaymentAsync(payment);
            }

            Assert.IsTrue(await TryCompleteAsync(payment.Id, isPaid: false, transactionCode: null), "An open payment could not be completed.");
            Assert.IsFalse(await TryCompleteAsync(payment.Id, isPaid: false, "RRN-2"), "An unpaid result replaced another result.");
            Assert.IsTrue(await TryCompleteAsync(payment.Id, isPaid: true, "RRN-3"), "A paid result could not replace an unpaid one.");
            Assert.IsFalse(await TryCompleteAsync(payment.Id, isPaid: true, "RRN-4"), "A paid payment was changed.");
            Assert.IsFalse(await TryCompleteAsync(payment.Id, isPaid: false, transactionCode: null), "A paid payment was changed.");

            await using var verifyContext = CreateContext();
            var stored = await new EntityFrameworkCoreStorage(verifyContext).GetPaymentByTrackingNumberAsync(1);

            Assert.IsTrue(stored.IsCompleted && stored.IsPaid);
            Assert.AreEqual("RRN-3", stored.TransactionCode);
            Assert.IsNotNull(verifyContext.Payments.Single().UpdatedOn);
        }

        [TestMethod]
        public async Task Completing_An_Unknown_Payment_Throws()
        {
            await Assert.ThrowsExceptionAsync<System.InvalidOperationException>(() => TryCompleteAsync(42, isPaid: true, "RRN"));
        }

        private async Task<bool> TryCompleteAsync(long paymentId, bool isPaid, string transactionCode)
        {
            // A new context each time, as each request would have.
            await using var context = CreateContext();

            return await new EntityFrameworkCoreStorage(context).TryCompletePaymentAsync(paymentId, isPaid, transactionCode);
        }

        private ParbadDataContext CreateContext() =>
            new(_contextOptions, new OptionsWrapper<EntityFrameworkCoreOptions>(new EntityFrameworkCoreOptions()));
    }
}
