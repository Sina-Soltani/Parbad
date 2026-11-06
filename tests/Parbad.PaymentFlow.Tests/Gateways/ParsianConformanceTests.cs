using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
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
[TestFixture]
public class ParsianConformanceTests : GatewayConformanceTests
{
    private const string LoginAccount = "PEC-LOGIN-ACCOUNT";
    private const string SpecialAdditionalData = "Order <42> & \"gift\" 'wrap' </sal:AdditionalData>";
    private const string SpecialSecondValue = "7&8<9>";
    private const string SaleNamespace = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService";
    private const string ConfirmNamespace = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService";
    private const string ReversalNamespace = "https://pec.Shaparak.ir/NewIPGServices/Reversal/ReversalService";

    private string? _receivedAdditionalData;
    private string? _receivedSecondValue;

    protected override string GatewayName => ParsianGateway.Name;

    protected override bool SupportsRefund => true;

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

    /// <summary>
    /// The SOAP envelope is built as a string, so every value must be XML-encoded. An unencoded "&amp;" or "&lt;"
    /// makes the request invalid XML, or lets the value add elements of its own.
    /// </summary>
    [Test]
    public async Task Request_values_with_xml_special_characters_reach_the_bank_unchanged()
    {
        Host.StartCheckoutRequest();

        var result = await Host.OnlinePayment.RequestAsync(invoice =>
        {
            invoice.SetTrackingNumber(InvoiceTrackingNumber)
                   .SetAmount(InvoiceAmount)
                   .SetCallbackUrl(CallbackUrl);

            UseGateway(invoice);
            invoice.SetParsianAdditionalData(new ParsianGatewayAdditionalDataRequest { AdditionalData = SpecialAdditionalData, Originator = SpecialSecondValue });
        });

        Assert.That(result.IsSucceed, Is.True, $"Request failed: {result.Message}");
        Assert.That(_receivedAdditionalData, Is.EqualTo(SpecialAdditionalData));
        Assert.That(_receivedSecondValue, Is.EqualTo(SpecialSecondValue));
    }

    /// <summary>
    /// The callback is read asynchronously from the form or the query string. A synchronous Request.Form
    /// threw on a GET request.
    /// </summary>
    [Test]
    public async Task Callback_sent_as_a_get_request_is_verified()
    {
        var transaction = await PayAsync(InvoiceTrackingNumber, InvoiceAmount);
        var post = CreateCallback(transaction, Claims(InvoiceTrackingNumber, InvoiceAmount));

        Host.ReceiveCallback(transaction.CallbackUrl!, new TestCallback { Method = HttpMethods.Get, Query = post.Form });
        var outcome = await FetchAndVerifyAsync();

        outcome.AssertNoUnexpectedException();
        Assert.That(outcome.Verify?.Status, Is.EqualTo(PaymentVerifyResultStatus.Succeed), outcome.Describe());
    }

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When($"{BankUrl}/NewIPGServices/Sale/SaleService.asmx").Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            if (call.Reject || call.Xml("LoginAccount") != LoginAccount)
            {
                return Result(SaleNamespace, "SalePaymentRequestResult", ("Token", "0"), ("Status", "-126"), ("Message", "Invalid merchant."));
            }

            _receivedAdditionalData = call.Xml("AdditionalData");
            _receivedSecondValue = call.Xml("Originator");

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
                // -1533 = "already confirmed" (several integrations; no official document found).
                // Assumption (unverified): the RRN is not returned again.
                return Result(ConfirmNamespace, "ConfirmPaymentResult", ("Status", "-1533"), ("RRN", "0"), ("Token", transaction.Token));
            }

            transaction.Verified = true;

            return Result(ConfirmNamespace, "ConfirmPaymentResult",
                          ("Status", "0"), ("RRN", transaction.Rrn), ("CardNumberMasked", transaction.MaskedCardNumber), ("Token", transaction.Token));
        }));

        // Assumption: the SOAP 1.2 envelope posted as text/xml is accepted (unverified).
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
