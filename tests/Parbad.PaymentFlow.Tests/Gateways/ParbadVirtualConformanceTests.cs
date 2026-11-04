using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.Gateway.ParbadVirtual;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// The Virtual Gateway: a payment page served by the merchant's own application. There is no bank, and it
/// approves any payment by design (ANALYSIS-02 §3, design note), so the bank-side scenarios do not apply.
/// </summary>
[TestClass]
public class ParbadVirtualConformanceTests : GatewayConformanceTests
{
    private const string NoBank = "the Virtual Gateway has no bank and approves payments by design (ANALYSIS-02 §3).";

    protected override string GatewayName => ParbadVirtualGateway.Name;

    protected override bool SupportsRefund => true;

    protected override bool HasBank => false;

    protected override IReadOnlyDictionary<string, string> NotApplicable => new Dictionary<string, string>
    {
        [nameof(Rejected_request_is_reported_as_failed)] = NoBank,
        [nameof(Html_error_page_on_request_is_reported_as_failed)] = NoBank,
        [nameof(Bank_rejection_during_verification_is_recorded_as_failed)] = NoBank,
        [nameof(Html_error_page_during_verification_does_not_crash_or_mark_the_payment_as_paid)] = NoBank,
        [nameof(Html_error_page_during_verification_leaves_the_payment_open_for_a_retry)] = NoBank,
        [nameof(Server_error_during_verification_leaves_the_payment_open_for_a_retry)] = NoBank,
        [nameof(Lost_verification_response_is_recovered_when_the_callback_is_retried)] = NoBank,
        [nameof(Payment_of_a_cheaper_invoice_cannot_settle_this_invoice)] = NoBank,
        [nameof(Payment_of_another_invoice_with_the_same_amount_cannot_settle_this_invoice)] = NoBank,
        [nameof(Payment_of_another_invoice_cannot_settle_this_invoice_even_with_this_invoices_token)] = NoBank,
        [nameof(Full_refund_reaches_the_bank)] = NoBank,
        [nameof(Partial_refund_is_not_silently_turned_into_a_full_refund)] = NoBank
    };

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddParbadVirtual()
                .WithOptions(options => options.GatewayPath = "/virtual-gateway");

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UseParbadVirtual();

    protected override void SetupBank(MockHttpMessageHandler http)
    {
    }

    /// <summary>The "bank" transaction is the form the Virtual Gateway posts to its own payment page.</summary>
    protected override BankTransaction? TransactionOpenedBy(IPaymentRequestResult result, long trackingNumber, int bankTransactionsBefore)
    {
        var form = result.GatewayTransporter.Descriptor?.Form?.ToDictionary(pair => pair.Key, pair => pair.Value);

        if (form == null) return null;

        return Bank.Open(
            long.Parse(form["trackingNumber"], CultureInfo.InvariantCulture),
            FakeBank.ParseAmount(form["amount"]),
            form["redirectUrl"]);
    }

    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        TestCallback.Post(
            ("result", claims.Paid ? "true" : "false"),
            ("TransactionCode", transaction.Reference));
}
