using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Parbad.Abstraction;
using Parbad.Gateway.FanAva.Internal.Models;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.FanAva.Internal
{
    internal static class FanAvaHelper
    {
        internal static FanAvaRequestModel CreateRequestModel(Invoice invoice, FanAvaGatewayAccount account)
        {
            var additionalData = invoice.GetFanAvaAdditionalData();

            string transType;

            if (additionalData == null)
            {
                transType = "EN_GOODS";
            }
            else
            {
                transType = additionalData.Type == FanAvaGatewayAdditionalDataRequestType.Goods
                    ? "EN_GOODS"
                    : "EN_BILL_PAY";
            }

            return new FanAvaRequestModel
            {
                WSContext = new FanAvaRequestModel.WSContextModel
                {
                    UserId = account.UserId,
                    Password = account.Password
                },
                TransType = transType,
                ReserveNum = invoice.TrackingNumber.ToString(),
                Amount = invoice.Amount,
                RedirectUrl = invoice.CallbackUrl,
                Email = additionalData?.Email,
                MobileNo = additionalData?.Email,
                GoodsReferenceId = additionalData?.GoodsReferenceId,
                MerchantGoodReferenceId = additionalData?.MerchantGoodReferenceId,
                ApportionmentAccountList = additionalData?.ApportionmentAccountList
            };
        }

        internal static async Task<PaymentRequestResult> CreateRequestResult(
            HttpResponseMessage responseMessage,
            HttpContext httpContext,
            FanAvaGatewayAccount account,
            FanAvaGatewayOptions gatewayOptions)
        {
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            if (!responseMessage.IsSuccessStatusCode)
            {
                return PaymentRequestResult.Failed(response.ToString(), account.Name);
            }

            var result = JsonConvert.DeserializeObject<FanAvaRequestResultModel>(response);

            var paymentPageUrl = QueryHelper.AddQueryString(gatewayOptions.PaymentPageUrl, new Dictionary<string, string>
            {
                {"token", result.Token},
                {"lang", "fa"}
            });

            return PaymentRequestResult.SucceedWithRedirect(account.Name, httpContext, paymentPageUrl);
        }

        internal static async Task<FanAvaCallbackResultModel> CreateCallbackResult(
            HttpRequest httpRequest,
            MessagesOptions messagesOptions,
            FanAvaGatewayAccount gatewayAccount,
            CancellationToken cancellationToken)
        {
            var state = await httpRequest.TryGetParamAsAsync<string>("State", cancellationToken).ConfigureAwaitFalse();

            if (!state.Exists || state.Value != "ok")
            {
                return new FanAvaCallbackResultModel
                {
                    IsSucceed = false,
                    Message = messagesOptions.InvalidDataReceivedFromGateway
                };
            }

            var invoiceNumber = await httpRequest.TryGetParamAsAsync<string>("RefNum", cancellationToken).ConfigureAwaitFalse();
            var token = await httpRequest.TryGetParamAsAsync<string>("token", cancellationToken).ConfigureAwaitFalse();

            var data = new FanAvaCheckRequestModel
            {
                WSContext = new FanAvaRequestModel.WSContextModel
                {
                    UserId = gatewayAccount.UserId,
                    Password = gatewayAccount.Password
                },
                Token = token.Value
            };

            return new FanAvaCallbackResultModel
            {
                IsSucceed = true,
                InvoiceNumber = invoiceNumber.Value,
                CallbackCheckData = data
            };
        }

        internal static async Task<FanAvaCheckResultModel> CreateCheckResult(HttpResponseMessage responseMessage, FanAvaGatewayAccount account, FanAvaCallbackResultModel callbackResult, MessagesOptions messagesOptions)
        {
            var response = await responseMessage.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<FanAvaCheckResultModel>(response);

            bool isSucceed;
            PaymentVerifyResult verifyResult = null;

            if (!result.IsSucceed)
            {
                isSucceed = false;
                verifyResult = PaymentVerifyResult.Failed(messagesOptions.InvalidDataReceivedFromGateway);
            }
            else
            {
                isSucceed = result.Result.Equals("erSucceed", StringComparison.OrdinalIgnoreCase) &&
                            result.InvoiceNumber == callbackResult.InvoiceNumber;

                if (!isSucceed)
                {
                    verifyResult = PaymentVerifyResult.Failed(messagesOptions.PaymentFailed);
                }
            }

            return new FanAvaCheckResultModel
            {
                IsSucceed = isSucceed,
                VerifyResult = verifyResult
            };
        }

        internal static FanAvaVerifyRequestModel CreateVerifyRequest(InvoiceContext context, FanAvaCallbackResultModel callbackResult, FanAvaCheckResultModel checkResult)
        {
            return new FanAvaVerifyRequestModel
            {
                WSContext = callbackResult.CallbackCheckData.WSContext,
                Token = callbackResult.CallbackCheckData.Token,
                Amount = checkResult.Amount,
                InvoiceNumber = callbackResult.InvoiceNumber
            };
        }

        internal static async Task<IPaymentVerifyResult> CreateVerifyResult(HttpResponseMessage responseMessage, FanAvaCallbackResultModel callbackResult, MessagesOptions messagesOptions)
        {
            var response = await responseMessage.Content.ReadAsStringAsync();

            var verifyResult = JsonConvert.DeserializeObject<FanAvaVerifyResultModel>(response);

            if (!verifyResult.IsSucceed)
            {
                return PaymentVerifyResult.Failed(messagesOptions.InvalidDataReceivedFromGateway);
            }

            return new PaymentVerifyResult
            {
                IsSucceed = verifyResult.Result == "erSucceed",
                Amount = Money.Parse(verifyResult.Amount),
                TrackingNumber = long.Parse(verifyResult.InvoiceNumber),
                Status = PaymentVerifyResultStatus.Succeed
            };
        }

        internal static FanAvaVerifyRequestModel CreateRefundRequest(InvoiceContext context, FanAvaCallbackResultModel callbackResult, FanAvaCheckResultModel checkResult)
        {
            return new FanAvaVerifyRequestModel
            {
                WSContext = callbackResult.CallbackCheckData.WSContext,
                Token = callbackResult.CallbackCheckData.Token,
                Amount = checkResult.Amount,
                InvoiceNumber = callbackResult.InvoiceNumber
            };
        }

        internal static async Task<IPaymentRefundResult> CreateRefundResult(HttpResponseMessage responseMessage, FanAvaCallbackResultModel callbackResult, MessagesOptions messagesOptions)
        {
            var response = await responseMessage.Content.ReadAsStringAsync();

            var refundResult = JsonConvert.DeserializeObject<FanAvaVerifyResultModel>(response);

            if (!refundResult.IsSucceed)
            {
                return PaymentRefundResult.Failed(messagesOptions.InvalidDataReceivedFromGateway);
            }

            return new PaymentRefundResult
            {
                IsSucceed = refundResult.IsSucceed,
                Amount = Money.Parse(refundResult.Amount),
                TrackingNumber = long.Parse(refundResult.InvoiceNumber),
                Status = PaymentRefundResultStatus.Succeed
            };
        }
    }
}
