using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Builder;
using Parbad.Gateway.Melli;
using Parbad.Tests.Helpers;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parbad.Tests.Gateway.Melli
{
    [TestClass]
    public class MelliGatewayTests
    {
        [TestMethod]
        public async Task Requesting_And_Verifying_Work()
        {
            const long expectedTrackingNumber = 1;
            const long expectedAmount = 1000;
            const string expectedCallbackUrl = "http://www.mywebsite.com";
            const string apiUrl = "http://localhost/";
            const string paymentPageUrl = "http://localhost/";

            await GatewayTestHelpers.TestGatewayAsync(
                gateways =>
                {
                    return gateways.AddMelli()
                        .WithAccounts(accounts =>
                        {
                            accounts.AddInMemory(account =>
                            {
                                account.MerchantId = "test";
                                account.TerminalId = "test";
                                account.TerminalKey = "853f31351e51cd9c5222c28e408bf2a3";
                            });
                        })
                        .WithOptions(options =>
                        {
                            options.ApiRequestUrl = apiUrl;
                            options.ApiVerificationUrl = apiUrl;
                            options.PaymentPageUrl = paymentPageUrl;
                        });
                },
                invoice =>
                {
                    invoice
                        .SetTrackingNumber(expectedTrackingNumber)
                        .SetAmount(expectedAmount)
                        .SetCallbackUrl(expectedCallbackUrl)
                        .UseMelli();
                },
                handler =>
                {
                    handler
                        .When("*/PaymentRequest")
                        .Respond("application/json", JsonConvert.SerializeObject(new
                        {
                            ResCode = 0,
                            Token = "test"
                        }));

                    handler
                        .When("*/Verify")
                        .Respond("application/json", JsonConvert.SerializeObject(new
                        {
                            ResCode = 0,
                            Token = "test",
                            Description = "good",
                            RetrivalRefNo = Guid.NewGuid().ToString("N")
                        }));
                },
                context =>
                {
                    context.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                    {
                        {"ResCode", "0"},
                        {"Token", "test"},
                        {"OrderId", "1"}
                    });
                },
                result =>
                {
                    Assert.IsNotNull(result);
                    Assert.IsTrue(result.IsSucceed);
                    Assert.AreEqual(expectedTrackingNumber, result.TrackingNumber);
                    Assert.AreEqual(MelliGateway.Name, result.GatewayName);
                    Assert.AreEqual(GatewayAccount.DefaultName, result.GatewayAccountName);
                    Assert.IsNotNull(result.GatewayTransporter);
                    Assert.IsNotNull(result.GatewayTransporter.Descriptor);
                    Assert.AreEqual(GatewayTransporterDescriptor.TransportType.Redirect, result.GatewayTransporter.Descriptor.Type);
                    Assert.IsNotNull(result.GatewayTransporter.Descriptor.Url);

                    var uri = new Uri(result.GatewayTransporter.Descriptor.Url);
                    var queries = QueryHelpers.ParseQuery(uri.Query);
                    Assert.IsTrue(queries.ContainsKey("token"));
                    Assert.AreEqual("test", (string)queries["token"]);
                },
                result => GatewayOnResultHelper.OnFetchResult(result, expectedTrackingNumber, expectedAmount, MelliGateway.Name),
            result => GatewayOnResultHelper.OnVerifyResult(result, expectedTrackingNumber, expectedAmount, MelliGateway.Name));
        }
    }
}
