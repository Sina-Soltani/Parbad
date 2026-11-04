using System.Collections.Generic;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.Gateway.PayIr;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;
using Parbad.PaymentFlow.Tests.Infrastructure;
using RichardSzalay.MockHttp;

namespace Parbad.PaymentFlow.Tests.Gateways;

/// <summary>
/// Pay.ir: pg/send → redirect → GET callback (token, status) → pg/verify.
/// </summary>
[TestClass]
public class PayIrConformanceTests : GatewayConformanceTests
{
    private const string ApiKey = "payir-api-key";

    protected override string GatewayName => PayIrGateway.Name;

    protected override bool SupportsRefund => false;

    protected override void AddGateway(IGatewayBuilder gateways) =>
        gateways.AddPayIr()
                .WithAccounts(accounts => accounts.AddInMemory(account => account.Api = ApiKey))
                .WithOptions(options =>
                {
                    options.ApiRequestUrl = $"{BankUrl}/pg/send";
                    options.ApiVerificationUrl = $"{BankUrl}/pg/verify";
                    options.PaymentPageUrl = $"{BankUrl}/pg/";
                });

    protected override void UseGateway(IInvoiceBuilder invoice) => invoice.UsePayIr();

    protected override void SetupBank(MockHttpMessageHandler http)
    {
        http.When($"{BankUrl}/pg/send").Respond(Bank.Endpoint(BankOperation.Request, call =>
        {
            if (call.Reject || (string?)call.Json["api"] != ApiKey)
            {
                return FakeBank.Json(new { status = 0, errorCode = "-3", errorMessage = "The API key is not valid." }, HttpStatusCode.UnprocessableEntity);
            }

            // The request carries no order id unless the optional factorNumber is set.
            var transaction = Bank.Open(0, (decimal)call.Json["amount"]!, (string?)call.Json["redirect"]);

            return FakeBank.Json(new { status = 1, token = transaction.Token });
        }));

        http.When($"{BankUrl}/pg/verify").Respond(Bank.Endpoint(BankOperation.Verify, call =>
        {
            var transaction = Bank.FindByToken((string?)call.Json["token"]);

            if (call.Reject || (string?)call.Json["api"] != ApiKey || transaction is not { Paid: true })
            {
                return FakeBank.Json(new { status = 0, errorCode = "-5", errorMessage = "The transaction is not valid." }, HttpStatusCode.UnprocessableEntity);
            }

            // Assumption (unverified): a repeated verification is answered like the first one.
            transaction.Verified = true;

            return FakeBank.Json(new
            {
                status = 1,
                amount = (long)transaction.Amount,
                transId = transaction.Rrn,
                factorNumber = (string?)null,
                cardNumber = transaction.MaskedCardNumber,
                message = "OK"
            });
        }));
    }

    protected override TestCallback CreateCallback(BankTransaction transaction, CallbackClaims claims) =>
        TestCallback.Get(("token", claims.Token ?? transaction.Token), ("status", claims.Paid ? "1" : "0"));
}
