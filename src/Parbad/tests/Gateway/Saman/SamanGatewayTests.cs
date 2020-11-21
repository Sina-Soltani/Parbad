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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Parbad.Tests.Gateway.Saman
{
    [TestClass]
    public class SamanGatewayTests
    {
        [TestMethod]
        public async Task Saman_Works()
        {
            await GatewayTestHelpers.TestGatewayAsync(
                "saman",
                gateways =>
                {
                    return gateways
                        .AddSaman()
                        .WithAccounts(accounts =>
                        {
                            accounts.AddInMemory(account =>
                            {
                                account.MerchantId = "test";
                                account.Password = "test";
                            });
                        });
                },
                handler =>
                {
                    var verificationResponseXml = File.ReadAllText(@"d:\verifyresponsexml.txt");

                    handler
                        .When("*/referencepayment.asmx")
                        .WithPartialContent("verifyTransaction")
                        .Respond("text/xml", verificationResponseXml);
                },
                context =>
                {
                    context.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                    {
                        {"state", "OK"},
                        {"ResNum", "test"},
                        {"RefNum", "test"}
                    });
                },
                result =>
                {
                    Assert.IsNotNull(result);
                    Assert.AreEqual(SamanGateway.Name, result.GatewayName);
                    Assert.IsTrue(result.IsSucceed);
                    Assert.IsNotNull(result.GatewayTransporter);
                    Assert.IsNotNull(result.GatewayTransporter.Descriptor);
                    Assert.AreEqual(GatewayTransporterDescriptor.TransportType.Post, result.GatewayTransporter.Descriptor.Type);
                    Assert.IsNotNull(result.GatewayTransporter.Descriptor.Url);
                    Assert.IsNotNull(result.GatewayTransporter.Descriptor.Form);

                    var expectedForm = new Dictionary<string, string>
                    {
                        {"Amount", "1000"},
                        {"MID", "test"},
                        {"ResNum", "1"},
                        {"RedirectURL", "http://www.mysite.com"}
                    };
                    foreach (var item in expectedForm)
                    {
                        if (item.Key == "RedirectURL")
                        {
                            var form = result.GatewayTransporter
                                .Descriptor
                                .Form
                                .SingleOrDefault(_ => _.Key == item.Key);

                            Assert.IsNotNull(form);
                            Assert.IsTrue(form.Value.StartsWith(item.Value));
                            var url = new Uri(form.Value);
                            var query = QueryHelpers.ParseQuery(url.Query);
                            Assert.IsTrue(query.ContainsKey("paymentToken"));
                            Assert.IsNotNull(query["paymentToken"]);
                        }
                        else
                        {
                            Assert.IsTrue(result
                                .GatewayTransporter
                                .Descriptor
                                .Form.Any(_ => _.Key == item.Key && _.Value == item.Value));
                        }
                    }
                },
                result =>
                {
                    Assert.IsNotNull(result);
                    Assert.AreEqual(1, result.TrackingNumber);
                    Assert.AreEqual(SamanGateway.Name, result.GatewayName);
                    Assert.AreEqual("Default", result.GatewayAccountName);
                    Assert.AreEqual(1000, (long)result.Amount);
                    Assert.IsTrue(result.IsSucceed);
                    Assert.IsFalse(result.IsAlreadyVerified);
                    Assert.AreEqual(PaymentFetchResultStatus.ReadyForVerifying, result.Status);
                },
                result =>
                {
                    Assert.IsNotNull(result);
                    Assert.AreEqual(1, result.TrackingNumber);
                    Assert.AreEqual(SamanGateway.Name, result.GatewayName);
                    Assert.AreEqual("Default", result.GatewayAccountName);
                    Assert.AreEqual(1000, (long)result.Amount);
                    Assert.IsTrue(result.IsSucceed);
                    Assert.AreEqual(PaymentVerifyResultStatus.Succeed, result.Status);
                    Assert.IsNotNull(result.TransactionCode);
                    Assert.AreEqual("test", result.TransactionCode);
                });
        }
    }
}
