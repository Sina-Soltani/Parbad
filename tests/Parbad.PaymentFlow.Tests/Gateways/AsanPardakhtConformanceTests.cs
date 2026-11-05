using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.Gateway.AsanPardakht;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// AsanPardakht REST API v1 (guide v1.6): Token → POST of the RefId to the payment page → callback, which only tells
/// the merchant to look → TranResult by the merchant's own invoice id → Verify → Settlement. Every call carries the
/// usr/pwd headers; nothing is encrypted.
/// </summary>
[TestClass]
public class AsanPardakhtConformanceTests : GatewayConformanceTests
{
    private const long MerchantConfigurationId = 4321;
    private const string UserName = "merchant";
    private const string Password = "secret";
    private const string Api = BankUrl + "/v1";

    private readonly ConcurrentBag<string> _requestsWithoutHeaders = new();

    protected override string GatewayName => AsanPardakhtGateway.Name;

    protected override bool SupportsRefund => false;

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddAsanPardakht()
                .WithAccounts(accounts => accounts.AddInMemory(account =>
                {
                    account.MerchantConfigurationId = MerchantConfigurationId.ToString(CultureInfo.InvariantCulture);
                    account.UserName = UserName;
                    account.Password = Password;
                }))
                .WithOptions(options =>
                {
                    options.PaymentPageUrl = $"{BankUrl}/asan/";
                    options.ApiBaseUrl = Api + "/";
                });

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UseAsanPardakht();

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When(HttpMethod.Post, $"{Api}/Token").Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            Record(call);

            if (call.Reject || !IsAuthorized(call) || (long)call.Json["merchantConfigurationId"]! != MerchantConfigurationId)
            {
                return Status(473); // invalid user name or password
            }

            if (!IsNowInGregorianCalendar((string?)call.Json["localDate"])) return Status(485); // invalid local date and time

            var invoiceId = (long)call.Json["localInvoiceId"]!;

            if (Bank.FindByOrderId(invoiceId) != null) return Status(489); // duplicate invoice id

            var transaction = Bank.Open(invoiceId, (decimal)call.Json["amountInRials"]!, (string?)call.Json["callbackURL"]);

            // The body is the RefId as a JSON string.
            return FakeBank.Json(transaction.Token);
        }));

        http.When(HttpMethod.Get, $"{Api}/TranResult").Respond(Bank.Endpoint(BankOperation.Other, call =>
        {
            Record(call);

            if (!IsAuthorized(call) || call.Query("merchantConfigurationId") != MerchantConfigurationId.ToString(CultureInfo.InvariantCulture))
            {
                return Status(471);
            }

            var transaction = Bank.FindByOrderId(long.Parse(call.Query("localInvoiceId")!, CultureInfo.InvariantCulture));

            // No record: not paid, or cancelled by the user (guide p.6).
            if (transaction is not { Paid: true }) return Status(472);

            return FakeBank.Json(new
            {
                cardNumber = transaction.MaskedCardNumber,
                rrn = transaction.Rrn,
                refID = transaction.Data.TryGetValue("refID", out var refId) ? refId : transaction.Token,
                amount = (long)transaction.Amount,
                payGateTranID = long.Parse(transaction.Reference, CultureInfo.InvariantCulture),
                salesOrderID = transaction.OrderId,
                serviceTypeId = 1
            });
        }));

        http.When(HttpMethod.Post, $"{Api}/Verify").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            Record(call);

            var transaction = Find(call);

            if (call.Reject || transaction == null) return Status(476); // not found or reversed

            // Assumption (unverified; codes from mrdaghestani/AsanPardakht.IPG, ANALYSIS-02 §7.3): 473 = already verified.
            if (transaction.Verified) return Status(473);

            transaction.Verified = true;

            return Status(200);
        }));

        http.When(HttpMethod.Post, $"{Api}/Settlement").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            Record(call);

            var transaction = Find(call);

            // Settlement is only possible after Verify (guide p.12).
            if (call.Reject || transaction is not { Verified: true } || transaction.Data.ContainsKey("failSettlement")) return Status(471);

            // Assumption (unverified, as above): 474 = already settled.
            if (transaction.Data.ContainsKey("settled")) return Status(474);

            transaction.Data["settled"] = "true";

            return Status(200);
        }));
    }

    /// <summary>
    /// The guide documents no callback fields: the merchant reads its own invoice id from the callback URL and asks
    /// TranResult. The gateway ignores these fields, so forged values cannot matter.
    /// </summary>
    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        TestCallback.Post(("PayGateTranID", transaction.Reference), ("ReturningParams", claims.Paid ? "ok" : "cancelled"));

    /// <summary>
    /// The REST API needs no encryption (G-H4): the credentials go only into the usr/pwd headers, never into a
    /// body or a URL.
    /// </summary>
    [TestMethod]
    public async Task Credentials_are_sent_only_in_the_authentication_headers()
    {
        await PaidInvoiceIsVerifiedAsync();

        Assert.AreEqual(0, _requestsWithoutHeaders.Count, $"Calls without the usr/pwd headers: {string.Join(", ", _requestsWithoutHeaders)}");
    }

    /// <summary>
    /// TranResult is looked up by the invoice id, and the RefId stored at request time must match what the bank reports.
    /// </summary>
    [TestMethod]
    public async Task Payment_reported_with_another_RefId_is_not_verified()
    {
        var transaction = await PayAsync(InvoiceTrackingNumber, InvoiceAmount);
        transaction.Data["refID"] = "9999999999";

        DeliverCallback(transaction, transaction, Claims(InvoiceTrackingNumber, InvoiceAmount));
        var outcome = await FetchAndVerifyAsync();

        outcome.AssertNoUnexpectedException();
        Assert.IsFalse((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsPaid, outcome.Describe());
        Assert.IsFalse(transaction.Verified, "The bank was asked to verify a payment whose RefId does not match the request.");
    }

    /// <summary>
    /// After a successful Verify the money is captured; a failed settlement is done by Shaparak 12 hours later
    /// (guide v1.6, p.12), so the payment is paid.
    /// </summary>
    [TestMethod]
    public async Task Payment_is_recorded_as_paid_when_only_the_settlement_fails()
    {
        var transaction = await PayAsync(InvoiceTrackingNumber, InvoiceAmount);
        transaction.Data["failSettlement"] = "true";

        DeliverCallback(transaction, transaction, Claims(InvoiceTrackingNumber, InvoiceAmount));
        var outcome = await FetchAndVerifyAsync();

        outcome.AssertNoUnexpectedException();
        Assert.IsTrue(transaction.Verified, "Test setup: the bank should have verified the payment.");
        Assert.IsTrue((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsPaid, outcome.Describe());
    }

    private void Record(BankCall call)
    {
        var leaksCredentials = call.Body.Contains(Password) || call.Request.RequestUri!.ToString().Contains(Password);

        if (!IsAuthorized(call) || leaksCredentials) _requestsWithoutHeaders.Add($"{call.Request.Method} {call.Request.RequestUri}");
    }

    private static bool IsAuthorized(BankCall call) => call.Header("usr") == UserName && call.Header("pwd") == Password;

    private BankTransaction? Find(BankCall call)
    {
        if (!IsAuthorized(call) || (long)call.Json["merchantConfigurationId"]! != MerchantConfigurationId) return null;

        var transaction = Bank.FindByReference(((long)call.Json["payGateTranId"]!).ToString(CultureInfo.InvariantCulture));

        return transaction is { Paid: true } ? transaction : null;
    }

    /// <summary>"yyyyMMdd HHmmss", within one day of the bank's clock (guide p.8).</summary>
    private static bool IsNowInGregorianCalendar(string? localDateTime) =>
        localDateTime != null &&
        Regex.IsMatch(localDateTime, @"^\d{8} \d{6}$") &&
        DateTime.TryParseExact(localDateTime, "yyyyMMdd HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime) &&
        Math.Abs((dateTime - DateTime.Now).TotalHours) <= 24;

    private static HttpResponseMessage Status(int statusCode) => new((HttpStatusCode)statusCode) { Content = new StringContent("") };
}
