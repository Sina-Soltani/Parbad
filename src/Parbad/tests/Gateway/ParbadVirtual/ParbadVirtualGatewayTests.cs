using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Parbad.Gateway.ParbadVirtual;
using Parbad.Tests.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parbad.Tests.Gateway.ParbadVirtual
{
    [TestClass]
    public class ParbadVirtualGatewayTests
    {
        private const long ExpectedAmount = 1000;
        private const string ExpectedTransactionCode = "test";

        [TestMethod]
        public async Task Requesting_And_Verifying_Work()
        {
            const long expectedTrackingNumber = 1;
            const string expectedCallbackUrl = "http://www.mywebsite.com";
            const string paymentPageUrl = "/virtual";

            await GatewayTestHelpers.TestGatewayAsync(
                gateways =>
                {
                    var builder = gateways
                        .AddParbadVirtual()
                        .WithOptions(options =>
                        {
                            options.GatewayPath = paymentPageUrl;
                        });

                    return builder;
                },
                invoice =>
                {
                    invoice
                        .SetTrackingNumber(expectedTrackingNumber)
                        .SetAmount(ExpectedAmount)
                        .SetCallbackUrl(expectedCallbackUrl)
                        .UseParbadVirtual();
                },
                handler => { },
                context =>
                {
                    context.Request.Scheme = "http";
                    context.Request.Host = new HostString("localhost", 5000);

                    context.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                    {
                        {"result", "true"},
                        {"TransactionCode", ExpectedTransactionCode}
                    });
                },
                result => GatewayOnResultHelper.OnRequestResult(
                    result,
                    ParbadVirtualGateway.Name,
                    GatewayTransporterDescriptor.TransportType.Post,
                    expectedPaymentPageUrl: $"http://localhost:5000{paymentPageUrl}",
                    expectedForm: new Dictionary<string, string>
                    {
                        {"CommandType", "request"},
                        {"trackingNumber", expectedTrackingNumber.ToString()},
                        {"amount", ExpectedAmount.ToString()}
                    }),
                result => GatewayOnResultHelper.OnFetchResult(result, expectedTrackingNumber, ExpectedAmount, ParbadVirtualGateway.Name),
                result => GatewayOnResultHelper.OnVerifyResult(result, expectedTrackingNumber, ExpectedAmount, ParbadVirtualGateway.Name, ExpectedTransactionCode));
        }
    }
}
