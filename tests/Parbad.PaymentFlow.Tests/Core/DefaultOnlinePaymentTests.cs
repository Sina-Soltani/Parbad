using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.Exceptions;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using Parbad.Options;
using Parbad.PaymentFlow.Tests.Infrastructure;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.PaymentFlow.Tests.Core;

/// <summary>
/// Tests for the orchestration in <c>DefaultOnlinePayment</c>, independent of any bank protocol.
/// </summary>
[TestClass]
public class DefaultOnlinePaymentTests
{
    private const long TrackingNumber = 1001;
    private const long OtherTrackingNumber = 2002;
    private const decimal Amount = 1_250_000;
    private const string CallbackUrl = "https://shop.test/payment/callback";

    private ParbadTestHost _host = null!;
    private FakeGatewayBehaviour _gateway = null!;

    [TestInitialize]
    public void Initialize()
    {
        _gateway = new FakeGatewayBehaviour();

        _host = ParbadTestHost.Create(
            gateways => gateways
                        .AddGateway<FakeGateway>()
                        .WithAccounts<FakeGatewayAccount>(accounts => accounts.AddInMemory(_ => { })),
            services => services.AddSingleton(_gateway));
    }

    [TestCleanup]
    public Task Cleanup() => _host.DisposeAsync().AsTask();

    // ---------- Request ----------

    [TestMethod]
    public async Task Request_stores_the_payment_and_a_request_transaction()
    {
        var result = await RequestAsync();

        Assert.IsTrue(result.IsSucceed, result.Message);
        Assert.AreEqual(FakeGateway.PaymentPageUrl, result.GatewayTransporter.Descriptor.Url);

        var payment = await _host.GetPaymentAsync(TrackingNumber);
        Assert.AreEqual(Amount, payment.Amount);
        Assert.AreEqual(FakeGateway.Name, payment.GatewayName);
        Assert.IsFalse(payment.IsCompleted);
        Assert.IsFalse(payment.IsPaid);
        Assert.IsFalse(string.IsNullOrEmpty(payment.Token));

        var transactions = await _host.Storage.GetTransactionsAsync(payment);
        Assert.AreEqual(1, transactions.Count);
        Assert.AreEqual(TransactionType.Request, transactions[0].Type);
        Assert.IsTrue(transactions[0].IsSucceed);
    }

    [TestMethod]
    public async Task Request_with_an_existing_tracking_number_is_rejected()
    {
        await RequestAsync();

        var second = await RequestAsync();

        Assert.AreEqual(PaymentRequestResultStatus.TrackingNumberAlreadyExists, second.Status);
        Assert.AreEqual(1, _host.Storage.Payments.Count());
    }

    [TestMethod]
    public async Task Request_failure_is_recorded_as_a_failed_and_completed_payment()
    {
        _gateway.RequestException = new InvalidOperationException("Bank unavailable.");

        var result = await RequestAsync();

        Assert.AreEqual(PaymentRequestResultStatus.Failed, result.Status);

        var payment = await _host.GetPaymentAsync(TrackingNumber);
        Assert.AreEqual(PaymentStatus.Failed, payment.Status);
        Assert.IsTrue(payment.IsCompleted);
        Assert.IsFalse(payment.IsPaid);
    }

    [TestMethod]
    public async Task Rejected_request_is_recorded_as_failed()
    {
        _gateway.RequestResult = account => PaymentRequestResult.Failed("Invalid terminal.", account);

        var result = await RequestAsync();

        Assert.AreEqual(PaymentRequestResultStatus.Failed, result.Status);
        Assert.AreEqual(PaymentStatus.Failed, (await _host.GetPaymentAsync(TrackingNumber)).Status,
                        "A rejected request left the payment open, although the user never reached the bank.");
    }

    [TestMethod]
    public async Task Requested_payment_is_open()
    {
        await RequestAsync();

        Assert.AreEqual(PaymentStatus.Requested, (await _host.GetPaymentAsync(TrackingNumber)).Status);
    }

    [TestMethod]
    public Task Request_failure_does_not_expose_internal_exception_details() =>
        KnownIssue.ReproducesAsync("a failed request exposes internal exception details", async () =>
        {
            const string internalDetails = "SOAP fault from https://10.0.0.5/ws (user=merchant01)";
            _gateway.RequestException = new InvalidOperationException(internalDetails);

            var result = await RequestAsync();

            Assert.IsFalse(result.IsSucceed);
            Assert.IsFalse(result.Message?.Contains(internalDetails) ?? false,
                           $"The result message, which applications show to users, contains the internal exception text: \"{result.Message}\".");
        });

    // ---------- Fetch ----------

    [TestMethod]
    public async Task Fetch_with_an_unknown_payment_token_throws_InvoiceNotFound()
    {
        await RequestAsync();

        _host.ReceiveCallback($"{CallbackUrl}?paymentToken=unknown-token", TestCallback.Get());

        try
        {
            await _host.OnlinePayment.FetchAsync();
            Assert.Fail("Expected InvoiceNotFoundException.");
        }
        catch (InvoiceNotFoundException)
        {
        }
    }

    [TestMethod]
    public async Task Fetch_finds_the_payment_from_the_callback_token()
    {
        await RequestAsync();
        await ReceiveCallbackAsync();

        var fetch = await _host.OnlinePayment.FetchAsync();

        Assert.AreEqual(PaymentFetchResultStatus.ReadyForVerifying, fetch.Status);
        Assert.AreEqual(TrackingNumber, fetch.TrackingNumber);
        Assert.AreEqual(Amount, (decimal)fetch.Amount);
    }

    [TestMethod]
    public async Task Fetch_of_a_completed_payment_reports_it_as_already_processed()
    {
        await RequestAsync();
        await _host.OnlinePayment.VerifyAsync(TrackingNumber);
        await ReceiveCallbackAsync();

        var fetch = await _host.OnlinePayment.FetchAsync();

        Assert.AreEqual(PaymentFetchResultStatus.AlreadyProcessed, fetch.Status);
        Assert.IsTrue(fetch.IsAlreadyVerified);
    }

    // ---------- Verify ----------

    [TestMethod]
    public async Task Verify_records_a_successful_payment()
    {
        await RequestAsync();

        var result = await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        Assert.AreEqual(PaymentVerifyResultStatus.Succeed, result.Status);
        Assert.AreEqual(FakeGateway.TransactionCode, result.TransactionCode);

        var payment = await _host.GetPaymentAsync(TrackingNumber);
        Assert.AreEqual(PaymentStatus.Paid, payment.Status);
        Assert.IsTrue(payment.IsCompleted);
        Assert.IsTrue(payment.IsPaid);
        Assert.AreEqual(FakeGateway.TransactionCode, payment.TransactionCode);
        Assert.AreEqual(FakeGateway.TransactionCode, payment.BankReference, "A paid payment does not claim its bank reference.");

        var transactions = await _host.Storage.GetTransactionsAsync(payment);
        Assert.AreEqual(1, transactions.Count(t => t.Type == TransactionType.Verify && t.IsSucceed));
    }

    [TestMethod]
    public async Task Verify_of_an_already_verified_payment_does_not_call_the_gateway_again()
    {
        await RequestAsync();
        await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        var second = await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        Assert.AreEqual(PaymentVerifyResultStatus.AlreadyVerified, second.Status);
        Assert.AreEqual(1, _gateway.VerifyCalls);
    }

    [TestMethod]
    public async Task Verify_that_throws_leaves_the_payment_open_for_a_retry()
    {
        await RequestAsync();
        _gateway.VerifyException = new TimeoutException("The bank did not answer.");

        await ThrowsAsync<TimeoutException>(() => _host.OnlinePayment.VerifyAsync(TrackingNumber));

        var payment = await _host.GetPaymentAsync(TrackingNumber);
        Assert.IsFalse(payment.IsCompleted, "A payment whose verification failed with an exception must stay retryable.");

        _gateway.VerifyException = null;
        var retry = await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        Assert.AreEqual(PaymentVerifyResultStatus.Succeed, retry.Status);
    }

    [TestMethod]
    public async Task Concurrent_verifications_of_the_same_payment_call_the_gateway_only_once()
    {
        await RequestAsync();

        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _gateway.BeforeVerify = () => gate.Task;

        // Two callbacks for the same payment arrive at the same time (double click, refresh, bank retry).
        var first = Task.Run(() => _host.OnlinePayment.VerifyAsync(TrackingNumber));
        var second = Task.Run(() => _host.OnlinePayment.VerifyAsync(TrackingNumber));

        // One of them is at the bank; give the other one time to get there too, if nothing stops it.
        await WaitUntilAsync(() => _gateway.VerifyCalls >= 1, TimeSpan.FromSeconds(2));
        await Task.Delay(200);
        gate.SetResult();
        var results = await Task.WhenAll(first, second);

        Assert.AreEqual(1, _gateway.VerifyCalls,
                        "Both concurrent requests passed the IsCompleted check and asked the bank to verify the same payment.");
        Assert.IsTrue((await _host.GetPaymentAsync(TrackingNumber)).IsPaid);
        CollectionAssert.AreEquivalent(new[] { PaymentVerifyResultStatus.Succeed, PaymentVerifyResultStatus.AlreadyVerified },
                                       results.Select(result => result.Status).ToArray());
    }

    [TestMethod]
    public async Task Failed_verification_does_not_overwrite_a_payment_another_server_recorded_as_paid()
    {
        await RequestAsync();
        var paymentId = (await _host.GetPaymentAsync(TrackingNumber)).Id;

        // While this server waits for the bank, another server verifies the same payment successfully.
        _gateway.BeforeVerify = () => _host.Storage.TryCompletePaymentAsync(paymentId, isPaid: true, "RRN-OTHER-SERVER");
        _gateway.VerifyResult = _ => PaymentVerifyResult.Failed("Already being verified.");

        var result = await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        Assert.AreEqual(PaymentVerifyResultStatus.AlreadyVerified, result.Status, result.Message);
        Assert.AreEqual("RRN-OTHER-SERVER", result.TransactionCode);

        var payment = await _host.GetPaymentAsync(TrackingNumber);
        Assert.IsTrue(payment.IsPaid, "The failed result overwrote the payment the other server recorded as paid.");
        Assert.AreEqual("RRN-OTHER-SERVER", payment.TransactionCode);
    }

    [TestMethod]
    public async Task Successful_verification_replaces_a_failure_another_server_recorded()
    {
        await RequestAsync();
        var paymentId = (await _host.GetPaymentAsync(TrackingNumber)).Id;

        // The other server got an error from the bank and recorded the payment as failed; this server's verification
        // succeeds, so the bank has the money.
        _gateway.BeforeVerify = () => _host.Storage.TryCompletePaymentAsync(paymentId, isPaid: false, transactionCode: null);

        var result = await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        Assert.AreEqual(PaymentVerifyResultStatus.Succeed, result.Status, result.Message);

        var payment = await _host.GetPaymentAsync(TrackingNumber);
        Assert.IsTrue(payment.IsCompleted && payment.IsPaid, "A payment the bank confirmed is recorded as unpaid.");
        Assert.AreEqual(FakeGateway.TransactionCode, payment.TransactionCode);
    }

    [TestMethod]
    public async Task Concurrent_cancellation_does_not_overwrite_a_successful_verification()
    {
        await RequestAsync();

        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _gateway.BeforeVerify = () => gate.Task;

        var verification = Task.Run(() => _host.OnlinePayment.VerifyAsync(TrackingNumber));
        await WaitUntilAsync(() => _gateway.VerifyCalls >= 1, TimeSpan.FromSeconds(2));

        // The shop cancels the order while the bank is still answering the verification.
        var cancellation = Task.Run(() => _host.OnlinePayment.CancelAsync(TrackingNumber));
        await Task.Delay(200);
        gate.SetResult();

        var verifyResult = await verification;
        var cancelResult = await cancellation;

        Assert.AreEqual(PaymentVerifyResultStatus.Succeed, verifyResult.Status);
        Assert.IsFalse(cancelResult.IsSucceed, "A payment the bank confirmed was cancelled.");
        Assert.IsTrue((await _host.GetPaymentAsync(TrackingNumber)).IsPaid);
    }


    [TestMethod]
    public async Task Database_failure_after_bank_confirmation_leaves_the_payment_retryable()
    {
        // Characterises today's behaviour: the bank has confirmed, the database write fails,
        // and the payment stays open, so the only recovery is to verify again.
        await RequestAsync();
        _host.Storage.FailNextPaymentUpdate = new TimeoutException("Database timeout.");

        await ThrowsAsync<TimeoutException>(() => _host.OnlinePayment.VerifyAsync(TrackingNumber));

        Assert.AreEqual(1, _gateway.VerifyCalls, "The bank was asked to verify before the database failed.");
        Assert.IsFalse((await _host.GetPaymentAsync(TrackingNumber)).IsCompleted);
    }

    // ---------- Verification rules enforced by the core ----------

    [TestMethod]
    public async Task Verify_is_accepted_when_the_bank_reports_the_amount_and_order_of_this_invoice()
    {
        _gateway.VerifyResult = _ => Succeed(FakeGateway.TransactionCode, bankAmount: Amount, bankOrderId: "1001");
        await RequestAsync();

        var result = await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        Assert.AreEqual(PaymentVerifyResultStatus.Succeed, result.Status, result.Message);
        Assert.IsTrue((await _host.GetPaymentAsync(TrackingNumber)).IsPaid);
    }

    [TestMethod]
    public async Task Verify_is_rejected_when_the_bank_reports_a_different_amount()
    {
        _gateway.VerifyResult = _ => Succeed(FakeGateway.TransactionCode, bankAmount: Amount - 1);
        await RequestAsync();

        var result = await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        Assert.AreEqual(PaymentVerifyResultStatus.Failed, result.Status);
        Assert.AreEqual(new MessagesOptions().BankReportedDifferentAmount, result.Message);
        await AssertRecordedAsUnpaidAsync(TrackingNumber);
    }

    [TestMethod]
    public async Task Verify_is_rejected_when_the_bank_reports_a_different_invoice()
    {
        _gateway.VerifyResult = _ => Succeed(FakeGateway.TransactionCode, bankOrderId: "2002");
        await RequestAsync();

        var result = await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        Assert.AreEqual(PaymentVerifyResultStatus.Failed, result.Status);
        Assert.AreEqual(new MessagesOptions().BankReportedDifferentInvoice, result.Message);
        await AssertRecordedAsUnpaidAsync(TrackingNumber);
    }

    [TestMethod]
    public async Task Transaction_code_that_settled_another_payment_cannot_settle_this_one()
    {
        await RequestAsync(OtherTrackingNumber);
        await _host.OnlinePayment.VerifyAsync(OtherTrackingNumber);
        await RequestAsync();

        // The gateway reports the same bank transaction again, e.g. because the bank does not tie it to the invoice.
        var result = await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        Assert.AreEqual(PaymentVerifyResultStatus.Failed, result.Status);
        Assert.AreEqual(new MessagesOptions().TransactionCodeAlreadyUsed, result.Message);
        await AssertRecordedAsUnpaidAsync(TrackingNumber);
        Assert.IsTrue((await _host.GetPaymentAsync(OtherTrackingNumber)).IsPaid, "The first payment must stay paid.");
    }

    [TestMethod]
    public async Task Bank_reference_not_transaction_code_decides_whether_a_bank_transaction_settled_another_payment()
    {
        // Two different customer-facing codes, but the bank verified the same reference twice.
        _gateway.VerifyResult = context => PaymentVerifyResult.Paid($"RRN-{context.Payment.TrackingNumber}", "REF-A", "Paid.");

        await RequestAsync(OtherTrackingNumber);
        await _host.OnlinePayment.VerifyAsync(OtherTrackingNumber);
        await RequestAsync();

        var result = await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        Assert.AreEqual(PaymentVerifyResultStatus.Failed, result.Status);
        Assert.AreEqual(new MessagesOptions().TransactionCodeAlreadyUsed, result.Message);
        Assert.AreEqual("REF-A", (await _host.GetPaymentAsync(OtherTrackingNumber)).BankReference);
        await AssertRecordedAsUnpaidAsync(TrackingNumber);
    }

    [TestMethod]
    public async Task A_gateway_may_return_its_own_implementation_of_the_result_interface()
    {
        _gateway.VerifyResult = _ => new OwnVerifyResult();
        await RequestAsync();

        var result = await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        Assert.AreEqual(PaymentVerifyResultStatus.Succeed, result.Status, result.Message);
        Assert.AreEqual(TrackingNumber, result.TrackingNumber, "Parbad did not complete the result with the invoice's data.");
        Assert.AreEqual("OWN-REF", result.BankReference);

        var payment = await _host.GetPaymentAsync(TrackingNumber);
        Assert.IsTrue(payment.IsPaid);
        Assert.AreEqual("OWN-REF", payment.BankReference);
    }

    [TestMethod]
    public async Task Already_verified_at_the_bank_is_recorded_as_paid_when_the_transaction_code_is_unused()
    {
        _gateway.VerifyResult = _ => AlreadyVerifiedAtBank(FakeGateway.TransactionCode);
        await RequestAsync();

        var result = await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        Assert.AreEqual(PaymentVerifyResultStatus.Succeed, result.Status, result.Message);

        var payment = await _host.GetPaymentAsync(TrackingNumber);
        Assert.IsTrue(payment.IsCompleted && payment.IsPaid);
        Assert.AreEqual(FakeGateway.TransactionCode, payment.TransactionCode);
    }

    [TestMethod]
    public async Task Already_verified_at_the_bank_is_not_recorded_as_paid_when_the_transaction_code_settled_another_payment()
    {
        await RequestAsync(OtherTrackingNumber);
        await _host.OnlinePayment.VerifyAsync(OtherTrackingNumber);

        _gateway.VerifyResult = _ => AlreadyVerifiedAtBank(FakeGateway.TransactionCode);
        await RequestAsync();

        var result = await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        Assert.AreEqual(PaymentVerifyResultStatus.Failed, result.Status);
        Assert.AreEqual(new MessagesOptions().TransactionCodeAlreadyUsed, result.Message);
        await AssertRecordedAsUnpaidAsync(TrackingNumber);
    }

    [TestMethod]
    public async Task Already_verified_at_the_bank_without_a_transaction_code_is_not_recorded_as_paid()
    {
        _gateway.VerifyResult = _ => AlreadyVerifiedAtBank(transactionCode: null);
        await RequestAsync();

        var result = await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        Assert.AreEqual(PaymentVerifyResultStatus.Failed, result.Status);
        Assert.AreEqual(new MessagesOptions().AlreadyVerifiedPaymentCannotBeConfirmed, result.Message);
        await AssertRecordedAsUnpaidAsync(TrackingNumber);
    }



    // ---------- Cancel ----------

    [TestMethod]
    public async Task Cancel_marks_the_payment_as_completed_and_unpaid()
    {
        await RequestAsync();

        var result = await _host.OnlinePayment.CancelAsync(TrackingNumber, "Out of stock.");

        Assert.IsTrue(result.IsSucceed);

        var payment = await _host.GetPaymentAsync(TrackingNumber);
        Assert.AreEqual(PaymentStatus.Cancelled, payment.Status);
        Assert.IsTrue(payment.IsCompleted);
        Assert.IsFalse(payment.IsPaid);
        Assert.AreEqual(0, _gateway.VerifyCalls);
    }

    [TestMethod]
    public async Task Cancel_is_recorded_as_a_cancellation_not_as_a_verification()
    {
        await RequestAsync();
        await _host.OnlinePayment.CancelAsync(TrackingNumber, "Out of stock.");

        var payment = await _host.GetPaymentAsync(TrackingNumber);
        var last = (await _host.Storage.GetTransactionsAsync(payment)).Last();

        Assert.AreEqual(TransactionType.Cancel, last.Type,
                        "The cancellation is not stored as a Cancel transaction, so reports cannot tell it from a failed verification.");
        Assert.AreEqual("Out of stock.", last.Message);
    }

    // ---------- Refund ----------

    [TestMethod]
    public async Task Refund_of_a_paid_payment_sends_the_full_amount_to_the_gateway()
    {
        await RequestAsync();
        await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        var result = await _host.OnlinePayment.RefundCompletelyAsync(TrackingNumber);

        Assert.IsTrue(result.IsSucceed, result.Message);
        CollectionAssert.AreEqual(new[] { Amount }, _gateway.RefundedAmounts.ToArray());

        var payment = await _host.GetPaymentAsync(TrackingNumber);
        Assert.AreEqual(PaymentStatus.Refunded, payment.Status);
        Assert.AreEqual(Amount, payment.RefundedAmount);
        Assert.IsTrue(payment.IsPaid, "A refunded payment was paid.");
    }

    [TestMethod]
    public async Task Partial_refund_is_recorded_on_the_payment()
    {
        await RequestAsync();
        await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        await _host.OnlinePayment.RefundAsync(new RefundInvoice(TrackingNumber, 800_000));

        var payment = await _host.GetPaymentAsync(TrackingNumber);
        Assert.AreEqual(PaymentStatus.PartiallyRefunded, payment.Status);
        Assert.AreEqual(800_000m, payment.RefundedAmount);
    }

    [TestMethod]
    public async Task Failed_refund_does_not_change_the_payment()
    {
        await RequestAsync();
        await _host.OnlinePayment.VerifyAsync(TrackingNumber);
        _gateway.RefundResult = () => PaymentRefundResult.Failed("The bank rejected the refund.");

        var result = await _host.OnlinePayment.RefundCompletelyAsync(TrackingNumber);

        Assert.IsFalse(result.IsSucceed);
        var payment = await _host.GetPaymentAsync(TrackingNumber);
        Assert.AreEqual(PaymentStatus.Paid, payment.Status);
        Assert.AreEqual(0m, payment.RefundedAmount);
    }

    [TestMethod]
    public async Task Refund_of_an_unpaid_payment_is_rejected_without_calling_the_gateway()
    {
        await RequestAsync();
        await _host.OnlinePayment.CancelAsync(TrackingNumber);

        var result = await _host.OnlinePayment.RefundCompletelyAsync(TrackingNumber);

        Assert.IsFalse(result.IsSucceed, "A cancelled (never paid) payment was refunded.");
        Assert.AreEqual(0, _gateway.RefundedAmounts.Count, "The gateway was asked to refund a payment that was never paid.");
    }

    [TestMethod]
    public async Task Refunds_cannot_exceed_the_paid_amount_in_total()
    {
        await RequestAsync();
        await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        var first = await _host.OnlinePayment.RefundAsync(new RefundInvoice(TrackingNumber, 800_000));
        var second = await _host.OnlinePayment.RefundAsync(new RefundInvoice(TrackingNumber, 800_000));

        Assert.IsTrue(first.IsSucceed, first.Message);
        Assert.IsFalse(second.IsSucceed, "Two partial refunds of 800,000 were accepted for a payment of 1,250,000.");
        Assert.AreEqual(1, _gateway.RefundedAmounts.Count);
    }

    [TestMethod]
    public async Task Complete_refund_after_a_partial_refund_refunds_only_the_remaining_amount()
    {
        await RequestAsync();
        await _host.OnlinePayment.VerifyAsync(TrackingNumber);

        await _host.OnlinePayment.RefundAsync(new RefundInvoice(TrackingNumber, 800_000));
        var rest = await _host.OnlinePayment.RefundCompletelyAsync(TrackingNumber);
        var nothingLeft = await _host.OnlinePayment.RefundCompletelyAsync(TrackingNumber);

        Assert.IsTrue(rest.IsSucceed, rest.Message);
        Assert.AreEqual(Amount - 800_000, (decimal)rest.Amount);
        Assert.IsFalse(nothingLeft.IsSucceed, "A fully refunded payment was refunded again.");
        CollectionAssert.AreEqual(new[] { 800_000m, Amount - 800_000 }, _gateway.RefundedAmounts.ToArray());
    }

    // ---------- helpers ----------

    private Task<IPaymentRequestResult> RequestAsync(long trackingNumber = TrackingNumber)
    {
        _host.StartCheckoutRequest();

        return _host.OnlinePayment.RequestAsync(invoice => invoice
                                                           .SetTrackingNumber(trackingNumber)
                                                           .SetAmount(Amount)
                                                           .SetCallbackUrl(CallbackUrl)
                                                           .SetGateway(FakeGateway.Name));
    }

    private static PaymentVerifyResult Succeed(string transactionCode, decimal? bankAmount = null, string? bankOrderId = null)
    {
        return PaymentVerifyResult.Paid(transactionCode, transactionCode, "Paid.") with
               {
                   BankReportedAmount = bankAmount,
                   BankReportedOrderId = bankOrderId
               };
    }

    /// <summary>What a gateway returns when the bank answers "already verified" without tying it to the invoice.</summary>
    private static PaymentVerifyResult AlreadyVerifiedAtBank(string? transactionCode)
    {
        return new PaymentVerifyResult
               {
                   Status = PaymentVerifyResultStatus.Failed,
                   Message = "Already verified.",
                   TransactionCode = transactionCode,
                   BankReference = transactionCode,
                   IsAlreadyVerifiedAtBank = true
               };
    }

    /// <summary>A third-party gateway's own result type, not derived from Parbad's.</summary>
    private sealed class OwnVerifyResult : IPaymentVerifyResult
    {
        public long TrackingNumber => 0;
        public Money Amount => 0;
        public bool IsSucceed => true;
        public string GatewayName => "Fake";
        public string GatewayAccountName => "Fake";
        public string GatewayResponseCode => "00";
        public string Message => "Paid by a third-party gateway.";
        public IDictionary<string, object> AdditionalData { get; } = new Dictionary<string, object>();
        public IDictionary<string, string> DatabaseAdditionalData { get; } = new Dictionary<string, string>();
        public string TransactionCode => "OWN-RRN";
        public PaymentVerifyResultStatus Status => PaymentVerifyResultStatus.Succeed;
        public string? BankReference => "OWN-REF";
        public Money? BankReportedAmount => null;
        public string? BankReportedOrderId => null;
        public bool IsAlreadyVerifiedAtBank => false;
    }

    private async Task AssertRecordedAsUnpaidAsync(long trackingNumber)
    {
        var payment = await _host.GetPaymentAsync(trackingNumber);

        Assert.IsTrue(payment.IsCompleted, "The rejected payment must be completed, so it is not verified again.");
        Assert.IsFalse(payment.IsPaid, "The rejected payment was recorded as paid.");
    }


    private async Task ReceiveCallbackAsync() =>
        _host.ReceiveCallback(await _host.GetCallbackUrlAsync(TrackingNumber, CallbackUrl), TestCallback.Get());

    private static async Task ThrowsAsync<TException>(Func<Task> action) where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException)
        {
            return;
        }

        Assert.Fail($"Expected {typeof(TException).Name}.");
    }

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();

        while (!condition() && stopwatch.Elapsed < timeout)
        {
            await Task.Delay(10);
        }
    }
}
