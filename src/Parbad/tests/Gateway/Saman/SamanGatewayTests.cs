using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Builder;
using Parbad.Gateway.Saman;
using Parbad.Tests.Helpers;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Parbad.Abstraction;
using Parbad.PaymentTokenProviders;

namespace Parbad.Tests.Gateway.Saman
{
    [TestClass]
    public class SamanGatewayTests
    {
        public const string MobileGatewayKey = "UseMobileGateway";
        public const string AdditionalVerificationDataKey = "SamanAdditionalVerificationData";
        public const string BaseServiceUrl = "https://sep.shaparak.ir/";
        public static string PaymentPageUrl => $"{BaseServiceUrl}payment.aspx";
        public const string WebServiceUrl = "/payments/referencepayment.asmx";
        public const string TokenWebServiceUrl = "/payments/initpayment.asmx";
        public const string MobilePaymentTokenUrl = "/MobilePG/MobilePayment";
        public static string MobilePaymentPageUrl => $"{BaseServiceUrl}OnlinePG/OnlinePG";
        public const string MobileVerifyPaymentUrl = "https://verify.sep.ir/Payments/ReferencePayment.asmx";
        public const string FormRedirectUrlKey = "RedirectURL";

        [TestMethod]
        public async Task Saman_WebGateway_Works()
        {
            const string expectedMerchantId = "test";
            const long expectedTrackingNumber = 1;
            const long expectedAmount = 1000;
            const string expectedCallbackUrl = "http://www.mywebsite.com";
            const string expectedTransactionCode = "test";

            await GatewayTestHelpers.TestGatewayAsync(
                    handler =>
                    {
                        handler
                            .Expect(TokenWebServiceUrl)
                            .WithPartialContent("RequestToken")
                            .WithPartialContent(expectedMerchantId)
                            .Respond(MediaTypes.Xml, GetTokenResponse());

                        handler
                            .Expect(WebServiceUrl)
                            .WithPartialContent("verifyTransaction")
                            .Respond(MediaTypes.Xml, GetVerificationResponse());
                    },
                    context =>
                    {
                        context.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                        {
                        {"state", "OK"},
                        {"ResNum", expectedTransactionCode},
                        {"RefNum", "test"}
                        });
                    },
                    gateways =>
                    {
                        return gateways
                            .AddSaman()
                            .WithAccounts(accounts =>
                            {
                                accounts.AddInMemory(account =>
                                {
                                    account.MerchantId = expectedMerchantId;
                                    account.Password = "test";
                                });
                            });
                    },
                    invoice =>
                    {
                        invoice
                            .SetTrackingNumber(expectedTrackingNumber)
                            .SetAmount(expectedAmount)
                            .SetCallbackUrl(expectedCallbackUrl)
                            .UseSaman();
                    },
                    result =>
                    {
                        Assert.IsNotNull(result);
                        Assert.IsTrue(result.IsSucceed);
                        Assert.AreEqual(SamanGateway.Name, result.GatewayName);
                        Assert.IsNotNull(result.GatewayTransporter);
                        Assert.IsNotNull(result.GatewayTransporter.Descriptor);
                        Assert.AreEqual(GatewayTransporterDescriptor.TransportType.Post, result.GatewayTransporter.Descriptor.Type);
                        Assert.IsNotNull(result.GatewayTransporter.Descriptor.Url);
                        Assert.IsNotNull(result.GatewayTransporter.Descriptor.Form);

                        Assert.AreEqual(PaymentPageUrl, result.GatewayTransporter.Descriptor.Url);

                        var expectedForm = new Dictionary<string, string>
                        {
                        {"Token", "test"},
                        {FormRedirectUrlKey, expectedCallbackUrl}
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

                            if (form.Key == FormRedirectUrlKey)
                            {
                                Assert.IsNotNull(form);
                                Assert.IsTrue(form.Value.StartsWith(item.Value));
                                var url = new Uri(form.Value);
                                var query = QueryHelpers.ParseQuery(url.Query);
                                Assert.IsTrue(query.ContainsKey(QueryStringPaymentTokenOptions.DefaultQueryName));
                                Assert.IsNotNull(query[QueryStringPaymentTokenOptions.DefaultQueryName]);
                            }
                        }
                    },
                    result =>
                    {
                        Assert.IsNotNull(result);
                        Assert.IsTrue(result.IsSucceed);
                        Assert.AreEqual(expectedTrackingNumber, result.TrackingNumber);
                        Assert.AreEqual(SamanGateway.Name, result.GatewayName);
                        Assert.AreEqual(GatewayAccount.DefaultName, result.GatewayAccountName);
                        Assert.AreEqual(expectedAmount, (long)result.Amount);
                        Assert.IsFalse(result.IsAlreadyVerified);
                        Assert.AreEqual(PaymentFetchResultStatus.ReadyForVerifying, result.Status);
                    },
                    result =>
                    {
                        Assert.IsNotNull(result);
                        Assert.IsTrue(result.IsSucceed);
                        Assert.AreEqual(expectedTrackingNumber, result.TrackingNumber);
                        Assert.AreEqual(SamanGateway.Name, result.GatewayName);
                        Assert.AreEqual(GatewayAccount.DefaultName, result.GatewayAccountName);
                        Assert.AreEqual(expectedAmount, (long)result.Amount);
                        Assert.AreEqual(PaymentVerifyResultStatus.Succeed, result.Status);
                        Assert.IsNotNull(result.TransactionCode);
                        Assert.AreEqual(expectedTransactionCode, result.TransactionCode);
                    });
        }

        [TestMethod]
        public async Task Saman_MobileGateway_Works()
        {
            const string expectedMerchantId = "test";
            const long expectedTrackingNumber = 1;
            const long expectedAmount = 1000;
            const string expectedCallbackUrl = "http://www.mywebsite.com";
            const string expectedTransactionCode = "test";

            await GatewayTestHelpers.TestGatewayAsync(
                    handler =>
                    {
                        handler
                            .Expect(MobilePaymentTokenUrl)
                            .WithExactFormData(new[]
                            {
                                new KeyValuePair<string, string>()
                            })
                            .WithPartialContent(expectedMerchantId)
                            .Respond(MediaTypes.Json, GetTokenResponse());

                        handler
                            .Expect(MobileVerifyPaymentUrl)
                            .WithPartialContent("verifyTransaction")
                            .Respond(MediaTypes.Json, GetVerificationResponse());
                    },
                    context =>
                    {
                        context.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                        {
                        {"state", "OK"},
                        {"ResNum", expectedTransactionCode},
                        {"RefNum", "test"}
                        });
                    },
                    gateways =>
                    {
                        return gateways
                            .AddSaman()
                            .WithAccounts(accounts =>
                            {
                                accounts.AddInMemory(account =>
                                {
                                    account.MerchantId = expectedMerchantId;
                                    account.Password = "test";
                                });
                            });
                    },
                    invoice =>
                    {
                        invoice
                            .SetTrackingNumber(expectedTrackingNumber)
                            .SetAmount(expectedAmount)
                            .SetCallbackUrl(expectedCallbackUrl)
                            .EnableSamanMobileGateway()
                            .UseSaman();
                    },
                    result =>
                    {
                        Assert.IsNotNull(result);
                        Assert.IsTrue(result.IsSucceed);
                        Assert.AreEqual(SamanGateway.Name, result.GatewayName);
                        Assert.IsNotNull(result.GatewayTransporter);
                        Assert.IsNotNull(result.GatewayTransporter.Descriptor);
                        Assert.AreEqual(GatewayTransporterDescriptor.TransportType.Post, result.GatewayTransporter.Descriptor.Type);
                        Assert.IsNotNull(result.GatewayTransporter.Descriptor.Url);
                        Assert.IsNotNull(result.GatewayTransporter.Descriptor.Form);

                        Assert.AreEqual(MobilePaymentPageUrl, result.GatewayTransporter.Descriptor.Url);

                        var expectedForm = new Dictionary<string, string>
                        {
                            {"Token", "test"}
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
                    result =>
                    {
                        Assert.IsNotNull(result);
                        Assert.IsTrue(result.IsSucceed);
                        Assert.AreEqual(expectedTrackingNumber, result.TrackingNumber);
                        Assert.AreEqual(SamanGateway.Name, result.GatewayName);
                        Assert.AreEqual(GatewayAccount.DefaultName, result.GatewayAccountName);
                        Assert.AreEqual(expectedAmount, (long)result.Amount);
                        Assert.IsFalse(result.IsAlreadyVerified);
                        Assert.AreEqual(PaymentFetchResultStatus.ReadyForVerifying, result.Status);
                    },
                    result =>
                    {
                        Assert.IsNotNull(result);
                        Assert.IsTrue(result.IsSucceed);
                        Assert.AreEqual(expectedTrackingNumber, result.TrackingNumber);
                        Assert.AreEqual(SamanGateway.Name, result.GatewayName);
                        Assert.AreEqual(GatewayAccount.DefaultName, result.GatewayAccountName);
                        Assert.AreEqual(expectedAmount, (long)result.Amount);
                        Assert.AreEqual(PaymentVerifyResultStatus.Succeed, result.Status);
                        Assert.IsNotNull(result.TransactionCode);
                        Assert.AreEqual(expectedTransactionCode, result.TransactionCode);
                    });
        }

        private static string GetTokenResponse()
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
