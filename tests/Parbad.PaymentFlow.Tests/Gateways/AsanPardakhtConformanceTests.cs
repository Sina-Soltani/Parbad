using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.Gateway.AsanPardakht;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// AsanPardakht legacy SOAP API: RequestOperation (encrypted through the remote internalutils service) →
/// POST to the payment page → POST callback with encrypted ReturningParams → RequestVerification + RequestReconciliation.
/// </summary>
[TestClass]
public class AsanPardakhtConformanceTests : GatewayConformanceTests
{
    private const string MerchantConfigurationId = "4321";
    private const string UserName = "merchant";
    private const string Password = "secret";
    private const string Key = "MERCHANT-AES-KEY-0123456789ABCDEF";
    private const string IV = "MERCHANT-AES-IV-0123";
    private const string ApiUrl = BankUrl + "/paygate/merchantservices.asmx";
    private const string EncryptUrl = BankUrl + "/paygate/internalutils.asmx?op=EncryptInAES";
    private const string DecryptUrl = BankUrl + "/paygate/internalutils.asmx?op=DecryptInAES";

    private readonly ConcurrentBag<string> _cryptoServiceRequests = new();

    protected override string GatewayName => AsanPardakhtGateway.Name;

    protected override bool SupportsRefund => false;

    protected override IReadOnlyDictionary<string, string> KnownIssues => new Dictionary<string, string>
    {
        // The verification result is translated with int.Parse; an HTML page yields "" and a FormatException.
        [nameof(Html_error_page_during_verification_does_not_crash_or_mark_the_payment_as_paid)] = "G-H9",
        // The local date is formatted with the current culture: under fa-IR it is a Persian date.
        [nameof(Payment_is_verified_on_a_server_with_persian_culture)] = "G-H6"
    };

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddAsanPardakht()
                .WithAccounts(accounts => accounts.AddInMemory(account =>
                {
                    account.MerchantConfigurationId = MerchantConfigurationId;
                    account.UserName = UserName;
                    account.Password = Password;
                    account.Key = Key;
                    account.IV = IV;
                }))
                .WithOptions(options =>
                {
                    options.PaymentPageUrl = $"{BankUrl}/asan/";
                    options.ApiUrl = ApiUrl;
                    options.EncryptUrl = EncryptUrl;
                    options.DecryptUrl = DecryptUrl;
                });

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UseAsanPardakht();

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        // The remote "internalutils" encryption service. Any reversible encoding will do for the simulation.
        http.When(EncryptUrl).Respond(Bank.Endpoint(BankOperation.Other, call =>
        {
            _cryptoServiceRequests.Add(call.Body);

            return Soap("EncryptInAESResponse", "EncryptInAESResult", Encrypt(call.Xml("toBeEncrypted") ?? ""));
        }));

        http.When(DecryptUrl).Respond(Bank.Endpoint(BankOperation.Other, call =>
        {
            _cryptoServiceRequests.Add(call.Body);

            return Soap("DecryptInAESResponse", "DecryptInAESResult", Decrypt(call.Xml("toBeDecrypted") ?? ""));
        }));

        http.When(ApiUrl).WithPartialContent("RequestOperation").Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            // 1,username,password,invoiceId,amount,yyyyMMdd HHmmss,additionalData,callbackUrl,0
            var fields = Decrypt(call.Xml("encryptedRequest") ?? "").Split(',');

            if (call.Reject || call.Xml("merchantConfigurationID") != MerchantConfigurationId ||
                fields.Length != 9 || fields[1] != UserName || fields[2] != Password)
            {
                return Soap("RequestOperationResponse", "RequestOperationResult", "305"); // invalid username or password
            }

            if (!IsTodayInGregorianCalendar(fields[5]))
            {
                return Soap("RequestOperationResponse", "RequestOperationResult", "-100"); // invalid local date
            }

            var transaction = Bank.Open(long.Parse(fields[3], CultureInfo.InvariantCulture), FakeBank.ParseAmount(fields[4]), fields[7]);

            return Soap("RequestOperationResponse", "RequestOperationResult", $"0,{transaction.Token}");
        }));

        http.When(ApiUrl).WithPartialContent("RequestVerification").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            var transaction = Find(call);

            if (call.Reject || transaction == null) return Soap("RequestVerificationResponse", "RequestVerificationResult", "503");
            if (transaction.Verified) return Soap("RequestVerificationResponse", "RequestVerificationResult", "504"); // already requested

            transaction.Verified = true;

            return Soap("RequestVerificationResponse", "RequestVerificationResult", "500");
        }));

        http.When(ApiUrl).WithPartialContent("RequestReconciliation").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            var transaction = Find(call);

            if (call.Reject || transaction is not { Verified: true }) return Soap("RequestReconciliationResponse", "RequestReconciliationResult", "604");
            if (transaction.Data.ContainsKey("settled")) return Soap("RequestReconciliationResponse", "RequestReconciliationResult", "606");

            transaction.Data["settled"] = "true";

            return Soap("RequestReconciliationResponse", "RequestReconciliationResult", "600");
        }));
    }

    /// <summary>
    /// The bank encrypts the callback, so an attacker cannot forge its fields; only <see cref="CallbackClaims.Paid"/>
    /// is taken from the claims. A swapped callback therefore carries the other transaction's real data.
    /// </summary>
    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims)
    {
        // amount,preInvoiceID,token,resCode,messageText,payGateTranID,rrn,lastFourDigitOfPAN
        var returningParams = string.Join(",",
                                          transaction.Amount.ToString("0", CultureInfo.InvariantCulture),
                                          transaction.OrderId.ToString(CultureInfo.InvariantCulture),
                                          transaction.Token,
                                          claims.Paid ? "0" : "1",
                                          claims.Paid ? "" : "Cancelled by the user",
                                          transaction.Reference,
                                          transaction.Rrn,
                                          "1234");

        return TestCallback.Post(("ReturningParams", Encrypt(returningParams)));
    }

    [TestMethod]
    public Task Merchant_encryption_key_is_not_sent_over_the_network() =>
        KnownIssue.ReproducesAsync("G-H4", () => UnderCultureAsync(CultureInfo.InvariantCulture.Name, async () =>
        {
            await PaidInvoiceIsVerifiedAsync();

            var leaking = _cryptoServiceRequests.Count(body => body.Contains(Key) || body.Contains(Password));

            Assert.AreEqual(0, leaking,
                            $"{leaking} calls to the remote encryption service contained the merchant's AES key or password.");
        }));

    private BankTransaction? Find(BankCall call)
    {
        if (call.Xml("merchantConfigurationID") != MerchantConfigurationId) return null;
        if (Decrypt(call.Xml("encryptedCredentials") ?? "") != $"{UserName},{Password}") return null;

        var transaction = Bank.FindByReference(call.Xml("payGateTranID"));

        return transaction is { Paid: true } ? transaction : null;
    }

    private static bool IsTodayInGregorianCalendar(string localDateTime)
    {
        var match = Regex.Match(localDateTime, @"^(\d{8}) \d{6}$");

        return match.Success &&
               DateTime.TryParseExact(match.Groups[1].Value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) &&
               Math.Abs((date - DateTime.Today).TotalDays) <= 1;
    }

    private static string Encrypt(string plain) => "ENC:" + Convert.ToBase64String(Encoding.UTF8.GetBytes(plain));

    private static string Decrypt(string cipher) =>
        cipher.StartsWith("ENC:", StringComparison.Ordinal)
            ? Encoding.UTF8.GetString(Convert.FromBase64String(cipher.Substring(4)))
            : "";

    private static HttpResponseMessage Soap(string response, string result, string value) =>
        FakeBank.Text(
            "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body>" +
            $"<{response} xmlns=\"http://tempuri.org/\"><{result}>{System.Security.SecurityElement.Escape(value)}</{result}></{response}>" +
            "</soap:Body></soap:Envelope>",
            "text/xml");
}
