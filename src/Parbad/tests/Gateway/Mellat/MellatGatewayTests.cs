using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Parbad.Builder;
using Parbad.Gateway.Mellat;
using Parbad.Tests.Helpers;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parbad.Tests.Gateway.Mellat
{
    [TestClass]
    public class MellatGatewayTests
    {
        private const long ExpectedAmount = 1000;
        private const string ExpectedTransactionCode = "test";
        private const string ExpectedRefId = "token";

        [TestMethod]
        public async Task Requesting_And_Verifying_Work()
        {
            const long expectedTrackingNumber = 1;
            const string expectedCallbackUrl = "http://www.mywebsite.com";
            const string apiUrl = "http://localhost/";
            const string paymentPageUrl = "http://localhost/";

            await GatewayTestHelpers.TestGatewayAsync(
                gateways =>
                {
                    var builder = gateways
                        .AddMellat()
                        .WithAccounts(accounts =>
                        {
                            accounts.AddInMemory(account =>
                            {
                                account.TerminalId = 1;
                                account.UserName = "test";
                                account.UserPassword = "test";
                            });
                        })
                        .WithOptions(options =>
                        {
                            options.ApiUrl = apiUrl;
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
                        .UseMellat();
                },
                handler =>
                {
                    handler
                        .Expect(apiUrl)
                        .WithPartialContent("bpPayRequest")
                        .Respond(MediaTypes.Xml, GetRequestResponse());

                    handler
                        .Expect(apiUrl)
                        .WithPartialContent("bpVerifyRequest")
                        .Respond(MediaTypes.Xml, GetVerificationResponse());

                    handler
                        .Expect(apiUrl)
                        .WithPartialContent("bpSettleRequest")
                        .Respond(MediaTypes.Xml, GetSettleResponse());
                },
                context =>
                {
                    context.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                    {
                        {"ResCode", "0"},
                        {"RefId", "test"},
                        {"SaleReferenceId", ExpectedTransactionCode}
                    });

                    context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
                    {
                        {"RefId", ExpectedRefId}
                    });
                },
                result => GatewayOnResultHelper.OnRequestResult(
                    result,
                    MellatGateway.Name,
                    GatewayTransporterDescriptor.TransportType.Post,
                    expectedPaymentPageUrl: paymentPageUrl,
                    expectedForm: new Dictionary<string, string>
                    {
                        {"RefId", ExpectedRefId}
                    }),
                result => GatewayOnResultHelper.OnFetchResult(result, expectedTrackingNumber, ExpectedAmount, MellatGateway.Name),
                result => GatewayOnResultHelper.OnVerifyResult(result, expectedTrackingNumber, ExpectedAmount, MellatGateway.Name, expectedTransactionCode: ExpectedTransactionCode));
        }

        private static string GetRequestResponse()
        {
            return
                "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                "<soap:Body>" +
                "<ns2:bpPayRequestResponse xmlns:ns2=\"http://interfaces.core.sw.bps.com/\">" +
                "<return>0,token</return>" +
                "</ns2:bpPayRequestResponse>" +
                "</soap:Body>" +
                "</soap:Envelope>";
        }

        private static string GetVerificationResponse()
        {
            return
                "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                "<soap:Body>" +
                "<ns2:bpVerifyRequestResponse xmlns:ns2=\"http://interfaces.core.sw.bps.com/\">" +
                "<return>0</return>" +
                "</ns2:bpVerifyRequestResponse>" +
                "</soap:Body>" +
                "</soap:Envelope>";
        }

        private static string GetSettleResponse()
        {
            return
                "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                "<soap:Body>" +
                "<ns2:bpSettleRequestResponse xmlns:ns2=\"http://interfaces.core.sw.bps.com/\">" +
                "<return>0</return>" +
                "</ns2:bpSettleRequestResponse>" +
                "</soap:Body>" +
                "</soap:Envelope>";
        }
    }
}
