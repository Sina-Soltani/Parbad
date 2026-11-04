using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.Gateway.Zibal;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// Zibal v1: request → redirect → GET callback (trackId, status) → verify.
/// </summary>
[TestClass]
public class ZibalConformanceTests : GatewayConformanceTests
{
    private const string Merchant = "zibal-merchant-id";
    private const string SandboxMerchant = "zibal";
    private const string ConfiguredHeader = "X-Configured-By-WithHttpClient";

    private string? _lastConfiguredHeader;

    protected override string GatewayName => ZibalGateway.Name;

    protected override bool SupportsRefund => false;

    protected override void AddGateway(IGatewayBuilder gateways) => AddZibal(gateways, isSandbox: false);

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UseZibal();

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When($"{BankUrl}/v1/request").Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            _lastConfiguredHeader = call.Header(ConfiguredHeader);
            var merchant = (string?)call.Json["merchant"];

            if (call.Reject || (merchant != Merchant && merchant != SandboxMerchant))
            {
                return FakeBank.Json(new { result = 102, message = "merchant not found" });
            }

            var transaction = Bank.Open(
                long.Parse((string)call.Json["orderId"]!, CultureInfo.InvariantCulture),
                (decimal)call.Json["amount"]!,
                (string?)call.Json["callbackUrl"]);
            transaction.Data["merchant"] = merchant!;

            return FakeBank.Json(new { trackId = long.Parse(transaction.Token, CultureInfo.InvariantCulture), result = 100, message = "success" });
        }));

        http.When($"{BankUrl}/v1/verify").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            var transaction = Bank.FindByToken((string?)call.Json["trackId"]);

            if (call.Reject || transaction == null || (string?)call.Json["merchant"] != transaction.Data["merchant"])
            {
                return FakeBank.Json(new { result = 102, message = "merchant not found" });
            }

            if (!transaction.Paid) return FakeBank.Json(new { result = 202, message = "order not paid or unsuccessful" });
            if (transaction.Verified) return FakeBank.Json(new { result = 201, message = "already verified" });

            transaction.Verified = true;

            return FakeBank.Json(new
            {
                paidAt = "2026-11-02T10:00:00.000000",
                cardNumber = transaction.MaskedCardNumber,
                status = 1,
                amount = (long)transaction.Amount,
                refNumber = long.Parse(transaction.Rrn, CultureInfo.InvariantCulture),
                orderId = transaction.OrderId.ToString(CultureInfo.InvariantCulture),
                result = 100,
                message = "success"
            });
        }));
    }

    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        TestCallback.Get(
            ("success", claims.Paid ? "1" : "0"),
            ("trackId", claims.Token ?? transaction.Token),
            ("orderId", claims.OrderId.ToString(CultureInfo.InvariantCulture)),
            ("status", claims.Paid ? "2" : "3")); // 3 = cancelled by the user

    [TestMethod]
    public Task Sandbox_payment_can_be_verified() =>
        UnderCultureAsync(CultureInfo.InvariantCulture.Name, async () =>
        {
            await UseHostAsync(gateways => AddZibal(gateways, isSandbox: true));

            await PaidInvoiceIsVerifiedAsync();
        });

    [TestMethod]
    public Task Http_client_configuration_is_applied_to_the_gateway() =>
        UnderCultureAsync(CultureInfo.InvariantCulture.Name, async () =>
        {
            await UseHostAsync(gateways => AddZibal(gateways, isSandbox: false)
                                   .WithHttpClient(client => client.ConfigureHttpClient(http => http.DefaultRequestHeaders.Add(ConfiguredHeader, "yes"))));

            await RequestSucceededAsync(InvoiceTrackingNumber, InvoiceAmount);

            Assert.AreEqual("yes", _lastConfiguredHeader,
                            "The configuration passed to WithHttpClient (also used by WithProxy) did not reach Zibal's HttpClient.");
        });

    /// <summary>3,000,000,000 Rial does not fit an Int32 amount in the verify response model.</summary>
    [TestMethod]
    public Task Large_amount_can_be_verified() =>
        UnderCultureAsync(CultureInfo.InvariantCulture.Name, () => PaidInvoiceIsVerifiedAsync(3_000_000_000));

    private static IGatewayConfigurationBuilder<ZibalGateway> AddZibal(IGatewayBuilder gateways, bool isSandbox) =>
        gateways.AddZibal()
                .WithAccounts(accounts => accounts.AddInMemory(account =>
                {
                    account.Merchant = Merchant;
                    account.IsSandbox = isSandbox;
                }))
                .WithOptions(options =>
                {
                    options.ApiRequestUrl = $"{BankUrl}/v1/request";
                    options.ApiVerificationUrl = $"{BankUrl}/v1/verify";
                    options.PaymentPageUrl = $"{BankUrl}/start";
                });
}
