using System.Collections.Generic;
using System.Globalization;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.Gateway.PayPing;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// PayPing v2: pay (bearer token) → redirect to gotoipg → POST callback (refid, clientrefid) → pay/verify.
/// </summary>
/// <remarks>
/// The simulated bank records the amount exactly as sent. Whether PayPing v2 expects Toman while Parbad
/// sends Rial (G-H8) cannot be decided by a simulator and needs confirmation from PayPing.
/// </remarks>
[TestClass]
public class PayPingConformanceTests : GatewayConformanceTests
{
    private const string AccessToken = "payping-access-token";
    private const string Api = BankUrl + "/v2/pay";

    protected override string GatewayName => PayPingGateway.Name;

    protected override bool SupportsRefund => false;

    protected override IReadOnlyDictionary<string, string> KnownIssues => new Dictionary<string, string>
    {
        // refid comes from the browser and is not tied to this invoice; only the amount check protects it.
        [nameof(Payment_of_another_invoice_with_the_same_amount_cannot_settle_this_invoice)] = "G-C1b",
        [nameof(Payment_of_another_invoice_cannot_settle_this_invoice_even_with_this_invoices_token)] = "G-C1b"
    };

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddPayPing()
                .WithAccounts(accounts => accounts.AddInMemory(account => account.AccessToken = AccessToken))
                .WithOptions(options =>
                {
                    options.ApiRequestUrl = Api;
                    options.ApiVerificationUrl = $"{Api}/verify";
                    options.PaymentPageUrl = $"{Api}/gotoipg";
                });

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UsePayPing();

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When(Api).Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            if (call.Reject || call.Header("Authorization") != $"Bearer {AccessToken}")
            {
                return FakeBank.Json(new Dictionary<string, string> { ["1"] = "The token is not valid." }, HttpStatusCode.BadRequest);
            }

            var transaction = Bank.Open(
                long.Parse((string)call.Json["ClientRefId"]!, CultureInfo.InvariantCulture),
                (decimal)call.Json["Amount"]!,
                (string?)call.Json["ReturnUrl"]);

            return FakeBank.Json(new { code = transaction.Token });
        }));

        http.When($"{Api}/verify").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            var transaction = Bank.FindByReference((string?)call.Json["RefId"]);

            if (call.Reject || call.Header("Authorization") != $"Bearer {AccessToken}" || transaction is not { Paid: true } ||
                (decimal)call.Json["Amount"]! != transaction.Amount)
            {
                return FakeBank.Json(new Dictionary<string, string> { ["15"] = "The payment cannot be verified." }, HttpStatusCode.BadRequest);
            }

            // Assumption (unverified): a repeated verification is answered like the first one.
            transaction.Verified = true;

            return FakeBank.Json(new { amount = (long)transaction.Amount, cardNumber = transaction.MaskedCardNumber, cardHashPan = "HASH" });
        }));
    }

    /// <summary>A cancelled payment comes back without a refid.</summary>
    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        claims.Paid
            ? TestCallback.Post(
                ("refid", transaction.Reference),
                ("clientrefid", claims.OrderId.ToString(CultureInfo.InvariantCulture)),
                ("cardnumber", transaction.MaskedCardNumber))
            : TestCallback.Post(("clientrefid", claims.OrderId.ToString(CultureInfo.InvariantCulture)));
}
