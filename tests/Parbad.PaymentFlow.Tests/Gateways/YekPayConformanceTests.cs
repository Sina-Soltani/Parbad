using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Parbad.Builder;
using Parbad.Gateway.YekPay;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// YekPay: payment/request → redirect → callback → payment/verify (with the Authority stored at request time).
/// Fetch never inspects the callback, so cancellations are only detected by the bank.
/// </summary>
[TestFixture]
public class YekPayConformanceTests : GatewayConformanceTests
{
    private const string MerchantId = "yekpay-merchant-id";
    private const string Api = BankUrl + "/api/payment";

    protected override string GatewayName => YekPayGateway.Name;

    protected override bool SupportsRefund => false;

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddYekPay()
                .WithAccounts(accounts => accounts.AddInMemory(account => account.MerchantId = MerchantId))
                .WithOptions(options =>
                {
                    options.ApiRequestUrl = $"{Api}/request";
                    options.ApiVerificationUrl = $"{Api}/verify";
                    options.PaymentPageUrl = $"{Api}/start/";
                });

    protected override void UseGateway(IInvoiceBuilder invoice) =>
        invoice.UseYekPay()
               .SetYekPayData(request =>
               {
                   request.FromCurrencyCode = 364; // IRR
                   request.ToCurrencyCode = 978;   // EUR
                   request.FirstName = "Test";
                   request.LastName = "Customer";
                   request.Email = "customer@shop.test";
                   request.Mobile = "+989120000000";
                   request.Description = "Order";
               });

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When($"{Api}/request").Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            if (call.Reject || (string?)call.Json["merchantId"] != MerchantId) return FakeBank.Json(new { Code = -1, Description = "The merchant id is not valid." });

            var transaction = Bank.Open((long)call.Json["orderNumber"]!, (decimal)call.Json["amount"]!, (string?)call.Json["callback"]);

            return FakeBank.Json(new { Code = 100, Description = "Success", Authority = transaction.Token });
        }));

        http.When($"{Api}/verify").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            var transaction = Bank.FindByToken((string?)call.Json["authority"]);

            if (call.Reject || (string?)call.Json["merchantId"] != MerchantId || transaction is not { Paid: true })
            {
                return FakeBank.Json(new { Code = -100, Description = "The payment was not successful." });
            }

            // Assumption (unverified): a repeated verification is answered like the first one.
            transaction.Verified = true;

            return FakeBank.Json(new
            {
                Code = 100,
                Description = "Success",
                Reference = transaction.Rrn,
                Amount = transaction.Amount,
                OrderNumber = transaction.OrderId.ToString(CultureInfo.InvariantCulture)
            });
        }));
    }

    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        TestCallback.Post(("success", claims.Paid ? "1" : "0"), ("authority", claims.Token ?? transaction.Token));
}
