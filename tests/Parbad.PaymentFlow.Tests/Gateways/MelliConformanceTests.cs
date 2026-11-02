using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.Gateway.Melli;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// Melli (Sadad VPG api/v0): PaymentRequest (3DES-signed) → redirect → POST callback → Advice/Verify (signed token).
/// </summary>
[TestClass]
public class MelliConformanceTests : GatewayConformanceTests
{
    private const string TerminalId = "24000615";
    private const string MerchantId = "000000140212149";
    private static readonly byte[] TerminalKeyBytes = Enumerable.Range(1, 24).Select(i => (byte)(i * 7)).ToArray();
    private static readonly string TerminalKey = Convert.ToBase64String(TerminalKeyBytes);

    protected override string GatewayName => MelliGateway.Name;

    protected override bool SupportsRefund => false;

    protected override IReadOnlyDictionary<string, string> KnownIssues => new Dictionary<string, string>
    {
        // The private PostJsonAsync deserialises the body without checking the HTTP status.
        [nameof(Html_error_page_during_verification_does_not_crash_or_mark_the_payment_as_paid)] = "G-H9"
    };

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddMelli()
                .WithAccounts(accounts => accounts.AddInMemory(account =>
                {
                    account.TerminalId = TerminalId;
                    account.MerchantId = MerchantId;
                    account.TerminalKey = TerminalKey;
                }))
                .WithOptions(options =>
                {
                    options.PaymentPageUrl = $"{BankUrl}/VPG/Purchase";
                    options.ApiRequestUrl = $"{BankUrl}/VPG/api/v0/Request/PaymentRequest";
                    options.ApiVerificationUrl = $"{BankUrl}/VPG/api/v0/Advice/Verify";
                });

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UseMelli();

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When($"{BankUrl}/VPG/api/v0/Request/PaymentRequest").Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            var json = call.Json;
            var orderId = (string)json["OrderId"]!;
            var amount = (decimal)json["Amount"]!;
            var expectedSignature = $"{TerminalId};{orderId};{amount.ToString("0", CultureInfo.InvariantCulture)}";

            if (call.Reject || (string?)json["TerminalId"] != TerminalId || (string?)json["MerchantId"] != MerchantId)
            {
                return FakeBank.Json(new { ResCode = 3, Description = "The merchant is not valid." });
            }

            if (Decrypt((string?)json["SignData"]) != expectedSignature)
            {
                return FakeBank.Json(new { ResCode = 1006, Description = "Invalid signature." });
            }

            var transaction = Bank.Open(long.Parse(orderId, CultureInfo.InvariantCulture), amount, (string?)json["ReturnUrl"]);

            return FakeBank.Json(new { ResCode = 0, Token = transaction.Token, Description = "Success" });
        }));

        http.When($"{BankUrl}/VPG/api/v0/Advice/Verify").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            var token = (string?)call.Json["token"];
            var transaction = Bank.FindByToken(token);

            if (call.Reject || transaction is not { Paid: true } || Decrypt((string?)call.Json["SignData"]) != token)
            {
                return FakeBank.Json(new { ResCode = -1, Description = "The transaction is not valid." });
            }

            // Assumption (unverified): Advice/Verify is idempotent and answers a repeated call like the first one.
            transaction.Verified = true;

            return FakeBank.Json(new
            {
                ResCode = 0,
                Amount = (long)transaction.Amount,
                Description = "Success",
                RetrivalRefNo = transaction.Rrn,
                SystemTraceNo = transaction.TraceNumber,
                OrderId = transaction.OrderId
            });
        }));
    }

    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        TestCallback.Post(
            ("Token", claims.Token ?? transaction.Token),
            ("OrderId", claims.OrderId.ToString(CultureInfo.InvariantCulture)),
            ("ResCode", claims.Paid ? "0" : "-1"));

    private static string? Decrypt(string? signData)
    {
        if (string.IsNullOrEmpty(signData)) return null;

        using var tripleDes = TripleDES.Create();
        tripleDes.Key = TerminalKeyBytes;
        tripleDes.Mode = CipherMode.ECB;
        tripleDes.Padding = PaddingMode.PKCS7;

        var cipher = Convert.FromBase64String(signData);

        return Encoding.UTF8.GetString(tripleDes.CreateDecryptor().TransformFinalBlock(cipher, 0, cipher.Length));
    }
}
