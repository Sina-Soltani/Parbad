using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
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
            const string apiRequestUrl = "http://localhost/request";
            const string apiVerificationUrl = "http://localhost/verify";
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
                            options.ApiRequestUrl = apiRequestUrl;
                            options.ApiVerificationUrl = apiVerificationUrl;
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
                        .Expect(apiRequestUrl)
                        .Respond("application/json", JsonConvert.SerializeObject(new
                        {
                            ResCode = 0,
                            Token = "test"
                        }));

                    handler
                        .Expect(apiVerificationUrl)
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
                result => GatewayOnResultHelper.OnRequestResult(
                    result,
                    MelliGateway.Name,
                    GatewayTransporterDescriptor.TransportType.Redirect,
                    additionalChecks: () =>
                    {
                        var uri = new Uri(result.GatewayTransporter.Descriptor.Url);
                        var queries = QueryHelpers.ParseQuery(uri.Query);
                        Assert.IsTrue(queries.ContainsKey("token"));
                        Assert.AreEqual("test", (string)queries["token"]);
                    }),
                result => GatewayOnResultHelper.OnFetchResult(result, expectedTrackingNumber, expectedAmount, MelliGateway.Name),
            result => GatewayOnResultHelper.OnVerifyResult(result, expectedTrackingNumber, expectedAmount, MelliGateway.Name));
        }
    }
}
