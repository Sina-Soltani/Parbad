using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Parbad.Configurations;
using Parbad.Core;
using Parbad.Infrastructure.Translating;
using Parbad.Providers.Melli.Models;
using Parbad.Providers.Melli.ResultTranslator;
using Parbad.Utilities;

namespace Parbad.Providers.Melli
{
    internal class MelliGateway : GatewayBase
    {
        private const string PaymentPageUrl = "https://sadad.shaparak.ir/VPG/Purchase";
        private const string PaymentRequestUrl = "https://sadad.shaparak.ir/VPG/api/v0/Request/PaymentRequest";
        private const string PaymentVerifyUrl = "https://sadad.shaparak.ir/VPG/api/v0/Advice/Verify";
        private const int DuplicateOrderNumberCode = 1011;
        private const int SuccessCode = 0;

        protected MelliGatewayConfiguration Configuration => ParbadConfiguration.Gateways.GetMelliGatewayConfiguration();

        public MelliGateway() : base(Gateway.Melli.ToString())
        {
        }

        public override RequestResult Request(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var data = CreateRequestData(invoice);

            var result = PostJson<MelliApiRequestResult>(PaymentRequestUrl, data);

            return CreateRequestResult(result);
        }

        public override async Task<RequestResult> RequestAsync(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var data = CreateRequestData(invoice);

            var result = await PostJsonAsync<MelliApiRequestResult>(PaymentRequestUrl, data);

            return CreateRequestResult(result);
        }

        public override VerifyResult Verify(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null) throw new ArgumentNullException(nameof(verifyPaymentContext));

            var data = CreateVerifyData(verifyPaymentContext);

            //  Is callback succeed?
            if (!data.Item1)
            {
                // return prepared VerifyResult object (Item4).
                return data.Item4;
            }

            // Send prepared JSON data (Item3)
            var result = PostJson<MelliApiVerifyResult>(PaymentVerifyUrl, data.Item3);

            // Create VerifyResult using the prepared token (Item2)
            return CreateVerifyResult(data.Item2, result);
        }

        public override async Task<VerifyResult> VerifyAsync(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            if (verifyPaymentContext == null) throw new ArgumentNullException(nameof(verifyPaymentContext));

            var data = CreateVerifyData(verifyPaymentContext);

            //  Is callback succeed?
            if (!data.Item1)
            {
                // return prepared VerifyResult object (Item4).
                return data.Item4;
            }

            // Send prepared JSON data (Item3)
            var result = await PostJsonAsync<MelliApiVerifyResult>(PaymentVerifyUrl, data.Item3);

            // Create VerifyResult using the prepared token (Item2)
            return CreateVerifyResult(data.Item2, result);
        }

        public override RefundResult Refund(GatewayRefundPaymentContext refundPaymentContext)
        {
            return new RefundResult(Gateway.Tejarat, 0, RefundResultStatus.Failed, "Gateway Melli does not have Refund operation.");
        }

        public override Task<RefundResult> RefundAsync(GatewayRefundPaymentContext refundPaymentContext)
        {
            return Task.FromResult(Refund(refundPaymentContext));
        }

        private object CreateRequestData(Invoice invoice)
        {
            var signedData = MelliHelper.SignRequestData(Configuration.TerminalId, Configuration.MerchantKey, invoice.OrderNumber, invoice.Amount);

            return new
            {
                TerminalId = Configuration.TerminalId,
                MerchantId = Configuration.MerchantId,
                Amount = invoice.Amount,
                SignData = signedData,
                ReturnUrl = invoice.CallbackUrl,
                LocalDateTime = DateTime.Now,
                OrderId = invoice.OrderNumber
            };
        }

        private static RequestResult CreateRequestResult(MelliApiRequestResult result)
        {
            if (result == null)
            {
                return new RequestResult(RequestResultStatus.Failed, Resource.UnexpectedErrorText);
            }

            string message;

            if (!result.Description.IsNullOrEmpty())
            {
                message = result.Description;
            }
            else
            {
                IGatewayResultTranslator resultTranslator = new MelliRequestResultTranslator();

                message = resultTranslator.Translate(result.ResCode);
            }

            RequestResultStatus status;

            switch (result.ResCode)
            {
                case SuccessCode:
                    status = RequestResultStatus.Success;
                    break;
                case DuplicateOrderNumberCode:
                    status = RequestResultStatus.DuplicateOrderNumber;
                    break;
                default:
                    status = RequestResultStatus.Failed;
                    break;
            }

            if (status != RequestResultStatus.Success)
            {
                return new RequestResult(status, message);
            }

            var paymentPageUrl = GeneratePaymentPageUrl(result.Token);

            return new RequestResult(RequestResultStatus.Success, message, result.Token)
            {
                BehaviorMode = GatewayRequestBehaviorMode.Redirect,
                Content = paymentPageUrl
            };
        }

        /// <summary>
        /// </summary>
        /// <returns>(bool isCallbackSucceed, string token, object jsonDataToVerify, VerifyResult onFailureCallbackResult)</returns>
        private Tuple<bool, string, object, VerifyResult> CreateVerifyData(GatewayVerifyPaymentContext verifyPaymentContext)
        {
            var apiToken = verifyPaymentContext.RequestParameters.GetAs<string>("Token");
            var apiResponseCode = verifyPaymentContext.RequestParameters.GetAs<int>("ResCode");
            var apiOrderId = verifyPaymentContext.RequestParameters.GetAs<long>("OrderId");

            var token = verifyPaymentContext.ReferenceId;

            //  Compare our token and OrderNumber with the token and OrderId, which received from the gateway
            if (apiToken == null || apiToken != token ||
                apiOrderId != verifyPaymentContext.OrderNumber)
            {
                var failureResult = new VerifyResult(Gateway.Melli, token, string.Empty, VerifyResultStatus.NotValid, "Invalid data is received from the gateway");
                return new Tuple<bool, string, object, VerifyResult>(false, apiToken, null, failureResult);
            }

            if (apiResponseCode != SuccessCode)
            {
                var failureResult = new VerifyResult(Gateway.Melli, token, string.Empty, VerifyResultStatus.Failed, "تراکنش انجام نشد.");
                return new Tuple<bool, string, object, VerifyResult>(false, apiToken, null, failureResult);
            }

            var signedData = MelliHelper.SignVerifyData(Configuration.MerchantKey, token);

            var dataToVerify = new
            {
                token = verifyPaymentContext.ReferenceId,
                SignData = signedData
            };

            return new Tuple<bool, string, object, VerifyResult>(true, apiToken, dataToVerify, null);
        }

        private static VerifyResult CreateVerifyResult(string token, MelliApiVerifyResult result)
        {
            if (result == null)
            {
                return new VerifyResult(Gateway.Melli, string.Empty, string.Empty, VerifyResultStatus.Failed, Resource.UnexpectedErrorText);
            }

            string message;

            if (!result.Description.IsNullOrEmpty())
            {
                message = result.Description;
            }
            else
            {
                IGatewayResultTranslator resultTranslator = new MelliVerifyResultTranslator();

                message = resultTranslator.Translate(result.ResCode);
            }

            var status = result.ResCode == SuccessCode ? VerifyResultStatus.Success : VerifyResultStatus.Failed;

            return new VerifyResult(Gateway.Melli, token, result.RetrivalRefNo, status, message);
        }

        private static string GeneratePaymentPageUrl(string token)
        {
            return $"{PaymentPageUrl}/Index?token={token}";
        }

        private static T PostJson<T>(string url, object data)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

            return WebHelper.SendAsJson<T>(url, data, "POST");
        }

        private static Task<T> PostJsonAsync<T>(string url, object data)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

            return WebHelper.SendAsJsonAsync<T>(url, data, "POST");
        }
    }
}