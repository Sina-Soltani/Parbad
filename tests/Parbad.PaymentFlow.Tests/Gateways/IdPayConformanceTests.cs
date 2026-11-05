using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.Gateway.IdPay;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// IDPay v1.1: payment (X-API-KEY) → redirect → POST callback → payment/verify.
/// </summary>
[TestClass]
public class IdPayConformanceTests : GatewayConformanceTests
{
    private const string ApiKey = "idpay-api-key";
    private const string Api = BankUrl + "/v1.1/payment";

    protected override string GatewayName => IdPayGateway.Name;

    protected override bool SupportsRefund => false;

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddIdPay()
                .WithAccounts(accounts => accounts.AddInMemory(account => account.Api = ApiKey))
                .WithOptions(options =>
                {
                    options.ApiRequestUrl = Api;
                    options.ApiVerificationUrl = $"{Api}/verify";
                });

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UseIdPay();

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When(Api).Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            if (call.Reject || call.Header("X-API-KEY") != ApiKey) return Error(11, "The API key is not valid.");

            var transaction = Bank.Open((long)call.Json["order_id"]!, (decimal)call.Json["amount"]!, (string?)call.Json["callback"]);

            return FakeBank.Json(new { id = transaction.Token, link = $"{BankUrl}/p/ws/{transaction.Token}" }, HttpStatusCode.Created);
        }));

        http.When($"{Api}/verify").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            var transaction = Bank.FindByToken((string?)call.Json["id"]);

            // Assumption (unverified): IDPay rejects an id that does not belong to the sent order_id.
            if (call.Reject || call.Header("X-API-KEY") != ApiKey || transaction is not { Paid: true } ||
                transaction.OrderId != (long)call.Json["order_id"]!)
            {
                return Error(53, "The payment cannot be verified.");
            }

            // 101 = "already verified" (official IDPay docs).
            // Assumption (one library's sample only): the answer carries the same fields as 100.
            var status = transaction.Verified ? "101" : "100";
            transaction.Verified = true;

            return FakeBank.Json(new
            {
                status,
                track_id = transaction.Rrn,
                id = transaction.Token,
                order_id = transaction.OrderId.ToString(CultureInfo.InvariantCulture),
                amount = (long)transaction.Amount,
                payment = new { track_id = transaction.Rrn, amount = (long)transaction.Amount, card_no = transaction.MaskedCardNumber }
            });
        }));
    }

    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        TestCallback.Post(
            ("status", claims.Paid ? "10" : "7"), // 10 = ready for verification, 7 = cancelled by the user
            ("track_id", transaction.TraceNumber),
            ("id", claims.Token ?? transaction.Token),
            ("order_id", claims.OrderId.ToString(CultureInfo.InvariantCulture)),
            ("amount", claims.Amount.ToString("0", CultureInfo.InvariantCulture)),
            ("card_no", transaction.MaskedCardNumber));

    private static HttpResponseMessage Error(int code, string message) =>
        FakeBank.Json(new { error_code = code, error_message = message }, HttpStatusCode.NotAcceptable);
}
