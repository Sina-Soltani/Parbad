using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.Gateway.Parsian;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// Parsian (PEC NewIPGServices, SOAP): SalePaymentRequest → redirect → POST callback → ConfirmPayment;
/// refunds use ReversalRequest.
/// </summary>
[TestClass]
public class ParsianConformanceTests : GatewayConformanceTests
{
    private const string LoginAccount = "PEC-LOGIN-ACCOUNT";
    private const string SaleNamespace = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService";
    private const string ConfirmNamespace = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService";
    private const string ReversalNamespace = "https://pec.Shaparak.ir/NewIPGServices/Reversal/ReversalService";

    protected override string GatewayName => ParsianGateway.Name;

    protected override bool SupportsRefund => true;

    protected override IReadOnlyDictionary<string, string> KnownIssues => new Dictionary<string, string>
    {
        // Any non-zero ConfirmPayment status is Failed, including "already confirmed".
        [nameof(Lost_verification_response_is_recovered_when_the_callback_is_retried)] = "G-H1 (assumes -1533 = already confirmed)",
        [nameof(Partial_refund_is_not_silently_turned_into_a_full_refund)] = "ANALYSIS-02 §5 (partial refunds)"
    };

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddParsian()
                .WithAccounts(accounts => accounts.AddInMemory(account => account.LoginAccount = LoginAccount))
                .WithOptions(options =>
                {
                    options.PaymentPageUrl = $"{BankUrl}/NewIPG/";
                    options.ApiRequestUrl = $"{BankUrl}/NewIPGServices/Sale/SaleService.asmx";
                    options.ApiVerificationUrl = $"{BankUrl}/NewIPGServices/Confirm/ConfirmService.asmx";
                    options.ApiRefundUrl = $"{BankUrl}/NewIPGServices/Reverse/ReversalService.asmx";
                });

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UseParsian();

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When($"{BankUrl}/NewIPGServices/Sale/SaleService.asmx").Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            if (call.Reject || call.Xml("LoginAccount") != LoginAccount)
            {
                return Result(SaleNamespace, "SalePaymentRequestResult", ("Token", "0"), ("Status", "-126"), ("Message", "Invalid merchant."));
            }

            var transaction = Bank.Open(
                long.Parse(call.Xml("OrderId")!, CultureInfo.InvariantCulture),
                FakeBank.ParseAmount(call.Xml("Amount")),
                call.Xml("CallBackUrl"));

            return Result(SaleNamespace, "SalePaymentRequestResult", ("Token", transaction.Token), ("Status", "0"), ("Message", ""));
        }));

        http.When($"{BankUrl}/NewIPGServices/Confirm/ConfirmService.asmx").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            var transaction = Bank.FindByToken(call.Xml("Token"));

            if (call.Reject || call.Xml("LoginAccount") != LoginAccount || transaction is not { Paid: true })
            {
                return Result(ConfirmNamespace, "ConfirmPaymentResult", ("Status", "-1"), ("RRN", "0"), ("Token", call.Xml("Token") ?? ""));
            }

            if (transaction.Verified)
            {
                // -1533: the transaction has already been confirmed.
                return Result(ConfirmNamespace, "ConfirmPaymentResult", ("Status", "-1533"), ("RRN", "0"), ("Token", transaction.Token));
            }

            transaction.Verified = true;

            return Result(ConfirmNamespace, "ConfirmPaymentResult",
                          ("Status", "0"), ("RRN", transaction.Rrn), ("CardNumberMasked", transaction.MaskedCardNumber), ("Token", transaction.Token));
        }));

        // Assumption: the SOAP 1.2 envelope posted as text/xml is accepted (unverified, see ANALYSIS-02 §5).
        http.When($"{BankUrl}/NewIPGServices/Reverse/ReversalService.asmx").Respond(Bank.Endpoint(BankOperation.Refund, call =>
        {
            var transaction = Bank.FindByToken(call.Xml("Token"));

            if (call.Xml("LoginAccount") != LoginAccount || transaction is not { Verified: true } || transaction.RefundedAmount > 0)
            {
                return Result(ReversalNamespace, "ReversalRequestResult", ("Status", "-1"), ("Message", "Reversal failed."), ("Token", ""));
            }

            transaction.RefundedAmount = transaction.Amount; // a reversal has no amount: the whole transaction

            return Result(ReversalNamespace, "ReversalRequestResult", ("Status", "0"), ("Message", "Success"), ("Token", transaction.Token));
        }));
    }

    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        TestCallback.Post(
            ("token", claims.Token ?? transaction.Token),
            ("status", claims.Paid ? "0" : "-138"), // -138 = cancelled by the user
            ("orderId", claims.OrderId.ToString(CultureInfo.InvariantCulture)),
            ("amount", claims.Amount.ToString("0", CultureInfo.InvariantCulture)),
            ("RRN", claims.Paid ? transaction.Rrn : "0"),
            ("HashCardNumber", "HASHED-CARD"));

    private static HttpResponseMessage Result(string ns, string resultElement, params (string Name, string Value)[] fields)
    {
        var body = string.Concat(System.Array.ConvertAll(fields, field => $"<{field.Name}>{field.Value}</{field.Name}>"));

        return FakeBank.Text(
            "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body>" +
            $"<Response xmlns=\"{ns}\"><{resultElement}>{body}</{resultElement}></Response>" +
            "</soap:Body></soap:Envelope>",
            "text/xml");
    }
}
