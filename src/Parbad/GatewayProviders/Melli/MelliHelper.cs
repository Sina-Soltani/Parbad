// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Data.Domain.Payments;
using Parbad.GatewayProviders.Melli.Models;
using Parbad.GatewayProviders.Melli.ResultTranslator;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;

namespace Parbad.GatewayProviders.Melli
{
    internal static class MelliHelper
    {
        public const string PaymentPageUrl = "https://sadad.shaparak.ir/VPG/Purchase";
        public const string BaseServiceUrl = "https://sadad.shaparak.ir/";
        public const string ServiceRequestUrl = "/VPG/api/v0/Request/PaymentRequest";
        public const string ServiceVerifyUrl = "/VPG/api/v0/Advice/Verify";

        private const int SuccessCode = 0;
        private const int DuplicateTrackingNumberCode = 1011;

        public static object CreateRequestData(Invoice invoice, MelliGatewayOptions options)
        {
            var signedData = SignRequestData(options.TerminalId, options.TerminalKey, invoice.TrackingNumber, invoice.Amount);

            return new
            {
                TerminalId = options.TerminalId,
                MerchantId = options.MerchantId,
                Amount = (long)invoice.Amount,
                SignData = signedData,
                ReturnUrl = invoice.CallbackUrl,
                LocalDateTime = DateTime.Now,
                OrderId = invoice.TrackingNumber
            };
        }

        public static PaymentRequestResult CreateRequestResult(
            MelliApiRequestResult result,
            IHttpContextAccessor httpContextAccessor,
            MessagesOptions messagesOptions)
        {
            if (result == null)
            {
                return PaymentRequestResult.Failed(messagesOptions.UnexpectedErrorText);
            }

            var isSucceed = result.ResCode == SuccessCode;

            if (!isSucceed)
            {
                string message;

                if (result.ResCode == DuplicateTrackingNumberCode)
                {
                    message = messagesOptions.DuplicateTrackingNumber;
                }
                else
                {
                    message = !result.Description.IsNullOrEmpty()
                        ? result.Description
                        : MelliRequestResultTranslator.Translate(result.ResCode, messagesOptions);
                }

                return PaymentRequestResult.Failed(message);
            }

            var paymentPageUrl = $"{PaymentPageUrl}/Index?token={result.Token}";

            return PaymentRequestResult.Succeed(new GatewayRedirect(httpContextAccessor, paymentPageUrl));
        }

        public static MelliCallbackResult CreateCallbackResult(Payment payment, HttpRequest httpRequest, MelliGatewayOptions options, MessagesOptions messagesOptions)
        {
            httpRequest.TryGetParamAs<int>("ResCode", out var apiResponseCode);

            if (apiResponseCode != SuccessCode)
            {
                return new MelliCallbackResult
                {
                    IsSucceed = false,
                    Result = PaymentVerifyResult.Failed(messagesOptions.PaymentFailed)
                };
            }

            httpRequest.TryGetParam("Token", out var apiToken);
            httpRequest.TryGetParamAs<long>("OrderId", out var apiOrderId);

            if (apiOrderId != payment.TrackingNumber)
            {
                return new MelliCallbackResult
                {
                    IsSucceed = false,
                    Token = apiToken,
                    Result = PaymentVerifyResult.Failed(messagesOptions.InvalidDataReceivedFromGateway)
                };
            }

            var signedData = SignVerifyData(options.TerminalKey, apiToken);

            var dataToVerify = new
            {
                token = apiToken,
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

        public static PaymentVerifyResult CreateVerifyResult(string token, MelliApiVerifyResult result, MessagesOptions messagesOptions)
        {
            if (result == null)
            {
                return PaymentVerifyResult.Failed(messagesOptions.UnexpectedErrorText);
            }

            string message;

            if (!result.Description.IsNullOrEmpty())
            {
                message = result.Description;
            }
            else
            {
                message = MelliVerifyResultTranslator.Translate(result.ResCode, messagesOptions);
            }

            return new PaymentVerifyResult
            {
                IsSucceed = result.ResCode == SuccessCode,
                TransactionCode = result.RetrivalRefNo,
                Message = message
            };
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
