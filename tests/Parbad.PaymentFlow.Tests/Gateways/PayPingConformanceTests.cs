using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parbad.Builder;
using Parbad.Exceptions;
using Parbad.Gateway.PayPing;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// PayPing API v3: pay (bearer token) → redirect to pay/start/{paymentCode} → POST callback (status, errorCode, and
/// data as a JSON string) → pay/verify with paymentRefId, paymentCode and amount.
/// </summary>
/// <remarks>
/// The simulated bank records the amount exactly as sent. PayPing expects Toman; Parbad never converts the amount.
/// </remarks>
[TestClass]
public class PayPingConformanceTests : GatewayConformanceTests
{
    private const string AccessToken = "payping-access-token";
    private const string Api = BankUrl + "/v3/pay";

    /// <summary>The answer to a rejected verification, when a test needs another one than a ProblemDetails 400.</summary>
    private Func<HttpResponseMessage>? _verifyRejection;

    private JObject? _lastVerifyBody;

    protected override string GatewayName => PayPingGateway.Name;

    protected override bool SupportsRefund => false;

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddPayPing()
                .WithAccounts(accounts => accounts.AddInMemory(account => account.AccessToken = AccessToken))
                .WithOptions(options =>
                {
                    options.ApiRequestUrl = Api;
                    options.ApiVerificationUrl = $"{Api}/verify";
                    options.PaymentPageUrl = $"{Api}/start";
                });

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UsePayPing();

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When(Api).Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            if (call.Header("Authorization") != $"Bearer {AccessToken}") return Problem(HttpStatusCode.Unauthorized, 401, "Unauthorized");

            if (call.Reject) return Problem(HttpStatusCode.BadRequest, 101, "اطلاعات ارسالی معتبر نیست");

            var transaction = Bank.Open(
                long.Parse((string)call.Json["clientRefId"]!, CultureInfo.InvariantCulture),
                (decimal)call.Json["amount"]!,
                (string?)call.Json["returnUrl"]);

            return FakeBank.Json(new { paymentCode = transaction.Token, url = $"{Api}/start/{transaction.Token}", amount = (long)transaction.Amount });
        }));

        http.When($"{Api}/verify").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            _lastVerifyBody = call.Json;

            if (call.Reject && _verifyRejection != null) return _verifyRejection();

            if (call.Header("Authorization") != $"Bearer {AccessToken}") return Problem(HttpStatusCode.Unauthorized, 401, "Unauthorized");

            // Issue #446: the v2 body has no payment code.
            var paymentCode = (string?)call.Json["paymentCode"];
            if (string.IsNullOrEmpty(paymentCode)) return Problem(HttpStatusCode.BadRequest, 101, "مقدار فیلد 'کد پرداخت' اجباری است");

            var transaction = Bank.FindByReference((string?)call.Json["paymentRefId"]);

            // Assumption (unverified): PayPing rejects a paymentRefId that does not belong to the paymentCode (133,
            // "invalid payment information").
            if (call.Reject || transaction is not { Paid: true } || transaction.Token != paymentCode)
            {
                return Problem(HttpStatusCode.BadRequest, 133, "اطلاعات پرداخت نامعتبر است");
            }

            if ((decimal)call.Json["amount"]! != transaction.Amount) return Problem(HttpStatusCode.BadRequest, 137, "مبلغ نامعتبر است");

            var verified = new
            {
                amount = (long)transaction.Amount,
                cardNumber = transaction.MaskedCardNumber,
                cardHashPan = "HASH",
                clientRefId = transaction.OrderId.ToString(CultureInfo.InvariantCulture),
                paymentRefId = long.Parse(transaction.Reference, CultureInfo.InvariantCulture),
                code = transaction.Token,
                payedDate = "2026-11-05T10:00:00Z"
            };

            // 409 with code 110: verified before; metaData.message carries the transaction in PascalCase.
            if (transaction.Verified)
            {
                var details = JObject.FromObject(verified);
                var pascalCase = new JObject();
                foreach (var property in details.Properties()) pascalCase[char.ToUpperInvariant(property.Name[0]) + property.Name.Substring(1)] = property.Value;

                return Problem(HttpStatusCode.Conflict, 110, "پرداخت قبلا تایید شده است", pascalCase.ToString(Formatting.None));
            }

            transaction.Verified = true;

            return FakeBank.Json(verified);
        }));
    }

    /// <summary>The data of a cancelled payment has no paymentRefId; status 0 and errorCode 127 = cancelled.</summary>
    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        TestCallback.Post(("status", claims.Paid ? "1" : "0"),
                          ("errorCode", claims.Paid ? "" : "127"),
                          ("data", CallbackData(transaction, claims).ToString(Formatting.None)));

    /// <summary>
    /// Issue #446: the verification must send the payment code PayPing issued at request time, and the paymentRefId
    /// of the callback.
    /// </summary>
    [TestMethod]
    public async Task Verification_sends_the_payment_code_of_the_request_and_the_payment_reference()
    {
        var transaction = await PayAndVerifyAsync(InvoiceTrackingNumber, InvoiceAmount);

        Assert.IsNotNull(_lastVerifyBody);
        Assert.AreEqual(transaction.Token, (string?)_lastVerifyBody["paymentCode"]);
        Assert.AreEqual(long.Parse(transaction.Reference, CultureInfo.InvariantCulture), (long)_lastVerifyBody["paymentRefId"]!);
        Assert.AreEqual(transaction.Reference, (await Host.GetPaymentAsync(InvoiceTrackingNumber)).TransactionCode);
    }

    /// <summary>The official WooCommerce plugin notes that some callbacks arrive as a JSON body instead of a form.</summary>
    [TestMethod]
    public async Task Callback_with_a_JSON_body_is_verified()
    {
        var transaction = await PayAsync(InvoiceTrackingNumber, InvoiceAmount);
        var claims = Claims(InvoiceTrackingNumber, InvoiceAmount);

        var body = new JObject { ["status"] = 1, ["errorCode"] = null, ["data"] = CallbackData(transaction, claims) };
        var context = TestHttpContextAccessor.CreateRequest("POST", transaction.CallbackUrl!, requestServices: Host.Services);
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body.ToString(Formatting.None)));
        Host.HttpContextAccessor.HttpContext = context;

        var outcome = await FetchAndVerifyAsync();

        Assert.IsTrue(outcome.Verify?.IsSucceed ?? false, outcome.Describe());
        Assert.IsTrue((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsPaid, outcome.Describe());
    }

    /// <summary>HTTP 202: PayPing is still verifying. The payment stays open and the retried callback verifies it.</summary>
    [TestMethod]
    public async Task Verification_in_progress_leaves_the_payment_open_for_a_retry()
    {
        var transaction = await PayAsync(InvoiceTrackingNumber, InvoiceAmount);
        Bank.VerifyFault = BankFault.Reject;
        _verifyRejection = () => new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent("") };

        DeliverCallback(transaction, transaction, Claims(InvoiceTrackingNumber, InvoiceAmount));
        var first = await FetchAndVerifyAsync();

        Assert.IsInstanceOfType<GatewayResponseException>(first.Exception, first.Describe());
        Assert.IsFalse((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsCompleted, first.Describe());

        Bank.VerifyFault = BankFault.None;
        DeliverCallback(transaction, transaction, Claims(InvoiceTrackingNumber, InvoiceAmount));
        var retry = await FetchAndVerifyAsync();

        Assert.IsTrue((await Host.GetPaymentAsync(InvoiceTrackingNumber)).IsPaid, retry.Describe());
    }

    /// <summary>A rejection is reported with PayPing's code and message.</summary>
    [TestMethod]
    public async Task Rejected_verification_reports_the_PayPing_code_and_message()
    {
        var transaction = await PayAsync(InvoiceTrackingNumber, InvoiceAmount);
        Bank.VerifyFault = BankFault.Reject;

        DeliverCallback(transaction, transaction, Claims(InvoiceTrackingNumber, InvoiceAmount));
        var outcome = await FetchAndVerifyAsync();

        Assert.IsNull(outcome.Exception, outcome.Describe());
        Assert.AreEqual(PaymentVerifyResultStatus.Failed, outcome.Verify?.Status, outcome.Describe());
        Assert.AreEqual("133", outcome.Verify?.GatewayResponseCode, outcome.Describe());
        StringAssert.Contains(outcome.Verify?.Message, "اطلاعات پرداخت نامعتبر است");
    }

    private static JObject CallbackData(BankTransaction transaction, CallbackClaims claims)
    {
        var data = new JObject
        {
            ["clientRefId"] = claims.OrderId.ToString(CultureInfo.InvariantCulture),
            ["paymentCode"] = claims.Token ?? transaction.Token,
            ["amount"] = (long)claims.Amount
        };

        if (claims.Paid)
        {
            data["paymentRefId"] = long.Parse(transaction.Reference, CultureInfo.InvariantCulture);
            data["cardNumber"] = transaction.MaskedCardNumber;
            data["cardHashPan"] = "HASH";
        }

        return data;
    }

    /// <summary>PayPing's v3 error shape (ProblemDetails).</summary>
    private static HttpResponseMessage Problem(HttpStatusCode status, int code, string message, string? details = null) =>
        FakeBank.Json(new
        {
            type = "https://tools.ietf.org/html/rfc9110",
            title = "One or more validation errors occurred.",
            status = (int)status,
            paypingTraceId = "trace-id",
            metaData = new { code, message = details, errors = new[] { new { message } } }
        }, status);
}
