using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
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
            if (call.Reject || (string?)call.Json["merchant_id"] != MerchantId) return Error(-9, "The merchant id is not valid.");

            // The v4 request has no order id.
            var transaction = Bank.Open(0, (decimal)call.Json["amount"]!, (string?)call.Json["callback_url"]);

            return FakeBank.Json(new { data = new { code = 100, message = "Success", authority = transaction.Token, fee_type = "Merchant", fee = 0 }, errors = new object[0] });
        }));

        http.When($"{Api}/verify.json").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
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

        // Assumption: refund.json accepts merchant_id + authority without a bearer token (unverified, see ANALYSIS-02 §5).
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

    /// <summary>The v4 error shape: HTTP 4xx, "data" as an empty array and "errors" as an object.</summary>
    private static HttpResponseMessage Error(int code, string message) =>
        FakeBank.Json(new { data = new object[0], errors = new { code, message, validations = new object[0] } }, HttpStatusCode.BadRequest);
}
