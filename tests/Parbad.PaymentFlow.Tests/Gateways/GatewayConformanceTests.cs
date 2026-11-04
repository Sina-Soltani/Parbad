using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// What the bank's callback (sent through the user's browser) claims about a payment.
/// In an attack, these values are forged to match the victim invoice.
/// </summary>
/// <param name="Paid">Whether the callback reports a successful payment.</param>
/// <param name="OrderId">The order id (tracking number) the callback claims.</param>
/// <param name="Amount">The amount the callback claims, in the bank's unit.</param>
/// <param name="Token">
/// The request token the callback claims, when an attacker replaces it with the victim invoice's token (which
/// they know, because it passed through their browser). Null means the transaction's own token.
/// </param>
public sealed record CallbackClaims(bool Paid, long OrderId, decimal Amount, string? Token = null);

/// <summary>
/// The same payment scenarios, run against every gateway through the real <see cref="IOnlinePayment"/>
/// pipeline and a simulated bank (<see cref="FakeBank"/>).
/// <para>
/// A subclass describes one gateway: how to register it, how its bank answers (<see cref="SetupBank"/>) and
/// what its callback looks like (<see cref="CreateCallback"/>). The scenarios assert the <b>correct</b>
/// behaviour. Scenarios that fail because of a documented bug are listed in <see cref="KnownIssues"/> and are
/// reported as inconclusive until the bug is fixed (see <see cref="KnownIssue"/>).
/// </para>
/// </summary>
public abstract class GatewayConformanceTests
{
    protected const string BankUrl = "https://bank.test";
    protected const string CallbackUrl = "https://shop.test/payment/callback";
    protected const long InvoiceTrackingNumber = 1001;
    protected const long OtherTrackingNumber = 2002;

    private readonly Dictionary<long, BankTransaction> _bankTransactions = new();

    protected FakeBank Bank { get; private set; } = null!;

    protected ParbadTestHost Host { get; private set; } = null!;

    /// <summary>Amount of the invoice under test, in Rial.</summary>
    protected virtual decimal InvoiceAmount => 1_250_000;

    /// <summary>Amount of the attacker's cheaper invoice, in Rial.</summary>
    protected virtual decimal CheaperAmount => 120_000;

    /// <summary>The gateway's routing name, as reported in the results.</summary>
    protected abstract string GatewayName { get; }

    /// <summary>Whether the gateway implements refunds.</summary>
    protected abstract bool SupportsRefund { get; }

    /// <summary>Registers the gateway, an account and options that point to <see cref="BankUrl"/>.</summary>
    protected abstract void AddGateway(IGatewayBuilder gateways);

    /// <summary>Selects the gateway on the invoice and sets any data it requires.</summary>
    protected abstract void UseGateway(IInvoiceBuilder invoice);

    /// <summary>Registers the simulated bank endpoints. They answer from <see cref="Bank"/>.</summary>
    protected abstract void SetupBank(MockHttpMessageHandler http);

    /// <summary>Builds the callback the bank sends for <paramref name="transaction"/>, carrying <paramref name="claims"/>.</summary>
    protected abstract TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims);

    /// <summary>Converts a Parbad amount (Rial) to the amount the bank API receives.</summary>
    protected virtual decimal ToBankAmount(decimal amount) => amount;

    /// <summary>
    /// Whether verification is confirmed by a bank. False for gateways without a bank (the Virtual Gateway),
    /// which skips the checks on the bank side.
    /// </summary>
    protected virtual bool HasBank => true;

    /// <summary>
    /// Returns the bank transaction created by a request. By default it is the one the simulated bank opened
    /// while answering the request; gateways without a bank can create it from <paramref name="result"/>.
    /// </summary>
    protected virtual BankTransaction? TransactionOpenedBy(IPaymentRequestResult result, long trackingNumber, int bankTransactionsBefore) =>
        Bank.Transactions.Count > bankTransactionsBefore ? Bank.Transactions[^1] : null;

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    /// <summary>Scenario (test method name) → documented issue id that currently makes it fail.</summary>
    protected virtual IReadOnlyDictionary<string, string> KnownIssues => new Dictionary<string, string>();

    /// <summary>Scenario (test method name) → reason it does not apply to this gateway.</summary>
    protected virtual IReadOnlyDictionary<string, string> NotApplicable => new Dictionary<string, string>();

    [TestInitialize]
    public void InitializeGateway()
    {
        AssertScenarioNamesExist(KnownIssues.Keys, nameof(KnownIssues));
        AssertScenarioNamesExist(NotApplicable.Keys, nameof(NotApplicable));

        Bank = new FakeBank();
        Host = ParbadTestHost.Create(AddGateway, ConfigureServices);
        SetupBank(Host.Http);
    }

    [TestCleanup]
    public async Task CleanupGateway()
    {
        await Host.DisposeAsync();

        if (Bank.SimulatorErrors.Count > 0)
        {
            Assert.Fail($"The {GatewayName} bank simulator failed, so this result cannot be trusted: {Bank.SimulatorErrors[0]}");
        }

        if (Host.UnexpectedHttpCalls.Count > 0)
        {
            Assert.Fail($"{GatewayName} called endpoints the simulated bank does not provide: {string.Join(", ", Host.UnexpectedHttpCalls)}");
        }
    }

    // ------------------------------------------------------------------ request

    [TestMethod]
    public Task Request_is_accepted_and_the_bank_receives_the_invoice_amount() =>
        RunAsync(nameof(Request_is_accepted_and_the_bank_receives_the_invoice_amount), async () =>
        {
            var result = await RequestAsync(InvoiceTrackingNumber, InvoiceAmount);

            Assert.IsTrue(result.IsSucceed, $"Request failed: {result.Message}");
            Assert.IsFalse(string.IsNullOrEmpty(result.GatewayTransporter.Descriptor?.Url), "No payment page URL.");

            var transaction = BankTransactionOf(InvoiceTrackingNumber);
            Assert.AreEqual(ToBankAmount(InvoiceAmount), transaction.Amount, "The bank received a different amount.");

            if (transaction.CallbackUrl != null)
            {
                var payment = await Host.GetPaymentAsync(InvoiceTrackingNumber);
                StringAssert.Contains(transaction.CallbackUrl, payment.Token, "The callback URL sent to the bank does not identify the payment.");
            }
        });

    [TestMethod]
    public Task Rejected_request_is_reported_as_failed() =>
        RunAsync(nameof(Rejected_request_is_reported_as_failed), async () =>
        {
            Bank.RequestFault = BankFault.Reject;

            var result = await RequestAsync(InvoiceTrackingNumber, InvoiceAmount);

            Assert.AreEqual(PaymentRequestResultStatus.Failed, result.Status, $"Message: {result.Message}");
            Assert.IsFalse((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsPaid);
        });

    [TestMethod]
    public Task Html_error_page_on_request_is_reported_as_failed() =>
        RunAsync(nameof(Html_error_page_on_request_is_reported_as_failed), async () =>
        {
            Bank.RequestFault = BankFault.HtmlErrorPage;

            var result = await RequestAsync(InvoiceTrackingNumber, InvoiceAmount);

            Assert.AreEqual(PaymentRequestResultStatus.Failed, result.Status, $"Message: {result.Message}");
        });

    // ------------------------------------------------------------------ verify

    [TestMethod]
    public Task Paid_invoice_is_verified_and_recorded_as_paid() =>
        RunAsync(nameof(Paid_invoice_is_verified_and_recorded_as_paid), PaidInvoiceIsVerifiedAsync);

    [TestMethod]
    public Task Payment_cancelled_by_the_user_is_not_recorded_as_paid() =>
        RunAsync(nameof(Payment_cancelled_by_the_user_is_not_recorded_as_paid), async () =>
        {
            await RequestSucceededAsync(InvoiceTrackingNumber, InvoiceAmount);
            var transaction = BankTransactionOf(InvoiceTrackingNumber);

            DeliverCallback(transaction, transaction, Claims(InvoiceTrackingNumber, InvoiceAmount, paid: false));
            var outcome = await FetchAndVerifyAsync();

            outcome.AssertNoUnexpectedException();
            Assert.IsFalse(outcome.Verify?.IsSucceed ?? false, "A cancelled payment was verified.");
            Assert.IsFalse((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsPaid);
            Assert.IsFalse(transaction.Verified, "The bank was asked to settle a payment the user cancelled.");
        });

    [TestMethod]
    public Task Bank_rejection_during_verification_is_recorded_as_failed() =>
        RunAsync(nameof(Bank_rejection_during_verification_is_recorded_as_failed), async () =>
        {
            var transaction = await PayAsync(InvoiceTrackingNumber, InvoiceAmount);
            Bank.VerifyFault = BankFault.Reject;

            DeliverCallback(transaction, transaction, Claims(InvoiceTrackingNumber, InvoiceAmount));
            var outcome = await FetchAndVerifyAsync();

            outcome.AssertNoUnexpectedException();
            Assert.IsFalse(outcome.Verify?.IsSucceed ?? false, "The verification succeeded although the bank rejected it.");
            Assert.IsFalse((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsPaid);
        });

    [TestMethod]
    public Task Html_error_page_during_verification_does_not_crash_or_mark_the_payment_as_paid() =>
        RunAsync(nameof(Html_error_page_during_verification_does_not_crash_or_mark_the_payment_as_paid), async () =>
        {
            var transaction = await PayAsync(InvoiceTrackingNumber, InvoiceAmount);
            Bank.VerifyFault = BankFault.HtmlErrorPage;

            DeliverCallback(transaction, transaction, Claims(InvoiceTrackingNumber, InvoiceAmount));
            var outcome = await FetchAndVerifyAsync();

            outcome.AssertNoUnexpectedException();
            Assert.IsFalse(outcome.Verify?.IsSucceed ?? false, "The verification succeeded on an HTML error page.");
            Assert.IsFalse((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsPaid);
        });

    [TestMethod]
    public Task Lost_verification_response_is_recovered_when_the_callback_is_retried() =>
        RunAsync(nameof(Lost_verification_response_is_recovered_when_the_callback_is_retried), async () =>
        {
            var transaction = await PayAsync(InvoiceTrackingNumber, InvoiceAmount);
            Bank.VerifyFault = BankFault.LoseResponseOnce;

            DeliverCallback(transaction, transaction, Claims(InvoiceTrackingNumber, InvoiceAmount));
            var first = await FetchAndVerifyAsync();

            // The user refreshes the callback page (or the bank retries the callback).
            DeliverCallback(transaction, transaction, Claims(InvoiceTrackingNumber, InvoiceAmount));
            var retry = await FetchAndVerifyAsync();

            Assert.IsTrue(transaction.Verified, "Test setup: the bank should have processed the first verification.");
            Assert.IsTrue((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsPaid,
                          "The bank captured the money, but the payment is not recorded as paid. " +
                          $"First attempt: {first.Describe()}. Retry: {retry.Describe()}.");
        });

    /// <summary>fa-IR: Persian calendar by default, "٫" as decimal separator.</summary>
    [TestMethod]
    public Task Payment_is_verified_on_a_server_with_persian_culture() =>
        RunAsync(nameof(Payment_is_verified_on_a_server_with_persian_culture),
                 () => UnderCultureAsync("fa-IR", PaidInvoiceIsVerifiedAsync));

    /// <summary>de-DE: "." as date separator and "," as decimal separator.</summary>
    [TestMethod]
    public Task Payment_is_verified_on_a_server_with_german_culture() =>
        RunAsync(nameof(Payment_is_verified_on_a_server_with_german_culture),
                 () => UnderCultureAsync("de-DE", PaidInvoiceIsVerifiedAsync));

    // ------------------------------------------------------------------ attacks

    [TestMethod]
    public Task Payment_of_a_cheaper_invoice_cannot_settle_this_invoice() =>
        RunAsync(nameof(Payment_of_a_cheaper_invoice_cannot_settle_this_invoice),
                 () => PaymentOfAnotherInvoiceCannotSettleThisInvoiceAsync(CheaperAmount));

    [TestMethod]
    public Task Payment_of_another_invoice_with_the_same_amount_cannot_settle_this_invoice() =>
        RunAsync(nameof(Payment_of_another_invoice_with_the_same_amount_cannot_settle_this_invoice),
                 () => PaymentOfAnotherInvoiceCannotSettleThisInvoiceAsync(InvoiceAmount));

    /// <summary>
    /// The attacker also replaces the request token in the callback with the victim invoice's token, so a check
    /// that only compares the token with the one stored at request time is not enough.
    /// </summary>
    [TestMethod]
    public Task Payment_of_another_invoice_cannot_settle_this_invoice_even_with_this_invoices_token() =>
        RunAsync(nameof(Payment_of_another_invoice_cannot_settle_this_invoice_even_with_this_invoices_token),
                 () => PaymentOfAnotherInvoiceCannotSettleThisInvoiceAsync(InvoiceAmount, forgeVictimToken: true));

    /// <summary>
    /// The attacker pays and verifies their own invoice, then sends its callback again to this invoice's callback
    /// URL. The bank answers "already verified", which must not settle this invoice too: one payment, two orders.
    /// </summary>
    [TestMethod]
    public Task Payment_that_settled_another_invoice_cannot_settle_this_invoice() =>
        RunAsync(nameof(Payment_that_settled_another_invoice_cannot_settle_this_invoice), async () =>
        {
            await RequestSucceededAsync(InvoiceTrackingNumber, InvoiceAmount);
            var victim = BankTransactionOf(InvoiceTrackingNumber);

            var attacker = await PayAndVerifyAsync(OtherTrackingNumber, InvoiceAmount);

            DeliverCallback(victim, attacker, Claims(InvoiceTrackingNumber, InvoiceAmount) with { Token = victim.Token });
            var outcome = await FetchAndVerifyAsync();

            outcome.AssertNoUnexpectedException();
            Assert.IsFalse((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsPaid,
                           $"Invoice {InvoiceTrackingNumber} was recorded as paid with the bank transaction that already settled " +
                           $"invoice {OtherTrackingNumber}. {outcome.Describe()}");
        });

    // ------------------------------------------------------------------ refund

    [TestMethod]
    public Task Full_refund_reaches_the_bank() =>
        RunAsync(nameof(Full_refund_reaches_the_bank), async () =>
        {
            RequireRefundSupport();
            var transaction = await PayAndVerifyAsync(InvoiceTrackingNumber, InvoiceAmount);

            var result = await Host.OnlinePayment.RefundCompletelyAsync(InvoiceTrackingNumber);

            Assert.IsTrue(result.IsSucceed, $"Refund failed: {result.Message}");
            Assert.AreEqual(transaction.Amount, transaction.RefundedAmount, "The bank did not refund the full amount.");
        });

    [TestMethod]
    public Task Partial_refund_is_not_silently_turned_into_a_full_refund() =>
        RunAsync(nameof(Partial_refund_is_not_silently_turned_into_a_full_refund), async () =>
        {
            RequireRefundSupport();
            var transaction = await PayAndVerifyAsync(InvoiceTrackingNumber, InvoiceAmount);
            var partialAmount = InvoiceAmount / 2;

            var result = await Host.OnlinePayment.RefundAsync(new RefundInvoice(InvoiceTrackingNumber, partialAmount));

            if (result.IsSucceed)
            {
                Assert.AreEqual(ToBankAmount(partialAmount), transaction.RefundedAmount,
                                $"A refund of {partialAmount} was requested, but the bank refunded {transaction.RefundedAmount}.");
            }
            else
            {
                Assert.AreEqual(0m, transaction.RefundedAmount, "The refund was reported as failed, but the bank refunded money.");
            }
        });

    [TestMethod]
    public Task Refund_on_a_gateway_without_refund_support_fails_cleanly() =>
        RunAsync(nameof(Refund_on_a_gateway_without_refund_support_fails_cleanly), async () =>
        {
            if (SupportsRefund) Assert.Inconclusive($"Not applicable: {GatewayName} supports refunds.");

            await PayAndVerifyAsync(InvoiceTrackingNumber, InvoiceAmount);

            IPaymentRefundResult? result = null;
            Exception? exception = null;

            try
            {
                result = await Host.OnlinePayment.RefundCompletelyAsync(InvoiceTrackingNumber);
            }
            catch (Exception caught)
            {
                exception = caught;
            }

            Assert.IsNull(exception, $"Refund threw {exception?.GetType().Name}: {exception?.Message}");
            Assert.IsFalse(result!.IsSucceed, "A gateway without refund support reported a successful refund.");
        });

    // ------------------------------------------------------------------ scenario steps

    /// <summary>
    /// Replaces the host with one whose gateway is registered by <paramref name="configureGateways"/>,
    /// for gateway-specific tests that need a different configuration (e.g. sandbox mode).
    /// </summary>
    protected async Task UseHostAsync(Action<IGatewayBuilder> configureGateways)
    {
        await Host.DisposeAsync();

        Host = ParbadTestHost.Create(configureGateways, ConfigureServices);
        SetupBank(Host.Http);
    }

    protected async Task PaidInvoiceIsVerifiedAsync() => await PaidInvoiceIsVerifiedAsync(InvoiceAmount);

    protected async Task PaidInvoiceIsVerifiedAsync(decimal amount)
    {
        var transaction = await PayAsync(InvoiceTrackingNumber, amount);
        Assert.AreEqual(ToBankAmount(amount), transaction.Amount, "The bank received a different amount.");

        DeliverCallback(transaction, transaction, Claims(InvoiceTrackingNumber, amount));
        var outcome = await FetchAndVerifyAsync();

        outcome.AssertNoUnexpectedException();
        Assert.AreEqual(PaymentFetchResultStatus.ReadyForVerifying, outcome.Fetch?.Status, $"Fetch: {outcome.Describe()}");
        Assert.AreEqual(PaymentVerifyResultStatus.Succeed, outcome.Verify?.Status, $"Verify: {outcome.Describe()}");
        Assert.IsFalse(string.IsNullOrEmpty(outcome.Verify!.TransactionCode), "No transaction code (RRN/reference) was recorded.");
        Assert.AreEqual(GatewayName, outcome.Verify.GatewayName);

        var payment = await Host.GetPaymentAsync(InvoiceTrackingNumber);
        Assert.IsTrue(payment.IsCompleted && payment.IsPaid, "The payment is not recorded as paid.");

        if (HasBank) Assert.IsTrue(transaction.Verified, "The bank never received a successful verification.");
    }

    private async Task PaymentOfAnotherInvoiceCannotSettleThisInvoiceAsync(decimal attackerAmount, bool forgeVictimToken = false)
    {
        // The victim invoice: requested, but never paid at the bank.
        await RequestSucceededAsync(InvoiceTrackingNumber, InvoiceAmount);
        var victim = BankTransactionOf(InvoiceTrackingNumber);

        // The attacker's own invoice: requested and paid at the bank, but its callback is held back.
        var attacker = await PayAsync(OtherTrackingNumber, attackerAmount);

        // The attacker sends the paid transaction's callback to the victim invoice's callback URL,
        // forging every browser-supplied field to match the victim invoice.
        var claims = Claims(InvoiceTrackingNumber, InvoiceAmount) with { Token = forgeVictimToken ? victim.Token : null };
        DeliverCallback(victim, attacker, claims);
        var outcome = await FetchAndVerifyAsync();

        outcome.AssertNoUnexpectedException();
        Assert.IsFalse((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsPaid,
                       $"Invoice {InvoiceTrackingNumber} ({InvoiceAmount:N0}) was recorded as paid with the bank transaction " +
                       $"of invoice {OtherTrackingNumber} ({attackerAmount:N0}).");
    }

    protected async Task<IPaymentRequestResult> RequestAsync(long trackingNumber, decimal amount)
    {
        var transactionsBefore = Bank.Transactions.Count;

        Host.StartCheckoutRequest();

        var result = await Host.OnlinePayment.RequestAsync(invoice =>
        {
            invoice.SetTrackingNumber(trackingNumber)
                   .SetAmount(amount)
                   .SetCallbackUrl(CallbackUrl);

            UseGateway(invoice);
        });

        if (TransactionOpenedBy(result, trackingNumber, transactionsBefore) is { } transaction)
        {
            _bankTransactions[trackingNumber] = transaction;
        }

        return result;
    }

    protected async Task RequestSucceededAsync(long trackingNumber, decimal amount)
    {
        var result = await RequestAsync(trackingNumber, amount);

        Assert.IsTrue(result.IsSucceed, $"Test setup: the request for invoice {trackingNumber} failed: {result.Message}");
    }

    /// <summary>Requests the invoice and simulates the user paying it at the bank.</summary>
    protected async Task<BankTransaction> PayAsync(long trackingNumber, decimal amount)
    {
        await RequestSucceededAsync(trackingNumber, amount);

        var transaction = BankTransactionOf(trackingNumber);
        transaction.Paid = true;

        return transaction;
    }

    protected async Task<BankTransaction> PayAndVerifyAsync(long trackingNumber, decimal amount)
    {
        var transaction = await PayAsync(trackingNumber, amount);

        DeliverCallback(transaction, transaction, Claims(trackingNumber, amount));
        var outcome = await FetchAndVerifyAsync();

        outcome.AssertNoUnexpectedException();
        Assert.IsTrue(outcome.Verify?.IsSucceed ?? false, $"Test setup: the payment could not be verified: {outcome.Describe()}");

        return transaction;
    }

    protected BankTransaction BankTransactionOf(long trackingNumber) =>
        _bankTransactions.TryGetValue(trackingNumber, out var transaction)
            ? transaction
            : throw new AssertFailedException($"The request for invoice {trackingNumber} never reached the bank.");

    protected CallbackClaims Claims(long trackingNumber, decimal amount, bool paid = true) =>
        new(paid, trackingNumber, ToBankAmount(amount));

    /// <summary>
    /// Delivers the callback of <paramref name="paidTransaction"/> to the callback URL of <paramref name="invoiceTransaction"/>.
    /// </summary>
    protected void DeliverCallback(BankTransaction invoiceTransaction, BankTransaction paidTransaction, CallbackClaims claims)
    {
        var callbackUrl = invoiceTransaction.CallbackUrl
                          ?? throw new AssertFailedException("The simulated bank did not record the callback URL.");

        Host.ReceiveCallback(callbackUrl, CreateCallback(paidTransaction, claims));
    }

    protected async Task<FlowOutcome> FetchAndVerifyAsync()
    {
        var outcome = new FlowOutcome();

        try
        {
            outcome.Fetch = await Host.OnlinePayment.FetchAsync();

            if (outcome.Fetch.Status == PaymentFetchResultStatus.ReadyForVerifying)
            {
                outcome.Verify = await Host.OnlinePayment.VerifyAsync(outcome.Fetch);
            }
        }
        catch (Exception exception)
        {
            outcome.Exception = exception;
        }

        return outcome;
    }

    private void RequireRefundSupport()
    {
        if (!SupportsRefund) Assert.Inconclusive($"Not applicable: {GatewayName} does not support refunds.");
    }

    /// <summary>
    /// Runs a scenario under the invariant culture, so results do not depend on the machine running the tests.
    /// The culture scenarios switch to a specific culture inside the body.
    /// </summary>
    private Task RunAsync(string scenario, Func<Task> body)
    {
        if (NotApplicable.TryGetValue(scenario, out var reason))
        {
            Assert.Inconclusive($"Not applicable to {GatewayName}: {reason}");
        }

        Task Body() => UnderCultureAsync(CultureInfo.InvariantCulture.Name, body);

        return KnownIssues.TryGetValue(scenario, out var issue)
            ? KnownIssue.ReproducesAsync(issue, Body)
            : Body();
    }

    protected static async Task UnderCultureAsync(string cultureName, Func<Task> body)
    {
        var culture = CultureInfo.CurrentCulture;
        var uiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);

            await body();
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = uiCulture;
        }
    }

    private static void AssertScenarioNamesExist(IEnumerable<string> names, string listName)
    {
        var scenarios = typeof(GatewayConformanceTests)
                        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(method => method.GetCustomAttribute<TestMethodAttribute>() != null)
                        .Select(method => method.Name)
                        .ToHashSet();

        var unknown = names.Where(name => !scenarios.Contains(name)).ToList();

        if (unknown.Count > 0)
        {
            Assert.Fail($"{listName} refers to unknown scenarios: {string.Join(", ", unknown)}");
        }
    }
}

/// <summary>
/// The result of handling one callback: Fetch, then Verify when Fetch allowed it.
/// </summary>
public sealed class FlowOutcome
{
    public IPaymentFetchResult? Fetch { get; set; }

    public IPaymentVerifyResult? Verify { get; set; }

    public Exception? Exception { get; set; }

    /// <summary>
    /// Fails when Parbad threw anything other than a network error. A gateway must turn an unexpected bank
    /// response into a failed result, not into a NullReferenceException or a parsing exception.
    /// </summary>
    public void AssertNoUnexpectedException()
    {
        if (Exception != null && Exception is not HttpRequestException)
        {
            Assert.Fail($"Parbad threw {Exception.GetType().Name} instead of returning a failed result: {Exception.Message}{Environment.NewLine}{Exception.StackTrace}");
        }
    }

    public string Describe()
    {
        if (Exception != null) return $"threw {Exception.GetType().Name}: {Exception.Message}";

        var fetch = Fetch == null ? "no fetch" : $"fetch {Fetch.Status} ({Fetch.Message})";
        var verify = Verify == null ? "no verify" : $"verify {Verify.Status} ({Verify.Message})";

        return $"{fetch}, {verify}";
    }
}
