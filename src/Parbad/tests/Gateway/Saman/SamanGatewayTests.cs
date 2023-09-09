using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Parbad.Builder;
using Parbad.Gateway.Saman;
using Parbad.Gateway.Saman.Internal.Models;
using Parbad.Tests.Helpers;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parbad.Tests.Gateway.Saman
{
    [TestClass]
    public class SamanGatewayTests
    {
        private const string ExpectedMerchantId = "test";
        private const long ExpectedTrackingNumber = 1;
        private const long ExpectedAmount = 1000;
        private const string ExpectedTransactionCode = "test";
        private const string FormRedirectUrlKey = "RedirectURL";
        private const string ExpectedCallbackUrl = "http://www.mywebsite.com";
        private const string ApiUrl = "http://localhost/";
        private const string PaymentPageUrl = "http://localhost/";

        [TestMethod]
        public async Task WebGateway_Requesting_And_Verifying_Work()
        {
            await GatewayTestHelpers.TestGatewayAsync(
                gateways =>
                {
                    return gateways
                        .AddSaman()
                        .WithAccounts(accounts =>
                        {
                            accounts.AddInMemory(account =>
                            {
                                account.TerminalId = ExpectedMerchantId;
                                account.Password = "test";
                            });
                        })
                        .WithOptions(options =>
                        {
                            options.WebApiUrl = ApiUrl;
                            options.WebApiTokenUrl = ApiUrl;
                            options.WebPaymentPageUrl = PaymentPageUrl;
                            options.MobileApiTokenUrl = ApiUrl;
                            options.MobileApiVerificationUrl = ApiUrl;
                            options.MobilePaymentPageUrl = ApiUrl;
                        });
                },
                invoice =>
                {
                    invoice
                        .SetTrackingNumber(ExpectedTrackingNumber)
                        .SetAmount(ExpectedAmount)
                        .SetCallbackUrl(ExpectedCallbackUrl)
                        .UseSaman();
                },
                handler =>
                {
                    handler
                        .Expect(ApiUrl)
                        .WithPartialContent("RequestToken")
                        .WithPartialContent(ExpectedMerchantId)
                        .Respond(MediaTypes.Xml, GetWebGatewayTokenResponse());

                    handler
                        .Expect(ApiUrl)
                        .WithPartialContent("verifyTransaction")
                        .Respond(MediaTypes.Xml, GetVerificationResponse());
                },
                context =>
                {
                    context.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                    {
                        {"state", "OK"},
                        {"ResNum", ExpectedTransactionCode},
                        {"RefNum", "test"}
                    });
                },
                result => GatewayOnResultHelper.OnRequestResult(
                    result,
                    SamanGateway.Name,
                    GatewayTransporterDescriptor.TransportType.Post,
                    expectedPaymentPageUrl: PaymentPageUrl,
                    expectedForm: new Dictionary<string, string>
                    {
                        {"Token", "test"},
                        {FormRedirectUrlKey, ExpectedCallbackUrl}
                    }),
                result => GatewayOnResultHelper.OnFetchResult(result, ExpectedTrackingNumber, ExpectedAmount, SamanGateway.Name),
                result => GatewayOnResultHelper.OnVerifyResult(result, ExpectedTrackingNumber, ExpectedAmount, SamanGateway.Name, ExpectedTransactionCode));
        }

        [TestMethod]
        public async Task MobileGateway_Requesting_And_Verifying_Work()
        {
            await GatewayTestHelpers.TestGatewayAsync(
                gateways =>
                {
                    return gateways
                        .AddSaman()
                        .WithAccounts(accounts =>
                        {
                            accounts.AddInMemory(account =>
                            {
                                account.TerminalId = ExpectedMerchantId;
                                account.Password = "test";
                            });
                        })
                        .WithOptions(options =>
                        {
                            options.WebApiUrl = ApiUrl;
                            options.WebApiTokenUrl = ApiUrl;
                            options.WebPaymentPageUrl = PaymentPageUrl;
                            options.MobileApiTokenUrl = ApiUrl;
                            options.MobileApiVerificationUrl = ApiUrl;
                            options.MobilePaymentPageUrl = ApiUrl;
                        });
                },
                invoice =>
                {
                    invoice
                        .SetTrackingNumber(ExpectedTrackingNumber)
                        .SetAmount(ExpectedAmount)
                        .SetCallbackUrl(ExpectedCallbackUrl)
                        .EnableSamanMobileGateway()
                        .UseSaman();
                },
                    handler =>
                    {
                        handler
                            .Expect(ApiUrl)
                            .WithHttpMethod(HttpMethod.Post)
                            .WithJsonBody<SamanMobilePaymentTokenRequest>(model =>
                            {
                                var isModelValid =
                                    model.Amount == ExpectedAmount &&
                                    model.TerminalId == ExpectedMerchantId &&
                                    model.ResNum == ExpectedTrackingNumber.ToString() &&
                                    model.Action == "Token" &&
                                    model.RedirectUrl != null &&
                                    model.RedirectUrl.StartsWith(ExpectedCallbackUrl);

                                return isModelValid;
                            })
                            .Respond(MediaTypes.Json, GetMobileGatewayTokenResponse());

                        handler
                            .Expect(PaymentPageUrl)
                            .WithPartialContent("verifyTransaction")
                            .Respond(MediaTypes.Xml, GetVerificationResponse());
                    },
                    context =>
                    {
                        context.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                        {
                        {"state", "OK"},
                        {"ResNum", ExpectedTransactionCode},
                        {"RefNum", "test"}
                        });
                    },
                result => GatewayOnResultHelper.OnRequestResult(
                    result,
                    SamanGateway.Name,
                    GatewayTransporterDescriptor.TransportType.Post,
                    expectedPaymentPageUrl: PaymentPageUrl,
                    expectedForm: new Dictionary<string, string>
                    {
                        {"Token", "test"}
                    }),
                result => GatewayOnResultHelper.OnFetchResult(result, ExpectedTrackingNumber, ExpectedAmount, SamanGateway.Name),
                result => GatewayOnResultHelper.OnVerifyResult(result, ExpectedTrackingNumber, ExpectedAmount, SamanGateway.Name, ExpectedTransactionCode));
        }

        private static string GetWebGatewayTokenResponse()
        {
            return
                "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:tns=\"urn:Foo\" xmlns:types=\"urn:Foo\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
                "<soap:Body soap:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
                "<types:RequestTokenResponse>" +
                "<result xsi:type=\"xsd:string\">0</result>" +
                "</types:RequestTokenResponse>" +
                "</soap:Body>" +
                "</soap:Envelope>";
        }

        private static string GetMobileGatewayTokenResponse()
        {
            return JsonConvert.SerializeObject(new
            {
                Status = 1,
                Token = "test"
            });
        }

        private static string GetVerificationResponse()
        {
            return
                "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:tns=\"urn:Foo\" xmlns:types=\"urn:Foo\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
                "<soap:Body soap:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
                "<types:verifyTransactionResponse>" +
                "<result xsi:type=\"xsd:double\">1000</result>" +
                "</types:verifyTransactionResponse>" +
                "</soap:Body>" +
                "</soap:Envelope>";
        }
    }
}
