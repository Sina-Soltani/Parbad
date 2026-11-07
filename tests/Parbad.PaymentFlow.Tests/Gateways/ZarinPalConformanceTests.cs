using Parbad.Storage.Abstractions.Models;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Parbad.Builder;
using Parbad.Exceptions;
using Parbad.Gateway.ZarinPal;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// ZarinPal API v4: payment/request.json → redirect to StartPay → GET callback (Authority, Status) →
/// payment/verify.json. Refunds use the GraphQL API: Terminals → Session (by ref_id) → AddRefund.
/// </summary>
[TestFixture]
public class ZarinPalConformanceTests : GatewayConformanceTests
{
    private const string MerchantId = "1344b5d4-0048-11e8-94db-005056a205be";
    private const string Api = BankUrl + "/pg/v4/payment";
    private const string GraphQl = BankUrl + "/api/v4/graphql/";
    private const string AccessToken = "panel-access-token";
    private const string TerminalId = "1915487";

    private string? _accountAccessToken = AccessToken;

    /// <summary>The answer to a rejected call, when a test needs another response shape than <see cref="Error"/>.</summary>
    private Func<HttpResponseMessage>? _rejection;

    private BankCall? _lastRequest;

    protected override string GatewayName => ZarinPalGateway.Name;

    protected override bool SupportsRefund => true;

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddZarinPal()
                .WithAccounts(accounts => accounts.AddInMemory(account =>
                {
                    account.MerchantId = MerchantId;
                    account.AuthorizationToken = _accountAccessToken;
                }))
                .WithOptions(options =>
                {
                    options.ApiRequestUrl = $"{Api}/request.json";
                    options.ApiVerificationUrl = $"{Api}/verify.json";
                    options.ApiGraphQlUrl = GraphQl;
                    options.PaymentPageUrl = $"{BankUrl}/pg/StartPay/";
                });

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UseZarinPal().SetZarinPalData("Order description");

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When($"{Api}/request.json").Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            if (call.Reject && _rejection != null) return _rejection();

            if (call.Reject || (string?)call.Json["merchant_id"] != MerchantId) return Error(-9, "The merchant id is not valid.");

            _lastRequest = call;

            var orderId = (string?)call.Json["metadata"]?["order_id"];
            var transaction = Bank.Open(orderId == null ? 0 : long.Parse(orderId), (decimal)call.Json["amount"]!, (string?)call.Json["callback_url"]);

            return FakeBank.Json(new { data = new { code = 100, message = "Success", authority = transaction.Token, fee_type = "Merchant", fee = 0 }, errors = new object[0] });
        }));

        http.When($"{Api}/verify.json").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            if (call.Reject && _rejection != null) return _rejection();

            var transaction = Bank.FindByToken((string?)call.Json["authority"]);

            if (call.Reject || (string?)call.Json["merchant_id"] != MerchantId || transaction is not { Paid: true })
            {
                return Error(-51, "The payment was not successful.");
            }

            if ((decimal)call.Json["amount"]! != transaction.Amount) return Error(-50, "The amount does not match the paid amount.");

            var code = transaction.Verified ? 101 : 100; // 101 = already verified
            transaction.Verified = true;

            return FakeBank.Json(new
            {
                data = new { code, message = code == 100 ? "Paid" : "Verified", ref_id = long.Parse(transaction.Rrn), card_pan = transaction.MaskedCardNumber, card_hash = "HASH", fee_type = "Merchant", fee = 0 },
                errors = new object[0]
            });
        }));

        http.When(GraphQl).Respond(Bank.Endpoint(BankOperation.Refund, call =>
        {
            if (call.Header("Authorization") != $"Bearer {AccessToken}")
            {
                return FakeBank.Json(new { data = (object?)null, errors = new[] { new { readable_code = "UNAUTHORIZED", message = "invalid access token or data" } } },
                                     HttpStatusCode.Unauthorized);
            }

            var query = (string)call.Json["query"]!;
            var variables = call.Json["variables"] as JObject;

            if (query.Contains("Terminals"))
            {
                return FakeBank.Json(new { data = new { Terminals = new[] { new { id = "1", key = "another-merchant" }, new { id = TerminalId, key = MerchantId } } } });
            }

            if (query.Contains("Session("))
            {
                // Assumption (unverified): Session can be filtered by reference_id (the verify response's ref_id).
                var sessions = (string?)variables?["terminal_id"] != TerminalId
                    ? new object[0]
                    : Bank.Transactions
                          .Where(transaction => transaction.Verified && transaction.Rrn == (string?)variables?["reference_id"])
                          .Select(transaction => (object)new { id = transaction.Reference, authority = transaction.Token, reference_id = transaction.Rrn, status = "VERIFIED", amount = (long)transaction.Amount })
                          .ToArray();

                return FakeBank.Json(new { data = new { Session = sessions } });
            }

            if (query.Contains("AddRefund"))
            {
                var transaction = Bank.FindByReference((string?)variables?["session_id"]);
                var amount = (decimal)variables!["amount"]!;

                if (call.Reject || transaction is not { Verified: true } || amount + transaction.RefundedAmount > transaction.Amount)
                {
                    return FakeBank.Json(new { data = (object?)null, errors = new[] { new { readable_code = "VALIDATION", message = "The refund is not valid." } } });
                }

                transaction.RefundedAmount += amount;

                return FakeBank.Json(new
                {
                    data = new
                    {
                        resource = new
                        {
                            terminal_id = TerminalId,
                            id = transaction.Reference,
                            amount = (long)transaction.Amount,
                            timeline = new { refund_amount = (long)amount, refund_time = "2026-11-06T10:00:00+03:30", refund_status = "PENDING" }
                        }
                    }
                });
            }

            return FakeBank.Json(new { data = (object?)null, errors = new[] { new { message = "Unknown operation." } } });
        }));
    }

    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        TestCallback.Get(("Authority", claims.Token ?? transaction.Token), ("Status", claims.Paid ? "OK" : "NOK"));

    /// <summary>
    /// A refund needs the access token of the ZarinPal panel; without it, Parbad says so and calls nothing.
    /// </summary>
    [Test]
    public async Task Refund_without_the_access_token_fails_clearly()
    {
        _accountAccessToken = null;
        await UseHostAsync(AddGateway);
        var transaction = await PayAndVerifyAsync(InvoiceTrackingNumber, InvoiceAmount);

        var result = await Host.OnlinePayment.RefundCompletelyAsync(InvoiceTrackingNumber);

        Assert.That(result.IsSucceed, Is.False);
        Assert.That(result.Message, Does.Contain("AuthorizationToken"));
        Assert.That(transaction.RefundedAmount, Is.EqualTo(0m));
    }

    /// <summary>
    /// A refund is ZarinPal's AddRefund for the payment's session, found by the verify response's ref_id.
    /// </summary>
    [Test]
    public async Task Refund_reaches_ZarinPal_with_the_session_of_this_payment_and_the_amount()
    {
        var transaction = await PayAndVerifyAsync(InvoiceTrackingNumber, InvoiceAmount);

        var result = await Host.OnlinePayment.RefundAsync(new RefundInvoice(InvoiceTrackingNumber, 400_000));

        Assert.That(result.IsSucceed, Is.True, result.Message);
        Assert.That(result.GatewayResponseCode, Is.EqualTo("PENDING"));
        Assert.That(transaction.RefundedAmount, Is.EqualTo(400_000m));
        Assert.That((await Host.GetPaymentAsync(InvoiceTrackingNumber)).Status, Is.EqualTo(PaymentStatus.PartiallyRefunded));
    }

    /// <summary>
    /// ZarinPal's documented request: mobile, email and the order id inside "metadata", no null values, and an
    /// Accept header for JSON.
    /// </summary>
    [Test]
    public async Task Request_sends_the_details_in_metadata_and_accepts_JSON()
    {
        await RequestAsync(InvoiceTrackingNumber, InvoiceAmount);

        Assert.That(_lastRequest, Is.Not.Null);
        Assert.That(_lastRequest.Header("Accept"), Does.Contain("application/json"));
        Assert.That(_lastRequest.Json["mobile"], Is.Null, "mobile is sent at the top level.");
        Assert.That(_lastRequest.Json["email"], Is.Null, "email is sent at the top level.");
        Assert.That((string?)_lastRequest.Json["metadata"]?["order_id"], Is.EqualTo(InvoiceTrackingNumber.ToString()));
        Assert.That(_lastRequest.Body.Contains("null"), Is.False, $"A null value is sent: {_lastRequest.Body}");
    }

    /// <summary>
    /// ZarinPal answers an error with "data" as an empty array or object and "errors" as an object or an array,
    /// with any HTTP status code (issues #310, #404, #432). The rejection must be reported with ZarinPal's code,
    /// not as a JSON exception.
    /// </summary>
    [TestCase(200, """{"data":[],"errors":{"code":-11,"message":"Merchant is not active.","validations":[]}}""", TestName = "{m}(HTTP 200, data [], errors object)")]
    [TestCase(200, """{"data":{},"errors":{"code":-11,"message":"Merchant is not active.","validations":[]}}""", TestName = "{m}(HTTP 200, data {}, errors object)")]
    [TestCase(200, """{"data":[],"errors":[{"code":-11,"message":"Merchant is not active."}]}""", TestName = "{m}(HTTP 200, errors array)")]
    [TestCase(200, """{"data":{"code":-11,"message":"Merchant is not active."},"errors":[]}""", TestName = "{m}(HTTP 200, code in data)")]
    [TestCase(422, """{"data":{},"errors":{"code":-11,"message":"Merchant is not active.","validations":[]}}""", TestName = "{m}(HTTP 422, data {}, errors object)")]
    [TestCase(401, """{"data":[],"errors":[{"code":-11,"message":"Merchant is not active."}]}""", TestName = "{m}(HTTP 401, errors array)")]
    public async Task Request_rejected_in_any_error_shape_is_reported_with_the_ZarinPal_code(int statusCode, string body)
    {
        Bank.RequestFault = BankFault.Reject;
        _rejection = () => Response(statusCode, body);

        var result = await RequestAsync(InvoiceTrackingNumber, InvoiceAmount);

        Assert.That(result.Status, Is.EqualTo(PaymentRequestResultStatus.Failed), result.Message);
        Assert.That(result.GatewayResponseCode, Is.EqualTo("-11"), result.Message);
        Assert.That(result.Message, Does.Contain("مرچنت کد فعال نیست"));
    }

    [TestCase(200, """{"data":[],"errors":{"code":-51,"message":"Session is not valid, session is not active paid try.","validations":[]}}""", TestName = "{m}(HTTP 200, data [], errors object)")]
    [TestCase(200, """{"data":[],"errors":[{"code":-51,"message":"Session is not valid, session is not active paid try."}]}""", TestName = "{m}(HTTP 200, errors array)")]
    [TestCase(200, """{"data":{"code":-51,"message":"Session is not valid, session is not active paid try."},"errors":[]}""", TestName = "{m}(HTTP 200, code in data)")]
    [TestCase(401, """{"data":{},"errors":{"code":-51,"message":"Session is not valid, session is not active paid try.","validations":[]}}""", TestName = "{m}(HTTP 401, data {}, errors object)")]
    public async Task Verification_rejected_in_any_error_shape_is_recorded_as_failed_with_the_ZarinPal_code(int statusCode, string body)
    {
        var transaction = await PayAsync(InvoiceTrackingNumber, InvoiceAmount);
        Bank.VerifyFault = BankFault.Reject;
        _rejection = () => Response(statusCode, body);

        DeliverCallback(transaction, transaction, Claims(InvoiceTrackingNumber, InvoiceAmount));
        var outcome = await FetchAndVerifyAsync();

        Assert.That(outcome.Exception, Is.Null, outcome.Describe());
        Assert.That(outcome.Verify?.Status, Is.EqualTo(PaymentVerifyResultStatus.Failed), outcome.Describe());
        Assert.That(outcome.Verify?.GatewayResponseCode, Is.EqualTo("-51"), outcome.Describe());
        Assert.That((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsPaid, Is.False);
    }

    /// <summary>
    /// A successful status code with neither data nor errors does not tell whether ZarinPal verified the payment,
    /// so the payment stays open and the retried callback verifies it.
    /// </summary>
    [TestCase("""{"data":[],"errors":[]}""", TestName = "{m}(data [], errors [])")]
    [TestCase("""{"data":{},"errors":[]}""", TestName = "{m}(data {}, errors [])")]
    [TestCase("""[]""", TestName = "{m}(JSON array)")]
    public async Task Verification_response_without_data_or_errors_leaves_the_payment_open_for_a_retry(string body)
    {
        var transaction = await PayAsync(InvoiceTrackingNumber, InvoiceAmount);
        Bank.VerifyFault = BankFault.Reject;
        _rejection = () => Response(200, body);

        DeliverCallback(transaction, transaction, Claims(InvoiceTrackingNumber, InvoiceAmount));
        var first = await FetchAndVerifyAsync();

        Assert.That(first.Exception, Is.InstanceOf<GatewayResponseException>(), first.Describe());
        Assert.That((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsCompleted, Is.False, first.Describe());

        Bank.VerifyFault = BankFault.None;
        DeliverCallback(transaction, transaction, Claims(InvoiceTrackingNumber, InvoiceAmount));
        var retry = await FetchAndVerifyAsync();

        Assert.That((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsPaid, Is.True, retry.Describe());
    }

    private static HttpResponseMessage Response(int statusCode, string body) =>
        new((HttpStatusCode)statusCode) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    /// <summary>The v4 error shape: HTTP 4xx, "data" as an empty array and "errors" as an object.</summary>
    private static HttpResponseMessage Error(int code, string message) =>
        FakeBank.Json(new { data = new object[0], errors = new { code, message, validations = new object[0] } }, HttpStatusCode.BadRequest);
}
