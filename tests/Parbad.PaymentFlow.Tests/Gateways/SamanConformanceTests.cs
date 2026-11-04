using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.Gateway.Saman;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// Saman (SEP) token API: token request → POST to the payment page → POST callback → VerifyTransaction / ReverseTransaction.
/// </summary>
[TestClass]
public class SamanConformanceTests : GatewayConformanceTests
{
    private const string TerminalId = "12345678";

    protected override string GatewayName => SamanGateway.Name;

    protected override bool SupportsRefund => true;

    protected override IReadOnlyDictionary<string, string> KnownIssues => new Dictionary<string, string>
    {
        // The callback RefNum is never compared with anything stored for this invoice; only the amount protects it.
        [nameof(Payment_of_another_invoice_with_the_same_amount_cannot_settle_this_invoice)] = "G-C1b",
        [nameof(Payment_of_another_invoice_cannot_settle_this_invoice_even_with_this_invoices_token)] = "G-C1b"
    };

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddSaman()
                .WithAccounts(accounts => accounts.AddInMemory(account => account.TerminalId = TerminalId))
                .WithOptions(options =>
                {
                    options.ApiTokenUrl = $"{BankUrl}/onlinepg/onlinepg";
                    options.PaymentPageUrl = $"{BankUrl}/OnlinePG/OnlinePG";
                    options.ApiVerificationUrl = $"{BankUrl}/verifyTxnRandomSessionkey/ipg/VerifyTransaction";
                    options.ApiReverseUrl = $"{BankUrl}/verifyTxnRandomSessionkey/ipg/ReverseTransaction";
                });

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UseSaman();

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When($"{BankUrl}/onlinepg/onlinepg").Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            if (call.Reject || (string?)call.Json["TerminalId"] != TerminalId)
            {
                return FakeBank.Json(new { Status = -1, ErrorCode = "5", ErrorDesc = "Invalid terminal." });
            }

            var transaction = Bank.Open(
                long.Parse((string)call.Json["ResNum"]!, CultureInfo.InvariantCulture),
                (decimal)call.Json["Amount"]!,
                (string?)call.Json["RedirectUrl"]);

            return FakeBank.Json(new { Status = 1, Token = transaction.Token });
        }));

        http.When($"{BankUrl}/verifyTxnRandomSessionkey/ipg/VerifyTransaction").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            var transaction = Bank.FindByReference((string?)call.Json["RefNum"]);

            if (call.Reject || transaction is not { Paid: true } || (string?)call.Json["TerminalNumber"] != TerminalId)
            {
                return FakeBank.Json(new { ResultCode = -2, ResultDescription = "Transaction not found.", Success = false });
            }

            // 2 = "duplicate request" (official SEP guide v3.3, ANALYSIS-02 §7.3).
            // Assumption (unverified): the answer still carries the TransactionDetail.
            var resultCode = transaction.Verified ? 2 : 0;
            transaction.Verified = true;

            return FakeBank.Json(new
            {
                ResultCode = resultCode,
                ResultDescription = resultCode == 0 ? "Success" : "Duplicate request",
                Success = true,
                TransactionDetail = Detail(transaction)
            });
        }));

        http.When($"{BankUrl}/verifyTxnRandomSessionkey/ipg/ReverseTransaction").Respond(Bank.Endpoint(BankOperation.Refund, call =>
        {
            var transaction = Bank.FindByReference((string?)call.Json["RefNum"]);

            if (transaction is not { Verified: true } || transaction.RefundedAmount > 0)
            {
                return FakeBank.Json(new { ResultCode = -2, ResultDescription = "Transaction not found.", Success = false });
            }

            // The SEP reverse API has no amount: it always reverses the whole transaction.
            transaction.RefundedAmount = transaction.Amount;

            return FakeBank.Json(new { ResultCode = 0, ResultDescription = "Success", Success = true, TransactionDetail = Detail(transaction) });
        }));
    }

    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        TestCallback.Post(
            ("MID", TerminalId),
            ("TerminalId", TerminalId),
            ("State", claims.Paid ? "OK" : "CanceledByUser"),
            ("Status", claims.Paid ? "2" : "1"),
            ("RefNum", claims.Paid ? transaction.Reference : ""),
            ("ResNum", claims.OrderId.ToString(CultureInfo.InvariantCulture)),
            ("Amount", claims.Amount.ToString("0", CultureInfo.InvariantCulture)),
            ("Rrn", claims.Paid ? transaction.Rrn : ""),
            ("TraceNo", transaction.TraceNumber),
            ("SecurePan", transaction.MaskedCardNumber));

    private static object Detail(BankTransaction transaction) => new
    {
        Rrn = transaction.Rrn,
        RefNum = transaction.Reference,
        MaskedPan = transaction.MaskedCardNumber,
        TerminalNumber = long.Parse(TerminalId, CultureInfo.InvariantCulture),
        OrginalAmount = (long)transaction.Amount,
        AffectiveAmount = (long)transaction.Amount,
        StraceNo = transaction.TraceNumber
    };
}
