using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using NUnit.Framework;
using Parbad.Builder;
using Parbad.Gateway.IdPay.Internal;
using Parbad.Tests.Helpers;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parbad.Gateway.IdPay.Tests
{
    public class IdPayGatewayTests
    {
        [Test]
        public async Task Requesting_And_Verifying_Work()
        {
            const long expectedTrackingNumber = 1;
            const long expectedAmount = 1000;
            const string expectedCallbackUrl = "http://www.mywebsite.com";
            const string apiRequestUrl = "http://localhost/request";
            const string apiVerificationUrl = "http://localhost/verify";
            const string expectedTransactionCode = "test";
            const string apiKey = "X-API-KEY";

            await GatewayTestHelpers.TestGatewayAsync(
                    gateways =>
                    {
                        return gateways.AddIdPay()
                            .WithAccounts(accounts =>
                            {
                                accounts.AddInMemory(account =>
                                {
                                    account.Api = "test";
                                });
                            })
                            .WithOptions(options =>
                            {
                                options.ApiRequestUrl = apiRequestUrl;
                                options.ApiVerificationUrl = apiVerificationUrl;
                            });
                    },
                    invoice =>
                    {
                        invoice
                            .SetTrackingNumber(expectedTrackingNumber)
                            .SetAmount(expectedAmount)
                            .SetCallbackUrl(expectedCallbackUrl)
                            .UseIdPay();
                    },
                    handler =>
                    {
                        handler
                            .Expect(apiRequestUrl)
                            .WithHeaders(apiKey, "test")
                            .WithJsonBody<IdPayRequestModel>(model =>
                            {
                                var isValid = model.Amount == expectedAmount &&
                                              model.Callback.StartsWith(expectedCallbackUrl) &&
                                              model.OrderId == expectedTrackingNumber;

                                return isValid;
                            })
                            .Respond(MediaTypes.Json, JsonConvert.SerializeObject(new
                            {
                                Link = "test"
                            }));

                        handler
                            .Expect(apiVerificationUrl)
                            .WithHeaders(apiKey, "test")
                            .WithJsonBody<IdPayVerifyModel>(model =>
                            {
                                var isValid = model.Id == "test" &&
                                              model.OrderId == expectedTrackingNumber;

                                return isValid;
                            })
                            .Respond(MediaTypes.Json, JsonConvert.SerializeObject(new
                            {
                                Status = "100",
                                track_id = expectedTransactionCode
                            }));
                    },
                    context =>
                    {
                        context.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                        {
                        {"status", "10"},
                        {"id", "test"},
                        {"track_id", expectedTransactionCode},
                        {"order_id", expectedTrackingNumber.ToString()},
                        {"amount", expectedAmount.ToString()}
                        });
                    },
                    result => GatewayOnResultHelper.OnRequestResult(
                        result,
                        IdPayGateway.Name,
                        GatewayTransporterDescriptor.TransportType.Redirect,
                        expectedPaymentPageUrl: "test"),
                    result => GatewayOnResultHelper.OnFetchResult(result, expectedTrackingNumber, expectedAmount, IdPayGateway.Name),
                result => GatewayOnResultHelper.OnVerifyResult(result, expectedTrackingNumber, expectedAmount, IdPayGateway.Name));
        }
    }
}
