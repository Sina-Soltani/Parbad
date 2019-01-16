using System;
using System.Security.Cryptography;
using System.Text;
using Parbad.Core;
using Parbad.Infrastructure.Translating;
using Parbad.Providers.Melli.Models;
using Parbad.Providers.Melli.ResultTranslator;
using Parbad.Utilities;

namespace Parbad.Providers.Melli
{
    internal static class MelliHelper
    {
        public const string PaymentPageUrl = "https://sadad.shaparak.ir/VPG/Purchase";
        public const string PaymentRequestUrl = "https://sadad.shaparak.ir/VPG/api/v0/Request/PaymentRequest";
        public const string PaymentVerifyUrl = "https://sadad.shaparak.ir/VPG/api/v0/Advice/Verify";

        private const int SuccessCode = 0;
        private const int DuplicateOrderNumberCode = 1011;

        public static object CreateRequestData(Invoice invoice, MelliGatewayConfiguration configuration)
        {
            var signedData = SignRequestData(configuration.TerminalId, configuration.TerminalKey, invoice.OrderNumber, invoice.Amount);

            return new
            {
                TerminalId = configuration.TerminalId,
                MerchantId = configuration.MerchantId,
                Amount = invoice.Amount,
                SignData = signedData,
                ReturnUrl = invoice.CallbackUrl,
                LocalDateTime = DateTime.Now,
                OrderId = invoice.OrderNumber
            };
        }

        public static RequestResult CreateRequestResult(MelliApiRequestResult result)
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

            var paymentPageUrl = $"{PaymentPageUrl}/Index?token={result.Token}";

            return new RequestResult(RequestResultStatus.Success, message, result.Token)
            {
                BehaviorMode = GatewayRequestBehaviorMode.Redirect,
                Content = paymentPageUrl
            };
        }

        public static MelliCallbackResult CreateCallbackResult(GatewayVerifyPaymentContext verifyPaymentContext, MelliGatewayConfiguration configuration)
        {
            var apiResponseCode = verifyPaymentContext.RequestParameters.GetAs<int>("ResCode");

            var token = verifyPaymentContext.ReferenceId;

            if (apiResponseCode != SuccessCode)
            {
                var failureResult = new VerifyResult(Gateway.Melli, token, string.Empty, VerifyResultStatus.Failed, "تراکنش انجام نشد.");

                return new MelliCallbackResult
                {
                    IsSucceed = false,
                    Result = failureResult
                };
            }

            var apiToken = verifyPaymentContext.RequestParameters.GetAs<string>("Token");
            var apiOrderId = verifyPaymentContext.RequestParameters.GetAs<long>("OrderId");

            //  Compare our token and OrderNumber with the token and OrderId, which has received from the gateway
            if (apiToken == null || apiToken != token ||
                apiOrderId != verifyPaymentContext.OrderNumber)
            {
                var failureResult = new VerifyResult(Gateway.Melli, token, string.Empty, VerifyResultStatus.NotValid, "Invalid data is received from the gateway");

                return new MelliCallbackResult
                {
                    IsSucceed = false,
                    Token = apiToken,
                    Result = failureResult
                };
            }

            var signedData = SignVerifyData(configuration.TerminalKey, token);

            var dataToVerify = new
            {
                token = verifyPaymentContext.ReferenceId,
                SignData = signedData
            };

            return new MelliCallbackResult
            {
                IsSucceed = true,
                Token = apiToken,
                JsonDataToVerify = dataToVerify,
                Result = null
            };
        }

        public static VerifyResult CreateVerifyResult(string token, MelliApiVerifyResult result)
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

        private static string SignRequestData(string terminalId, string terminalKey, long orderId, long amount)
        {
            try
            {
                var dataBytes = Encoding.UTF8.GetBytes($"{terminalId};{orderId};{amount}");

                var symmetric = SymmetricAlgorithm.Create("TripleDes");
                symmetric.Mode = CipherMode.ECB;
                symmetric.Padding = PaddingMode.PKCS7;

                var encryptor = symmetric.CreateEncryptor(Convert.FromBase64String(terminalKey), new byte[8]);

                return Convert.ToBase64String(encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length));
            }
            catch (Exception exception)
            {
                throw new MelliGatewayDataSigningException(exception);
            }
        }

        private static string SignVerifyData(string terminalKey, string token)
        {
            try
            {
                var dataBytes = Encoding.UTF8.GetBytes(token);

                var symmetric = SymmetricAlgorithm.Create("TripleDes");
                symmetric.Mode = CipherMode.ECB;
                symmetric.Padding = PaddingMode.PKCS7;

                var encryptor = symmetric.CreateEncryptor(Convert.FromBase64String(terminalKey), new byte[8]);

                return Convert.ToBase64String(encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length));
            }
            catch (Exception exception)
            {
                throw new MelliGatewayDataSigningException(exception);
            }
        }
    }
}
