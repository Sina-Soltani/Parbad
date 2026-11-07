using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using NUnit.Framework;
using Parbad.Builder;
using Parbad.Gateway.Mellat;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// Mellat (Behpardakht) SOAP API: bpPayRequest → POST to the payment page → POST callback →
/// bpVerifyRequest + bpSettleRequest; refunds use bpReversalRequest.
/// </summary>
[TestFixture]
public class MellatConformanceTests : GatewayConformanceTests
{
    private const long TerminalId = 1234567;
    private const string UserName = "merchant";
    private const string Password = "secret";
    private const string ApiUrl = BankUrl + "/pgwchannel/services/pgw";
    private const string SpecialAdditionalData = "Order <42> & \"gift\" 'wrap' </additionalData>";
    private const string SpecialSecondValue = "7&8<9>";

    private string? _receivedAdditionalData;
    private string? _receivedSecondValue;

    protected override string GatewayName => MellatGateway.Name;

    protected override bool SupportsRefund => true;

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddMellat()
                .WithAccounts(accounts => accounts.AddInMemory(account =>
                {
                    account.TerminalId = TerminalId;
                    account.UserName = UserName;
                    account.UserPassword = Password;
                }))
                .WithOptions(options =>
                {
                    options.ApiUrl = ApiUrl;
                    options.PaymentPageUrl = $"{BankUrl}/pgwchannel/startpay.mellat";
                });

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UseMellat();

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
            invoice.SetMellatAdditionalData(new MellatGatewayAdditionalDataRequest { AdditionalData = SpecialAdditionalData, PayerId = SpecialSecondValue });
        });

        Assert.That(result.IsSucceed, Is.True, $"Request failed: {result.Message}");
        Assert.That(_receivedAdditionalData, Is.EqualTo(SpecialAdditionalData));
        Assert.That(_receivedSecondValue, Is.EqualTo(SpecialSecondValue));
    }

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When(ApiUrl).WithPartialContent("bpPayRequest").Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            if (call.Reject || !IsMerchant(call)) return Return("21"); // invalid merchant

            _receivedAdditionalData = call.Xml("additionalData");
            _receivedSecondValue = call.Xml("payerId");

            if (!IsTodayInGregorianCalendar(call.Xml("localDate"))) return Return("34"); // invalid local date

            var orderId = long.Parse(call.Xml("orderId")!, CultureInfo.InvariantCulture);

            if (Bank.FindByOrderId(orderId) != null) return Return("41"); // duplicate order id

            var transaction = Bank.Open(orderId, FakeBank.ParseAmount(call.Xml("amount")), call.Xml("callBackUrl"));

            return Return($"0,{transaction.Token}");
        }));

        http.When(ApiUrl).WithPartialContent("bpVerifyRequest").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            var transaction = FindPaid(call);

            if (call.Reject || transaction == null) return Return("54"); // reference transaction not found
            if (transaction.Verified) return Return("43"); // already verified

            transaction.Verified = true;

            return Return("0");
        }));

        http.When(ApiUrl).WithPartialContent("bpSettleRequest").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            var transaction = FindPaid(call);

            if (call.Reject || transaction is not { Verified: true }) return Return("54");
            if (transaction.Data.ContainsKey("settled")) return Return("45"); // already settled

            transaction.Data["settled"] = "true";

            return Return("0");
        }));

        // Assumption: the bank accepts a reversal of a settled transaction (unverified).
        http.When(ApiUrl).WithPartialContent("bpReversalRequest").Respond(Bank.Endpoint(BankOperation.Refund, call =>
        {
            var transaction = FindPaid(call);

            if (transaction is not { Verified: true } || transaction.RefundedAmount > 0) return Return("54");

            transaction.RefundedAmount = transaction.Amount; // a reversal has no amount: always the whole transaction

            return Return("0");
        }));
    }

    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        TestCallback.Post(
            ("RefId", claims.Token ?? transaction.Token),
            ("ResCode", claims.Paid ? "0" : "17"), // 17 = cancelled by the user
            ("SaleOrderId", claims.OrderId.ToString(CultureInfo.InvariantCulture)),
            ("SaleReferenceId", claims.Paid ? transaction.Reference : ""),
            ("CardHolderPan", transaction.MaskedCardNumber));

    private static bool IsMerchant(BankCall call) =>
        call.Xml("terminalId") == TerminalId.ToString(CultureInfo.InvariantCulture) &&
        call.Xml("userName") == UserName &&
        call.Xml("userPassword") == Password;

    /// <summary>The bank identifies the transaction by the pair (orderId, saleReferenceId).</summary>
    private BankTransaction? FindPaid(BankCall call)
    {
        if (!IsMerchant(call)) return null;

        var transaction = Bank.FindByReference(call.Xml("saleReferenceId"));

        return transaction is { Paid: true } && transaction.OrderId.ToString(CultureInfo.InvariantCulture) == call.Xml("orderId")
            ? transaction
            : null;
    }

    private static bool IsTodayInGregorianCalendar(string? localDate) =>
        DateTime.TryParseExact(localDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) &&
        Math.Abs((date - DateTime.Today).TotalDays) <= 1;

    private static System.Net.Http.HttpResponseMessage Return(string value) =>
        FakeBank.Text(
            "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body>" +
            $"<ns2:Response xmlns:ns2=\"http://interfaces.core.sw.bps.com/\"><return>{value}</return></ns2:Response>" +
            "</soap:Body></soap:Envelope>",
            "text/xml");
}
