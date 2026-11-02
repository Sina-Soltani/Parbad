using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
[TestClass]
public class MellatConformanceTests : GatewayConformanceTests
{
    private const long TerminalId = 1234567;
    private const string UserName = "merchant";
    private const string Password = "secret";
    private const string ApiUrl = BankUrl + "/pgwchannel/services/pgw";

    protected override string GatewayName => MellatGateway.Name;

    protected override bool SupportsRefund => true;

    protected override IReadOnlyDictionary<string, string> KnownIssues => new Dictionary<string, string>
    {
        // The retry after a lost response gets code 43 (already verified), which is mapped to Failed.
        [nameof(Lost_verification_response_is_recovered_when_the_callback_is_retried)] = "G-H1",
        // localDate is formatted with the current culture: under fa-IR it is a Persian date (e.g. 14050811).
        [nameof(Payment_is_verified_on_a_server_with_persian_culture)] = "G-H6",
        [nameof(Partial_refund_is_not_silently_turned_into_a_full_refund)] = "ANALYSIS-02 §5 (partial refunds)"
    };

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

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When(ApiUrl).WithPartialContent("bpPayRequest").Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            if (call.Reject || !IsMerchant(call)) return Return("21"); // invalid merchant

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

        // Assumption: the bank accepts a reversal of a settled transaction (unverified, see ANALYSIS-02 §5).
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
            ("RefId", transaction.Token),
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
