using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Parbad.Builder;
using Parbad.Gateway.AsanPardakht;
using Parbad.Tests.Helpers;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parbad.Tests.Gateway.AsanPardakht
{
    [TestClass]
    public class AsanPardakhtGatewayTests
    {
        private IAsanPardakhtCrypto _crypto;

        private const long ExpectedAmount = 1000;
        private const string ExpectedTransactionCode = "test";

        [TestInitialize]
        public void Initialize()
        {
            var mockCrypto = new Mock<IAsanPardakhtCrypto>();
            mockCrypto
                .Setup(crypto => crypto.Encrypt(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("test");

            var decryptedValue = $"{ExpectedAmount},0,test,0,message,test,{ExpectedTransactionCode},test";

            mockCrypto
                .Setup(crypto => crypto.Decrypt(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(decryptedValue);

            _crypto = mockCrypto.Object;
        }

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
                        .AddAsanPardakht()
                        .WithAccounts(accounts =>
                        {
                            accounts.AddInMemory(account =>
                            {
                                account.UserName = "test";
                                account.Password = "test";
                                account.Key = "QWxsLWluLTEgdG9vbHMgb24gdGhlIEludGVybmV0";
                                account.IV = "QWxsLWluLTEgdG9vbHMgb24gdGhlIEludGVybmV0";
                                account.MerchantConfigurationId = "test";
                            });
                        })
                        .WithOptions(options =>
                        {
                            options.ApiUrl = apiUrl;
                            options.PaymentPageUrl = paymentPageUrl;
                        });

                    builder.Services.RemoveAll<IAsanPardakhtCrypto>();

                    builder.Services.AddSingleton(_crypto);

                    return builder;
                },
                invoice =>
                {
                    invoice
                        .SetTrackingNumber(expectedTrackingNumber)
                        .SetAmount(ExpectedAmount)
                        .SetCallbackUrl(expectedCallbackUrl)
                        .UseAsanPardakht();
                },
                handler =>
                {
                    handler
                        .Expect(apiUrl)
                        .WithPartialContent("RequestOperation")
                        .Respond(MediaTypes.Xml, GetRequestResponse(expectedRefId));

                    handler
                        .Expect(apiUrl)
                        .WithPartialContent("RequestVerification")
                        .Respond(MediaTypes.Xml, GetVerificationResponse());

                    handler
                        .Expect(apiUrl)
                        .WithPartialContent("RequestReconciliation")
                        .Respond(MediaTypes.Xml, GetSettleResponse());
                },
                context =>
                {
                    context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
                    {
                        {"ReturningParams", "test"}
                    });
                },
                result => GatewayOnResultHelper.OnRequestResult(
                    result,
                    AsanPardakhtGateway.Name,
                    GatewayTransporterDescriptor.TransportType.Post,
                    expectedPaymentPageUrl: paymentPageUrl,
                    expectedForm: new Dictionary<string, string>
                    {
                        {"RefId", expectedRefId}
                    }),
                result => GatewayOnResultHelper.OnFetchResult(result, expectedTrackingNumber, ExpectedAmount, AsanPardakhtGateway.Name),
                result => GatewayOnResultHelper.OnVerifyResult(result, expectedTrackingNumber, ExpectedAmount, AsanPardakhtGateway.Name));
        }

        private static string GetRequestResponse(string refId)
        {
            return
                "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
                "<soap:Body>" +
                "<RequestOperationResponse xmlns=\"http://tempuri.org/\">" +
                $"<RequestOperationResult>0,{refId}</RequestOperationResult>" +
                "</RequestOperationResponse>" +
                "</soap:Body>" +
                "</soap:Envelope>";
        }

        private static string GetVerificationResponse()
        {
            return
                "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
                "<soap:Body>" +
                "<RequestVerificationResponse xmlns=\"http://tempuri.org/\">" +
                "<RequestVerificationResult>500</RequestVerificationResult>" +
                "</RequestVerificationResponse>" +
                "</soap:Body>" +
                "</soap:Envelope>";
        }

        private static string GetSettleResponse()
        {
            return
                "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
                "<soap:Body>" +
                "<RequestReconciliationResponse xmlns=\"http://tempuri.org/\">" +
                "<RequestReconciliationResult>600</RequestReconciliationResult>" +
                "</RequestReconciliationResponse>" +
                "</soap:Body>" +
                "</soap:Envelope>";
        }
    }
}
