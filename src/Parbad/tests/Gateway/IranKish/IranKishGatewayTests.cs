using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.Gateway.IranKish;
using Parbad.Tests.Helpers;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parbad.Tests.Gateway.IranKish
{
    [TestClass]
    public class IranKishGatewayTests
    {
        private const string ExpectedMerchantId = "test";
        private const long ExpectedAmount = 1000;
        private const string ExpectedTransactionCode = "test";
        private const string ExpectedToken = "Token";

        [TestMethod]
        public async Task Requesting_And_Verifying_Work()
        {
            const string expectedRefId = "test";
            const long expectedTrackingNumber = 1;
            const string expectedCallbackUrl = "http://www.mywebsite.com";
            const string apiUrl = "http://localhost/";
            const string paymentPageUrl = "http://localhost/";

            await GatewayTestHelpers.TestGatewayAsync(
                gateways =>
                {
                    var builder = gateways
                        .AddIranKish()
                        .WithAccounts(accounts =>
                        {
                            accounts.AddInMemory(account =>
                            {
                                account.MerchantId = ExpectedMerchantId;
                                account.Sha1Key = "testkey";
                            });
                        })
                        .WithOptions(options =>
                        {
                            options.ApiTokenUrl = apiUrl;
                            options.ApiVerificationUrl = apiUrl;
                            options.PaymentPageUrl = paymentPageUrl;
                        });

                    return builder;
                },
                invoice =>
                {
                    invoice
                        .SetTrackingNumber(expectedTrackingNumber)
                        .SetAmount(ExpectedAmount)
                        .SetCallbackUrl(expectedCallbackUrl)
                        .UseIranKish();
                },
                handler =>
                {
                    handler
                        .Expect(apiUrl)
                        .WithPartialContent("MakeToken")
                        .Respond(MediaTypes.Xml, GetRequestResponse());

                    handler
                        .Expect(apiUrl)
                        .WithPartialContent("KicccPaymentsVerification")
                        .Respond(MediaTypes.Xml, GetVerificationResponse());
                },
                context =>
                {
                    context.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                    {
                        {"ResultCode", "100"},
                        {"Token", ExpectedToken},
                        {"MerchantId", ExpectedMerchantId},
                        {"InvoiceNumber", expectedTrackingNumber.ToString()},
                        {"ReferenceId", ExpectedTransactionCode}
                    });
                },
                result =>
                {
                    Assert.IsNotNull(result);
                    Assert.IsTrue(result.IsSucceed);
                    Assert.AreEqual(IranKishGateway.Name, result.GatewayName);
                    Assert.IsNotNull(result.GatewayTransporter);
                    Assert.IsNotNull(result.GatewayTransporter.Descriptor);
                    Assert.AreEqual(GatewayTransporterDescriptor.TransportType.Post, result.GatewayTransporter.Descriptor.Type);
                    Assert.IsNotNull(result.GatewayTransporter.Descriptor.Url);
                    Assert.IsNotNull(result.GatewayTransporter.Descriptor.Form);

                    Assert.AreEqual(paymentPageUrl, result.GatewayTransporter.Descriptor.Url);

                    var expectedForm = new Dictionary<string, string>
                    {
                        {"merchantid", expectedRefId},
                        {"token", ExpectedToken}
                    };

                    foreach (var item in expectedForm)
                    {
                        var form = result
                            .GatewayTransporter
                            .Descriptor
                            .Form
                            .SingleOrDefault(_ => _.Key == item.Key);

                        Assert.IsNotNull(form.Key);
                        Assert.IsNotNull(form.Value);
                    }
                },
                result => GatewayOnResultHelper.OnFetchResult(result, expectedTrackingNumber, ExpectedAmount, IranKishGateway.Name),
                result => GatewayOnResultHelper.OnVerifyResult(result, expectedTrackingNumber, ExpectedAmount, IranKishGateway.Name, ExpectedTransactionCode));
        }

        private static string GetRequestResponse()
        {
            return
                "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                "<s:Body>" +
                "<MakeTokenResponse xmlns=\"http://tempuri.org/\">" +
                "<MakeTokenResult xmlns:a=\"http://schemas.datacontract.org/2004/07/Token\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                "<a:message>909</a:message>" +
                "<a:result>true</a:result>" +
                $"<a:token>{ExpectedToken}</a:token>" +
                "</MakeTokenResult>" +
                "</MakeTokenResponse>" +
                "</s:Body>" +
                "</s:Envelope>";
        }

        private static string GetVerificationResponse()
        {
            return
                "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                "<s:Body>" +
                "<KicccPaymentsVerificationResponse xmlns=\"http://tempuri.org/\">" +
                $"<KicccPaymentsVerificationResult>{ExpectedAmount}</KicccPaymentsVerificationResult>" +
                "</KicccPaymentsVerificationResponse>" +
                "</s:Body>" +
                "</s:Envelope>";
        }
    }
}
