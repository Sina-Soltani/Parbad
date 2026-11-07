using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Parbad.Builder;
using Parbad.Gateway.Sepehr;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// Sepehr (PeymentApi V1): GetToken → POST to the payment page → POST callback → Advice; refunds use Rollback.
/// </summary>
[TestFixture]
public class SepehrConformanceTests : GatewayConformanceTests
{
    private const long TerminalId = 99015001;
    private const string Api = BankUrl + "/V1/PeymentApi";

    protected override string GatewayName => SepehrGateway.Name;

    protected override bool SupportsRefund => true;

    protected override IReadOnlyDictionary<string, string> KnownIssues => new Dictionary<string, string>
    {
        // The digital receipt comes from the browser and is not tied to this invoice; only the amount protects it.
        [nameof(Payment_of_another_invoice_with_the_same_amount_cannot_settle_this_invoice)] = "same-amount payment swap",
        [nameof(Payment_of_another_invoice_cannot_settle_this_invoice_even_with_this_invoices_token)] = "same-amount payment swap"
    };

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddSepehr()
                .WithAccounts(accounts => accounts.AddInMemory(account => account.TerminalId = TerminalId))
                .WithOptions(options =>
                {
                    options.ApiTokenUrl = $"{Api}/GetToken";
                    options.ApiAdviceUrl = $"{Api}/Advice";
                    options.ApiRollbackUrl = $"{Api}/Rollback";
                    options.PaymentPageUrl = $"{BankUrl}/Pay";
                });

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UseSepehr();

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When($"{Api}/GetToken").Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            if (call.Reject || (long)call.Json["terminalId"]! != TerminalId) return FakeBank.Json(new { Status = -1, AccessToken = (string?)null });

            var transaction = Bank.Open(
                long.Parse((string)call.Json["invoiceId"]!, CultureInfo.InvariantCulture),
                (decimal)call.Json["amount"]!,
                (string?)call.Json["callbackUrl"]);

            return FakeBank.Json(new { Status = 0, AccessToken = transaction.Token });
        }));

        http.When($"{Api}/Advice").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            var transaction = Bank.FindByReference((string?)call.Json["digitalReceipt"]);

            if (call.Reject || (long)call.Json["tid"]! != TerminalId || transaction is not { Paid: true })
            {
                return FakeBank.Json(new { Status = "NOk", ReturnId = "-1", Message = "The transaction is not valid." });
            }

            var status = transaction.Verified ? "Duplicate" : "Ok";
            transaction.Verified = true;

            return FakeBank.Json(new { Status = status, ReturnId = transaction.Amount.ToString("0", CultureInfo.InvariantCulture), Message = "" });
        }));

        http.When($"{Api}/Rollback").Respond(Bank.Endpoint(BankOperation.Refund, call =>
        {
            var transaction = Bank.FindByReference((string?)call.Json["digitalReceipt"]);

            if ((long)call.Json["tid"]! != TerminalId || transaction is not { Verified: true }) return FakeBank.Json(new { Status = "NOk", ReturnId = "-1" });
            if (transaction.RefundedAmount > 0) return FakeBank.Json(new { Status = "Duplicate", ReturnId = transaction.Amount.ToString("0", CultureInfo.InvariantCulture) });

            transaction.RefundedAmount = transaction.Amount; // a rollback has no amount: the whole transaction

            return FakeBank.Json(new { Status = "Ok", ReturnId = transaction.Amount.ToString("0", CultureInfo.InvariantCulture) });
        }));
    }

    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        TestCallback.Post(
            ("respcode", claims.Paid ? "0" : "-1"),
            ("respmsg", claims.Paid ? "Success" : "Cancelled by the user"),
            ("invoiceid", claims.OrderId.ToString(CultureInfo.InvariantCulture)),
            ("terminalid", TerminalId.ToString(CultureInfo.InvariantCulture)),
            ("amount", claims.Amount.ToString("0", CultureInfo.InvariantCulture)),
            ("tracenumber", transaction.TraceNumber),
            ("rrn", transaction.Rrn),
            ("digitalreceipt", transaction.Reference),
            ("cardnumber", transaction.MaskedCardNumber));
}
