using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.Gateway.Pasargad;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// Pasargad REST API: Token/GetToken → Api/Payment/purchase (bearer) → redirect → callback →
/// Api/Payment/Verify-Payment; refunds use Api/Payment/Reverse-Transactions.
/// </summary>
[TestClass]
public class PasargadConformanceTests : GatewayConformanceTests
{
    private const string ApiBase = BankUrl + "/pasargad/";
    private const string TerminalNumber = "31234567";
    private const string Username = "merchant";
    private const string Password = "secret";
    private const string AccessToken = "jwt-access-token";

    /// <summary>
    /// How the simulated bank shapes "data" in a successful verify response. The shared scenarios use null;
    /// <see cref="Verify_accepts_a_response_whose_data_is_an_object"/> uses the object shape.
    /// </summary>
    private bool _verifyDataIsObject;

    protected override string GatewayName => PasargadGateway.Name;

    protected override bool SupportsRefund => true;

    protected override IReadOnlyDictionary<string, string> KnownIssues => new Dictionary<string, string>
    {
        // PostJsonAsync deserialises the body without checking the HTTP status.
        [nameof(Html_error_page_during_verification_does_not_crash_or_mark_the_payment_as_paid)] = "G-H9",
        // InvoiceDate is formatted with the current culture: under fa-IR it is a Persian date (e.g. 1405/08/11),
        // and "/" is the culture's date separator, so de-DE produces 2026.11.02.
        [nameof(Payment_is_verified_on_a_server_with_persian_culture)] = "G-H6",
        [nameof(Payment_is_verified_on_a_server_with_german_culture)] = "G-H6",
        [nameof(Partial_refund_is_not_silently_turned_into_a_full_refund)] = "ANALYSIS-02 §5 (partial refunds)"
    };

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddPasargad()
                .WithAccounts(accounts => accounts.AddInMemory(account =>
                {
                    account.TerminalNumber = TerminalNumber;
                    account.Username = Username;
                    account.Password = Password;
                }))
                .WithOptions(options => options.ApiBaseUrl = ApiBase);

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UsePasargad();

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When($"{ApiBase}Token/GetToken").Respond(Bank.Endpoint(BankOperation.Request, call =>
            (string?)call.Json["Username"] == Username && (string?)call.Json["Password"] == Password
                ? FakeBank.Json(new { ResultCode = 0, ResultMsg = "Successful", Token = AccessToken })
                : FakeBank.Json(new { ResultCode = 13001, ResultMsg = "Invalid username or password." })));

        http.When($"{ApiBase}Api/Payment/purchase").Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            var json = call.Json;

            if (call.Reject || !IsAuthorized(call) || (string?)json["TerminalNumber"] != TerminalNumber)
            {
                return FakeBank.Json(new { ResultCode = 13046, ResultMsg = "The terminal is not valid.", Data = (object?)null });
            }

            if (!IsTodayInGregorianCalendar((string?)json["InvoiceDate"]))
            {
                return FakeBank.Json(new { ResultCode = 13051, ResultMsg = "The invoice date is not valid.", Data = (object?)null });
            }

            var transaction = Bank.Open(
                long.Parse((string)json["Invoice"]!, CultureInfo.InvariantCulture),
                (decimal)json["Amount"]!,
                (string?)json["CallbackApi"]);

            return FakeBank.Json(new
            {
                ResultCode = 0,
                ResultMsg = "Successful",
                Data = new { UrlId = transaction.Token, Url = $"{ApiBase}pay/{transaction.Token}" }
            });
        }));

        http.When($"{ApiBase}Api/Payment/Verify-Payment").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            var transaction = Find(call);

            if (call.Reject || !IsAuthorized(call) || transaction is not { Paid: true })
            {
                return FakeBank.Json(new { ResultCode = 13029, ResultMsg = "The transaction is not valid.", Data = (object?)null });
            }

            // Assumption (unverified): a repeated verification is answered like the first one.
            transaction.Verified = true;

            object? data = _verifyDataIsObject
                ? new
                {
                    invoice = transaction.OrderId.ToString(CultureInfo.InvariantCulture),
                    referenceNumber = transaction.Reference,
                    trackId = transaction.TraceNumber,
                    maskedCardNumber = transaction.MaskedCardNumber,
                    amount = (long)transaction.Amount
                }
                : null;

            return FakeBank.Json(new { ResultCode = 0, ResultMsg = "Successful", Data = data });
        }));

        http.When($"{ApiBase}Api/Payment/Reverse-Transactions").Respond(Bank.Endpoint(BankOperation.Refund, call =>
        {
            var transaction = Find(call);

            if (!IsAuthorized(call) || transaction is not { Verified: true } || transaction.RefundedAmount > 0)
            {
                return FakeBank.Json(new { IsSuccess = false, Message = "The transaction cannot be reversed." });
            }

            transaction.RefundedAmount = transaction.Amount; // the API has no amount: the whole transaction is reversed

            return FakeBank.Json(new { IsSuccess = true, Message = "Successful" });
        }));
    }

    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        TestCallback.Get(
            ("invoiceId", claims.OrderId.ToString(CultureInfo.InvariantCulture)),
            ("status", claims.Paid ? "success" : "failed"),
            ("referenceNumber", claims.Paid ? transaction.Reference : ""),
            ("trackId", transaction.TraceNumber));

    /// <summary>
    /// Open question from ANALYSIS-02 §5: Parbad declares the verify response's "data" as a string, while the
    /// bank is believed to return an object. Confirm the real shape against the Pasargad sandbox.
    /// </summary>
    [TestMethod]
    public Task Verify_accepts_a_response_whose_data_is_an_object() =>
        KnownIssue.ReproducesAsync("ANALYSIS-02 §5 (Pasargad verify data shape, unverified)", () =>
            UnderCultureAsync(CultureInfo.InvariantCulture.Name, () =>
            {
                _verifyDataIsObject = true;

                return PaidInvoiceIsVerifiedAsync();
            }));

    private static bool IsAuthorized(BankCall call) => call.Header("Authorization") == $"Bearer {AccessToken}";

    /// <summary>The bank identifies the transaction by the pair (Invoice, UrlId).</summary>
    private BankTransaction? Find(BankCall call)
    {
        var transaction = Bank.FindByToken((string?)call.Json["UrlId"]);

        return transaction != null && transaction.OrderId.ToString(CultureInfo.InvariantCulture) == (string?)call.Json["Invoice"]
            ? transaction
            : null;
    }

    private static bool IsTodayInGregorianCalendar(string? invoiceDate) =>
        DateTime.TryParseExact(invoiceDate, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) &&
        Math.Abs((date - DateTime.Now).TotalDays) <= 1;
}
