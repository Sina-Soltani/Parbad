using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Parbad.Builder;
using Parbad.Gateway.IranKish;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// Iran Kish (IKC API v3): tokenization/make with an RSA/AES authentication envelope → POST to the payment
/// page → POST callback → confirmation/purchase.
/// </summary>
[TestClass]
public class IranKishConformanceTests : GatewayConformanceTests
{
    private const string TerminalId = "08000001";
    private const string AcceptorId = "992180008000001";
    private const string PassPhrase = "0123456789ABCDEF0123456789ABCDEF";

    private static readonly RSA BankKey = RSA.Create(2048);

    protected override string GatewayName => IranKishGateway.Name;

    protected override bool SupportsRefund => false;

    protected override IReadOnlyDictionary<string, string> KnownIssues => new Dictionary<string, string>
    {
        // CreateVerifyResult reads result.Result.ResponseCode although Result is null on errors.
        [nameof(Bank_rejection_during_verification_is_recorded_as_failed)] = "G-H9",
        [nameof(Html_error_page_during_verification_does_not_crash_or_mark_the_payment_as_paid)] = "G-H9",
        // Token, RRN and STAN come from the callback; the confirmation's Amount and PaymentId are not compared.
        [nameof(Payment_of_a_cheaper_invoice_cannot_settle_this_invoice)] = "G-C1",
        [nameof(Payment_of_another_invoice_with_the_same_amount_cannot_settle_this_invoice)] = "G-C1"
    };

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddIranKish()
                .WithAccounts(accounts => accounts.AddInMemory(account =>
                {
                    account.TerminalId = TerminalId;
                    account.AcceptorId = AcceptorId;
                    account.PassPhrase = PassPhrase;
                    account.PublicKey = BankKey.ExportSubjectPublicKeyInfoPem();
                }))
                .WithOptions(options =>
                {
                    options.PaymentPageUrl = $"{BankUrl}/iuiv3/IPG/Index";
                    options.ApiTokenUrl = $"{BankUrl}/api/v3/tokenization/make";
                    options.ApiVerificationUrl = $"{BankUrl}/api/v3/confirmation/purchase";
                });

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UseIranKish();

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When($"{BankUrl}/api/v3/tokenization/make").Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            var request = (JObject)call.Json["Request"]!;
            var amount = (long)request["Amount"]!;

            if (call.Reject || (string?)request["TerminalId"] != TerminalId || (string?)request["AcceptorId"] != AcceptorId)
            {
                return FakeBank.Json(new { ResponseCode = "5", Description = "Invalid terminal.", Status = false, Result = (object?)null });
            }

            if (!IsValidEnvelope((JObject)call.Json["AuthenticationEnvelope"]!, amount))
            {
                return FakeBank.Json(new { ResponseCode = "933", Description = "Invalid authentication envelope.", Status = false, Result = (object?)null });
            }

            var transaction = Bank.Open(long.Parse((string)request["PaymentId"]!, CultureInfo.InvariantCulture), amount, (string?)request["RevertUri"]);

            return FakeBank.Json(new
            {
                ResponseCode = "00",
                Description = "",
                Status = true,
                Result = new { Token = transaction.Token, InitiateTimeStamp = 0, ExpiryTimeStamp = 0, TransactionType = "Purchase" }
            });
        }));

        http.When($"{BankUrl}/api/v3/confirmation/purchase").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            var transaction = Bank.FindByToken((string?)call.Json["TokenIdentity"]);

            if (call.Reject || (string?)call.Json["TerminalId"] != TerminalId || transaction is not { Paid: true } ||
                (string?)call.Json["RetrievalReferenceNumber"] != transaction.Rrn ||
                (string?)call.Json["SystemTraceAuditNumber"] != transaction.TraceNumber)
            {
                return FakeBank.Json(new { ResponseCode = "5", Description = "The transaction is not valid.", Status = false, Result = (object?)null });
            }

            // Assumption (unverified): a repeated confirmation is answered like the first one.
            transaction.Verified = true;

            return FakeBank.Json(new
            {
                ResponseCode = "00",
                Description = "",
                Status = true,
                Result = new
                {
                    ResponseCode = "00",
                    SystemTraceAuditNumber = transaction.TraceNumber,
                    RetrievalReferenceNumber = transaction.Rrn,
                    PaymentId = transaction.OrderId.ToString(CultureInfo.InvariantCulture),
                    Amount = transaction.Amount.ToString("0", CultureInfo.InvariantCulture)
                }
            });
        }));
    }

    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        TestCallback.Post(
            ("Token", transaction.Token),
            ("AcceptorId", AcceptorId),
            ("ResponseCode", claims.Paid ? "00" : "17"), // 17 = cancelled by the user
            ("PaymentId", claims.OrderId.ToString(CultureInfo.InvariantCulture)),
            ("RequestId", "request-id"),
            ("Sha256OfPan", "PAN-HASH"),
            ("RetrievalReferenceNumber", transaction.Rrn),
            ("Amount", claims.Amount.ToString("0", CultureInfo.InvariantCulture)),
            ("MaskedPan", transaction.MaskedCardNumber),
            ("SystemTraceAuditNumber", transaction.TraceNumber));

    /// <summary>
    /// Checks the envelope like the bank: RSA-decrypt (AES key ‖ SHA-256), AES-encrypt the expected base string
    /// with that key and the sent IV, and compare the hashes.
    /// </summary>
    private static bool IsValidEnvelope(JObject envelope, long amount)
    {
        var rsaData = Convert.FromHexString((string)envelope["Data"]!);
        var iv = Convert.FromHexString((string)envelope["Iv"]!);
        var decrypted = BankKey.Decrypt(rsaData, RSAEncryptionPadding.Pkcs1);
        var aesKey = decrypted.Take(16).ToArray();
        var hash = decrypted.Skip(16).ToArray();

        var baseString = TerminalId + PassPhrase + amount.ToString(CultureInfo.InvariantCulture).PadLeft(12, '0') + "00";

        using var aes = Aes.Create();
        aes.Key = aesKey;
        var encrypted = aes.EncryptCbc(Convert.FromHexString(baseString), iv, PaddingMode.PKCS7);

        return SHA256.HashData(encrypted).SequenceEqual(hash);
    }
}
