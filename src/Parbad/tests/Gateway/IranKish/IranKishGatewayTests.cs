using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Parbad.Builder;
using Parbad.Gateway.IranKish;
using Parbad.Tests.Helpers;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Parbad.Gateway.IranKish.Internal.Models;

namespace Parbad.Tests.Gateway.IranKish
{
    public class IranKishGatewayTests
    {
        [Test]
        public async Task Requesting_And_Verifying_Work()
        {
            const long expectedAmount = 1000;
            const string expectedTransactionCode = "test";
            const string expectedToken = "Token";
            const long expectedTrackingNumber = 1;
            const string expectedCallbackUrl = "http://www.mywebsite.com";
            const string apiTokenUrl = "http://localhost/token";
            const string apiVerificationUrl = "http://localhost/verification";
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
                                account.TerminalId = "1234";
                                account.AcceptorId = "4321";
                                account.PassPhrase = "1234567890ABCDEF";
                                account.PublicKey = @"-----BEGIN PUBLIC KEY-----
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDK29P6S5KBONx080Aq+j3jFoA5
Zb9vPwJkCVyfPLCabXt26AghTkYVPaXK2/zLHeGOPoU7Er6kWPlAdN6JC4GHvJ6H
neDEtA+O2B1lQSZnUL2xwrqajq8MeNQckXvwCeAbl4m9Ev7/RPYBeJb16i3xPsjh
fS14GMsP5hVYVy4B7wIDAQAB
-----END PUBLIC KEY-----";
                            });
                        })
                        .WithOptions(options =>
                        {
                            options.ApiTokenUrl = apiTokenUrl;
                            options.ApiVerificationUrl = apiVerificationUrl;
                            options.PaymentPageUrl = paymentPageUrl;
                        });

                    return builder;
                },
                invoice =>
                {
                    invoice
                        .SetTrackingNumber(expectedTrackingNumber)
                        .SetAmount(expectedAmount)
                        .SetCallbackUrl(expectedCallbackUrl)
                        .UseIranKish();
                },
                handler =>
                {
                    handler
                        .Expect(HttpMethod.Post, apiTokenUrl)
                        .Respond(MediaTypes.Json, JsonConvert.SerializeObject(new IranKishTokenResult
                        {
                            ResponseCode = "00",
                            Status = true,
                            Result = new IranKishTokenResultInfo
                            {
                                Token = expectedToken,
                                TransactionType = "Purchase",
                                BillInfo = new IranKishBillInfo()
                            }
                        }));

                    handler
                        .Expect(HttpMethod.Post, apiVerificationUrl)
                        .Respond(MediaTypes.Json, JsonConvert.SerializeObject(new IranKishVerifyResult
                        {
                            ResponseCode = "00",
                            Status = true,
                            Result = new IranKishVerifyResultInfo
                            {
                                ResponseCode = "00",
                                RetrievalReferenceNumber = expectedTransactionCode,
                                Amount = expectedAmount.ToString()
                            }
                        }));
                },
                context =>
                {
                    context.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                    {
                        {"ResponseCode", "00"},
                        {"Token", expectedToken},
                        {"RetrievalReferenceNumber", expectedTransactionCode},
                        {"Amount", expectedAmount.ToString()}
                    });
                },
                result => GatewayOnResultHelper.OnRequestResult(
                    result,
                    IranKishGateway.Name,
                    GatewayTransporterDescriptor.TransportType.Post,
                    expectedPaymentPageUrl: paymentPageUrl,
                    expectedForm: new Dictionary<string, string>
                    {
                        {"tokenIdentity", expectedToken}
                    }),
                result => GatewayOnResultHelper.OnFetchResult(result, expectedTrackingNumber, expectedAmount, IranKishGateway.Name),
                result => GatewayOnResultHelper.OnVerifyResult(result, expectedTrackingNumber, expectedAmount, IranKishGateway.Name, expectedTransactionCode));
        }
    }
}
