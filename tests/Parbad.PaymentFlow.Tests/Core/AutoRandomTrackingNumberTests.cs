using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;
using Parbad.Abstraction;
using Parbad.Builder;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Abstractions.Models;
using Parbad.TrackingNumberProviders;

namespace Parbad.PaymentFlow.Tests.Core;

/// <summary>
/// Tests for <see cref="AutoRandomTrackingNumber"/>.
/// The storages used here throw from the obsolete <see cref="IStorage.Payments"/>, so every test also proves
/// that the formatter no longer loads all stored payments.
/// </summary>
[TestFixture]
public class AutoRandomTrackingNumberTests
{
    private const int MaxAttempts = 10;

    [TestCase(long.MaxValue - 1000)]
    [TestCase(long.MaxValue / 2)] // About half of the random samples are rejected for this range.
    [TestCase(long.MaxValue)]
    [TestCase(1000L)] // The default minimum.
    public async Task Generated_numbers_are_at_least_the_minimum_value(long minimumValue)
    {
        var formatter = CreateFormatter(new CheckingStorage(), minimumValue);

        for (var i = 0; i < 2000; i++)
        {
            var invoice = new Invoice();

            await formatter.FormatAsync(invoice);

            Assert.That(invoice.TrackingNumber >= minimumValue, Is.True, $"{invoice.TrackingNumber} is below the minimum {minimumValue}.");
        }
    }

    [TestCase(-1L)]
    [TestCase(long.MinValue)]
    public async Task Generated_numbers_are_not_negative_when_the_minimum_is_negative(long minimumValue)
    {
        var formatter = CreateFormatter(new CheckingStorage(), minimumValue);

        for (var i = 0; i < 2000; i++)
        {
            var invoice = new Invoice();

            await formatter.FormatAsync(invoice);

            Assert.That(invoice.TrackingNumber >= 0, Is.True, $"{invoice.TrackingNumber} is negative.");
        }
    }

    [Test]
    public async Task Generated_numbers_are_random()
    {
        var formatter = CreateFormatter(new CheckingStorage(), AutoRandomTrackingNumberOptions.DefaultMinimumNumber);

        var numbers = new HashSet<long>();

        for (var i = 0; i < 100; i++)
        {
            var invoice = new Invoice();

            await formatter.FormatAsync(invoice);

            numbers.Add(invoice.TrackingNumber);
        }

        Assert.That(numbers.Count, Is.EqualTo(100), "Two of 100 random 63-bit numbers are equal.");
    }

    [Test]
    public async Task Numbers_that_already_exist_are_skipped()
    {
        const int existingCount = 3;
        var storage = new CheckingStorage { Exists = (_, call) => call <= existingCount };
        var formatter = CreateFormatter(storage, AutoRandomTrackingNumberOptions.DefaultMinimumNumber);
        var invoice = new Invoice();

        await formatter.FormatAsync(invoice);

        Assert.That(storage.CheckedNumbers.Count, Is.EqualTo(existingCount + 1));
        Assert.That(invoice.TrackingNumber, Is.EqualTo(storage.CheckedNumbers.Last()));
        Assert.That(storage.CheckedNumbers.Take(existingCount).ToList(), Has.No.Member(invoice.TrackingNumber));
    }

    [Test]
    public async Task A_clear_exception_is_thrown_when_no_unused_number_is_found()
    {
        var storage = new CheckingStorage { Exists = (_, _) => true };
        var formatter = CreateFormatter(storage, long.MaxValue - 5);

        var exception = await Assert.CatchAsync<InvalidOperationException>(() => formatter.FormatAsync(new Invoice()));

        Assert.That(storage.CheckedNumbers.Count, Is.EqualTo(MaxAttempts));
        Assert.That(exception.Message, Does.Contain("No unused tracking number was found"));
        Assert.That(exception.Message, Does.Contain(nameof(AutoRandomTrackingNumberOptions.MinimumValue)));
    }

    [Test]
    public async Task A_cancelled_token_throws_instead_of_returning_a_number()
    {
        var storage = new CheckingStorage();
        var formatter = CreateFormatter(storage, AutoRandomTrackingNumberOptions.DefaultMinimumNumber);
        var invoice = new Invoice();

        await Assert.CatchAsync<OperationCanceledException>(() => formatter.FormatAsync(invoice, new CancellationToken(canceled: true)));

        Assert.That(storage.CheckedNumbers.Count, Is.EqualTo(0));
        Assert.That(invoice.TrackingNumber, Is.EqualTo(0));
    }

    [Test]
    public async Task Cancellation_between_attempts_throws_instead_of_returning_a_number()
    {
        using var cancellation = new CancellationTokenSource();
        var storage = new CheckingStorage
        {
            Exists = (_, _) =>
            {
                cancellation.Cancel();
                return true;
            }
        };
        var formatter = CreateFormatter(storage, AutoRandomTrackingNumberOptions.DefaultMinimumNumber);
        var invoice = new Invoice();

        await Assert.CatchAsync<OperationCanceledException>(() => formatter.FormatAsync(invoice, cancellation.Token));

        Assert.That(storage.CheckedNumbers.Count, Is.EqualTo(1));
        Assert.That(invoice.TrackingNumber, Is.EqualTo(0));
    }

    [Test]
    public async Task Requesting_with_UseAutoRandomTrackingNumber_stores_payments_with_random_numbers()
    {
        const long minimumValue = long.MaxValue - 1_000_000;

        await using var host = ParbadTestHost.Create(
            gateways => gateways
                        .AddGateway<FakeGateway>()
                        .WithAccounts<FakeGatewayAccount>(accounts => accounts.AddInMemory(_ => { })),
            services =>
            {
                services.AddSingleton(new FakeGatewayBehaviour());
                services.Configure<AutoRandomTrackingNumberOptions>(options => options.MinimumValue = minimumValue);

                // Wraps the host's storage, so that a use of IStorage.Payments anywhere in the request fails the test.
                var hostStorage = (IStorage)services.Single(descriptor => descriptor.ServiceType == typeof(IStorage)).ImplementationInstance!;
                services.Replace(ServiceDescriptor.Singleton<IStorage>(new CheckingStorage(hostStorage)));
            });

        var trackingNumbers = new List<long>();

        for (var i = 0; i < 3; i++)
        {
            host.StartCheckoutRequest();

            var result = await host.OnlinePayment.RequestAsync(invoice => invoice
                                                                          .UseAutoRandomTrackingNumber()
                                                                          .SetAmount(1_250_000)
                                                                          .SetCallbackUrl("https://shop.test/payment/callback")
                                                                          .SetGateway(FakeGateway.Name));

            Assert.That(result.IsSucceed, Is.True, result.Message);
            Assert.That(result.TrackingNumber >= minimumValue, Is.True, $"{result.TrackingNumber} is below the minimum {minimumValue}.");

            var payment = await host.GetPaymentAsync(result.TrackingNumber);
            Assert.That(payment.GatewayName, Is.EqualTo(FakeGateway.Name));

            trackingNumbers.Add(result.TrackingNumber);
        }

        Assert.That(trackingNumbers.Distinct().Count(), Is.EqualTo(trackingNumbers.Count));
    }

    private static AutoRandomTrackingNumber CreateFormatter(IStorage storage, long minimumValue) =>
        new(storage, Microsoft.Extensions.Options.Options.Create(new AutoRandomTrackingNumberOptions { MinimumValue = minimumValue }));

    /// <summary>
    /// Delegates to another storage and records the tracking numbers checked with
    /// <see cref="DoesPaymentExistAsync(long, CancellationToken)"/>.
    /// </summary>
    private sealed class CheckingStorage : IStorage
    {
        private readonly IStorage _inner;
        private readonly object _sync = new();
        private readonly List<long> _checkedNumbers = new();

        public CheckingStorage(IStorage? inner = null)
        {
            _inner = inner ?? new InMemoryStorage();
        }

        /// <summary>
        /// When set, decides whether a tracking number exists, from the number and the 1-based call count.
        /// Otherwise the inner storage decides.
        /// </summary>
        public Func<long, int, bool>? Exists { get; init; }

        public IReadOnlyList<long> CheckedNumbers
        {
            get
            {
                lock (_sync) return _checkedNumbers.ToList();
            }
        }

        public async Task<bool> DoesPaymentExistAsync(long trackingNumber, CancellationToken cancellationToken = default)
        {
            int call;

            lock (_sync)
            {
                _checkedNumbers.Add(trackingNumber);
                call = _checkedNumbers.Count;
            }

            return Exists?.Invoke(trackingNumber, call) ?? await _inner.DoesPaymentExistAsync(trackingNumber, cancellationToken);
        }

        public Task<bool> DoesPaymentExistAsync(string paymentToken, CancellationToken cancellationToken = default) =>
            _inner.DoesPaymentExistAsync(paymentToken, cancellationToken);

        public Task<long?> GetLargestTrackingNumberAsync(CancellationToken cancellationToken = default) =>
            _inner.GetLargestTrackingNumberAsync(cancellationToken);

        public Task<Payment?> FindPaidPaymentByBankReferenceAsync(string gatewayName, string bankReference, long exceptTrackingNumber, CancellationToken cancellationToken = default) =>
            _inner.FindPaidPaymentByBankReferenceAsync(gatewayName, bankReference, exceptTrackingNumber, cancellationToken);

        public Task<bool> TryCompletePaymentAsync(long paymentId, PaymentStatus status, string? transactionCode, string? bankReference, CancellationToken cancellationToken = default) =>
            _inner.TryCompletePaymentAsync(paymentId, status, transactionCode, bankReference, cancellationToken);

        public Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default) =>
            _inner.CreatePaymentAsync(payment, cancellationToken);

        public Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default) =>
            _inner.UpdatePaymentAsync(payment, cancellationToken);

        public Task DeletePaymentAsync(Payment payment, CancellationToken cancellationToken = default) =>
            _inner.DeletePaymentAsync(payment, cancellationToken);

        public Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default) =>
            _inner.CreateTransactionAsync(transaction, cancellationToken);

        public Task UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default) =>
            _inner.UpdateTransactionAsync(transaction, cancellationToken);

        public Task DeleteTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default) =>
            _inner.DeleteTransactionAsync(transaction, cancellationToken);

        public Task<Payment?> GetPaymentByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default) =>
            _inner.GetPaymentByTrackingNumberAsync(trackingNumber, cancellationToken);

        public Task<Payment?> GetPaymentByTokenAsync(string paymentToken, CancellationToken cancellationToken = default) =>
            _inner.GetPaymentByTokenAsync(paymentToken, cancellationToken);

        public Task<List<Transaction>> GetTransactionsAsync(Payment payment, CancellationToken cancellationToken = default) =>
            _inner.GetTransactionsAsync(payment, cancellationToken);
    }
}
