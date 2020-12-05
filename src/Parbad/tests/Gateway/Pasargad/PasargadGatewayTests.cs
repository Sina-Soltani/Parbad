using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using Parbad.Builder;
using Parbad.Gateway.Pasargad;
using Parbad.Tests.Helpers;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parbad.Tests.Gateway.Pasargad
{
    public class PasargadGatewayTests
    {
        private IPasargadCrypto _crypto;

        private const long ExpectedTrackingNumber = 1;
        private const long ExpectedAmount = 1000;
        private const string ExpectedTransactionCode = "test";
        private const string ExpectedMerchantCode = "test";
        private const string ExpectedTerminalCode = "test";
        private const string ExpectedActionNumber = "1003";
        private const string ExpectedSignedValue = "test";

        [SetUp]
        public void Setup()
        {
            var mockCrypto = new Mock<IPasargadCrypto>();
            mockCrypto
                .Setup(crypto => crypto.Encrypt(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(ExpectedSignedValue);

            _crypto = mockCrypto.Object;
        }

        [Test]
        public async Task Requesting_And_Verifying_Work()
        {
            const string expectedCallbackUrl = "http://www.mywebsite.com";
            const string apiCheckPaymentUrl = "http://localhost/CheckTransactionResult";
            const string apiVerificationUrl = "http://localhost/VerifyPayment";
            const string paymentPageUrl = "http://localhost/";

            await GatewayTestHelpers.TestGatewayAsync(
                gateways =>
                {
                    var builder = gateways
                        .AddPasargad()
                        .WithAccounts(accounts =>
                        {
                            accounts.AddInMemory(account =>
                            {
                                account.PrivateKey = "test";
                                account.MerchantCode = ExpectedMerchantCode;
                                account.TerminalCode = ExpectedTerminalCode;
                            });
                        })
                        .WithOptions(options =>
                        {
                            options.ApiCheckPaymentUrl = apiCheckPaymentUrl;
                            options.ApiVerificationUrl = apiVerificationUrl;
                            options.PaymentPageUrl = paymentPageUrl;
                        });

                    builder.Services.RemoveAll<IPasargadCrypto>();

                    builder.Services.AddSingleton(_crypto);

                    return builder;
                },
                invoice =>
                {
                    invoice
                        .SetTrackingNumber(ExpectedTrackingNumber)
                        .SetAmount(ExpectedAmount)
                        .SetCallbackUrl(expectedCallbackUrl)
                        .UsePasargad();
                },
                handler =>
                {
                    handler
                        .When("*CheckTransactionResult")
                        .Respond(MediaTypes.Xml, GetCheckCallbackResponse());

                    handler
                        .When("*VerifyPayment")
                        .Respond(MediaTypes.Xml, GetVerificationResponse());
                },
                context =>
                {
                    context.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                    {
                        {"iN", ExpectedTrackingNumber.ToString()},
                        {"iD", "test"},
                        {"tref", ExpectedTransactionCode},
                        {"result", "true"}
                    });
                },
                result => GatewayOnResultHelper.OnRequestResult(
                    result,
                    PasargadGateway.Name,
                    GatewayTransporterDescriptor.TransportType.Post,
                    expectedPaymentPageUrl: paymentPageUrl,
                    expectedForm: new Dictionary<string, string>
                    {
                        {"merchantCode", ExpectedMerchantCode},
                        {"terminalCode", ExpectedTerminalCode},
                        {"invoiceNumber", ExpectedTrackingNumber.ToString()},
                        {"amount", ExpectedAmount.ToString()},
                        {"redirectAddress", expectedCallbackUrl},
                        {"action", ExpectedActionNumber},
                        {"sign", ExpectedSignedValue}
                    }),
                result => GatewayOnResultHelper.OnFetchResult(result, ExpectedTrackingNumber, ExpectedAmount, PasargadGateway.Name),
                result => GatewayOnResultHelper.OnVerifyResult(result, ExpectedTrackingNumber, ExpectedAmount, PasargadGateway.Name, expectedTransactionCode: ExpectedTransactionCode));
        }

        private static string GetCheckCallbackResponse()
        {
            return
                "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                "<soap:Body>" +
                "<ns2:bpPayRequestResponse xmlns:ns2=\"http://interfaces.core.sw.bps.com/\">" +
                $"<invoiceNumber>{ExpectedTrackingNumber}</invoiceNumber>" +
                $"<action>{ExpectedActionNumber}</action>" +
                $"<merchantCode>{ExpectedMerchantCode}</merchantCode>" +
                $"<terminalCode>{ExpectedTerminalCode}</terminalCode>" +
                "<result>true</result>" +
                "</ns2:bpPayRequestResponse>" +
                "</soap:Body>" +
                "</soap:Envelope>";
        }

        private static string GetVerificationResponse()
        {
            return
                "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                "<soap:Body>" +
                "<ns2:bpPayRequestResponse xmlns:ns2=\"http://interfaces.core.sw.bps.com/\">" +
                "<result>true</result>" +
                "</ns2:bpPayRequestResponse>" +
                "</soap:Body>" +
                "</soap:Envelope>";
        }
    }
}
