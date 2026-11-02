using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Parbad.Builder;
using Parbad.Gateway.FanAva;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// FanAva (ref-payment REST): generateTokenWithNoSign → redirect → POST callback → inquiryMerchantToken →
/// verifyMerchantTrans; refunds use reverseMerchantTrans.
/// </summary>
/// <remarks>
/// Because verification can never succeed (G-C2), the attack scenarios pass for the wrong reason: nothing is
/// ever verified. They become meaningful once G-C2 is fixed.
/// </remarks>
[TestClass]
public class FanAvaConformanceTests : GatewayConformanceTests
{
    private const string UserId = "merchant";
    private const string Password = "secret";
    private const string Api = BankUrl + "/ref-payment/RestServices/mts";

    protected override string GatewayName => FanAvaGateway.Name;

    protected override bool SupportsRefund => true;

    protected override IReadOnlyDictionary<string, string> KnownIssues => new Dictionary<string, string>
    {
        // The token response's Result is never checked: a rejected request redirects with token=null.
        [nameof(Rejected_request_is_reported_as_failed)] = "G-C2",
        // FanAvaCheckResultModel.IsSucceed is [JsonIgnore], so every inquiry is treated as failed.
        [nameof(Paid_invoice_is_verified_and_recorded_as_paid)] = "G-C2",
        [nameof(Lost_verification_response_is_recovered_when_the_callback_is_retried)] = "G-C2",
        [nameof(Payment_is_verified_on_a_server_with_persian_culture)] = "G-C2",
        [nameof(Payment_is_verified_on_a_server_with_german_culture)] = "G-C2",
        [nameof(Full_refund_reaches_the_bank)] = "G-C2",
        [nameof(Partial_refund_is_not_silently_turned_into_a_full_refund)] = "G-C2",
        // The inquiry response is deserialised without checking the HTTP status.
        [nameof(Html_error_page_during_verification_does_not_crash_or_mark_the_payment_as_paid)] = "G-H9"
    };

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddFanAva()
                .WithAccounts(accounts => accounts.AddInMemory(account =>
                {
                    account.UserId = UserId;
                    account.Password = Password;
                }))
                .WithOptions(options =>
                {
                    options.ApiTokenGenerationUrl = $"{Api}/generateTokenWithNoSign";
                    options.PaymentPageUrl = $"{BankUrl}/_ipgw_/payment";
                    options.ApiCheckPaymentUrl = $"{Api}/inquiryMerchantToken";
                    options.ApiVerificationUrl = $"{Api}/verifyMerchantTrans";
                    options.ApiRefundUrl = $"{Api}/reverseMerchantTrans";
                });

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UseFanAva();

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When($"{Api}/generateTokenWithNoSign").Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            if (call.Reject || !IsMerchant(call.Json)) return FakeBank.Json(new { Result = "erAAS_InvalidUseridOrPass" });

            var transaction = Bank.Open(
                long.Parse((string)call.Json["ReserveNum"]!, CultureInfo.InvariantCulture),
                (decimal)call.Json["Amount"]!,
                (string?)call.Json["RedirectUrl"]);

            return FakeBank.Json(new { Result = "erSucceed", Token = transaction.Token, ExpirationDate = "", ChannelId = "1", UserId });
        }));

        http.When($"{Api}/inquiryMerchantToken").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            var transaction = Bank.FindByToken((string?)call.Json["Token"]);

            if (call.Reject || !IsMerchant(call.Json) || transaction is not { Paid: true })
            {
                return FakeBank.Json(new { Result = "erMts_InvalidToken" });
            }

            return FakeBank.Json(new { Result = "erSucceed", ReferenceNum = transaction.Reference, Amount = transaction.Amount });
        }));

        http.When($"{Api}/verifyMerchantTrans").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            var transaction = Bank.FindByToken((string?)call.Json["Token"]);
            var amount = call.Json["Amount"] is JValue value ? (decimal?)value : null;

            if (call.Reject || !IsMerchant(call.Json) || transaction is not { Paid: true } ||
                (string?)call.Json["RefNum"] != transaction.Reference || amount != transaction.Amount)
            {
                return FakeBank.Json(new { Result = "erMts_InvalidAmount" });
            }

            transaction.Verified = true;

            return FakeBank.Json(new { Result = "erSucceed", Amount = transaction.Amount.ToString("0", CultureInfo.InvariantCulture), RefNum = transaction.Reference });
        }));

        http.When($"{Api}/reverseMerchantTrans").Respond(Bank.Endpoint(BankOperation.Refund, call =>
        {
            var transaction = Bank.FindByToken((string?)call.Json["Token"]);

            if (!IsMerchant(call.Json) || transaction is not { Verified: true } || transaction.RefundedAmount > 0)
            {
                return FakeBank.Json(new { Result = "erMts_InvalidToken" });
            }

            transaction.RefundedAmount = transaction.Amount;

            return FakeBank.Json(new { Result = "erSucceed", Amount = transaction.Amount.ToString("0", CultureInfo.InvariantCulture), RefNum = transaction.Reference });
        }));
    }

    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        TestCallback.Post(
            ("State", claims.Paid ? "ok" : "CanceledByUser"),
            ("RefNum", claims.Paid ? transaction.Reference : ""),
            ("ResNum", claims.OrderId.ToString(CultureInfo.InvariantCulture)),
            ("token", claims.Token ?? transaction.Token));

    private static bool IsMerchant(JObject json) =>
        (string?)json["WSContext"]?["UserId"] == UserId && (string?)json["WSContext"]?["Password"] == Password;
}
