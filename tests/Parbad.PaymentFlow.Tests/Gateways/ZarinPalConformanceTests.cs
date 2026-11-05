using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
/// payment/verify.json; refunds use payment/refund.json.
/// </summary>
[TestClass]
public class ZarinPalConformanceTests : GatewayConformanceTests
{
    private const string MerchantId = "1344b5d4-0048-11e8-94db-005056a205be";
    private const string Api = BankUrl + "/pg/v4/payment";

    /// <summary>The answer to a rejected call, when a test needs another response shape than <see cref="Error"/>.</summary>
    private Func<HttpResponseMessage>? _rejection;

    protected override string GatewayName => ZarinPalGateway.Name;

    protected override bool SupportsRefund => true;

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddZarinPal()
                .WithAccounts(accounts => accounts.AddInMemory(account => account.MerchantId = MerchantId))
                .WithOptions(options =>
                {
                    options.ApiRequestUrl = $"{Api}/request.json";
                    options.ApiVerificationUrl = $"{Api}/verify.json";
                    options.ApiRefundUrl = $"{Api}/refund.json";
                    options.PaymentPageUrl = $"{BankUrl}/pg/StartPay/";
                });

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UseZarinPal().SetZarinPalData("Order description");

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When($"{Api}/request.json").Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            if (call.Reject && _rejection != null) return _rejection();

            if (call.Reject || (string?)call.Json["merchant_id"] != MerchantId) return Error(-9, "The merchant id is not valid.");

            // The v4 request has no order id.
            var transaction = Bank.Open(0, (decimal)call.Json["amount"]!, (string?)call.Json["callback_url"]);

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

        // Disproved assumption: the real refund.json answers 404 or 401. The simulator keeps it,
        // so the refund scenarios check Parbad's side only.
        http.When($"{Api}/refund.json").Respond(Bank.Endpoint(BankOperation.Refund, call =>
        {
            var transaction = Bank.FindByToken((string?)call.Json["authority"]);

            if ((string?)call.Json["merchant_id"] != MerchantId || transaction is not { Verified: true } || transaction.RefundedAmount > 0)
            {
                return Error(-80, "The transaction cannot be refunded.");
            }

            transaction.RefundedAmount = transaction.Amount;

            return FakeBank.Json(new { data = new { code = 100, message = "Refunded", ref_id = transaction.Rrn }, errors = new object[0] });
        }));
    }

    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        TestCallback.Get(("Authority", claims.Token ?? transaction.Token), ("Status", claims.Paid ? "OK" : "NOK"));

    /// <summary>
    /// ZarinPal answers an error with "data" as an empty array or object and "errors" as an object or an array,
    /// with any HTTP status code (issues #310, #404, #432). The rejection must be reported with ZarinPal's code,
    /// not as a JSON exception.
    /// </summary>
    [TestMethod]
    [DataRow(200, """{"data":[],"errors":{"code":-11,"message":"Merchant is not active.","validations":[]}}""", DisplayName = "HTTP 200, data [], errors object")]
    [DataRow(200, """{"data":{},"errors":{"code":-11,"message":"Merchant is not active.","validations":[]}}""", DisplayName = "HTTP 200, data {}, errors object")]
    [DataRow(200, """{"data":[],"errors":[{"code":-11,"message":"Merchant is not active."}]}""", DisplayName = "HTTP 200, errors array")]
    [DataRow(200, """{"data":{"code":-11,"message":"Merchant is not active."},"errors":[]}""", DisplayName = "HTTP 200, code in data")]
    [DataRow(422, """{"data":{},"errors":{"code":-11,"message":"Merchant is not active.","validations":[]}}""", DisplayName = "HTTP 422, data {}, errors object")]
    [DataRow(401, """{"data":[],"errors":[{"code":-11,"message":"Merchant is not active."}]}""", DisplayName = "HTTP 401, errors array")]
    public async Task Request_rejected_in_any_error_shape_is_reported_with_the_ZarinPal_code(int statusCode, string body)
    {
        Bank.RequestFault = BankFault.Reject;
        _rejection = () => Response(statusCode, body);

        var result = await RequestAsync(InvoiceTrackingNumber, InvoiceAmount);

        Assert.AreEqual(PaymentRequestResultStatus.Failed, result.Status, result.Message);
        Assert.AreEqual("-11", result.GatewayResponseCode, result.Message);
        StringAssert.Contains(result.Message, "مرچنت کد فعال نیست");
    }

    [TestMethod]
    [DataRow(200, """{"data":[],"errors":{"code":-51,"message":"Session is not valid, session is not active paid try.","validations":[]}}""", DisplayName = "HTTP 200, data [], errors object")]
    [DataRow(200, """{"data":[],"errors":[{"code":-51,"message":"Session is not valid, session is not active paid try."}]}""", DisplayName = "HTTP 200, errors array")]
    [DataRow(200, """{"data":{"code":-51,"message":"Session is not valid, session is not active paid try."},"errors":[]}""", DisplayName = "HTTP 200, code in data")]
    [DataRow(401, """{"data":{},"errors":{"code":-51,"message":"Session is not valid, session is not active paid try.","validations":[]}}""", DisplayName = "HTTP 401, data {}, errors object")]
    public async Task Verification_rejected_in_any_error_shape_is_recorded_as_failed_with_the_ZarinPal_code(int statusCode, string body)
    {
        var transaction = await PayAsync(InvoiceTrackingNumber, InvoiceAmount);
        Bank.VerifyFault = BankFault.Reject;
        _rejection = () => Response(statusCode, body);

        DeliverCallback(transaction, transaction, Claims(InvoiceTrackingNumber, InvoiceAmount));
        var outcome = await FetchAndVerifyAsync();

        Assert.IsNull(outcome.Exception, outcome.Describe());
        Assert.AreEqual(PaymentVerifyResultStatus.Failed, outcome.Verify?.Status, outcome.Describe());
        Assert.AreEqual("-51", outcome.Verify?.GatewayResponseCode, outcome.Describe());
        Assert.IsFalse((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsPaid);
    }

    /// <summary>
    /// A successful status code with neither data nor errors does not tell whether ZarinPal verified the payment,
    /// so the payment stays open and the retried callback verifies it.
    /// </summary>
    [TestMethod]
    [DataRow("""{"data":[],"errors":[]}""", DisplayName = "data [], errors []")]
    [DataRow("""{"data":{},"errors":[]}""", DisplayName = "data {}, errors []")]
    [DataRow("""[]""", DisplayName = "JSON array")]
    public async Task Verification_response_without_data_or_errors_leaves_the_payment_open_for_a_retry(string body)
    {
        var transaction = await PayAsync(InvoiceTrackingNumber, InvoiceAmount);
        Bank.VerifyFault = BankFault.Reject;
        _rejection = () => Response(200, body);

        DeliverCallback(transaction, transaction, Claims(InvoiceTrackingNumber, InvoiceAmount));
        var first = await FetchAndVerifyAsync();

        Assert.IsInstanceOfType<GatewayResponseException>(first.Exception, first.Describe());
        Assert.IsFalse((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsCompleted, first.Describe());

        Bank.VerifyFault = BankFault.None;
        DeliverCallback(transaction, transaction, Claims(InvoiceTrackingNumber, InvoiceAmount));
        var retry = await FetchAndVerifyAsync();

        Assert.IsTrue((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsPaid, retry.Describe());
    }

    private static HttpResponseMessage Response(int statusCode, string body) =>
        new((HttpStatusCode)statusCode) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    /// <summary>The v4 error shape: HTTP 4xx, "data" as an empty array and "errors" as an object.</summary>
    private static HttpResponseMessage Error(int code, string message) =>
        FakeBank.Json(new { data = new object[0], errors = new { code, message, validations = new object[0] } }, HttpStatusCode.BadRequest);
}
