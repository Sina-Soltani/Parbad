﻿// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Gateway.AsanPardakht.Internal.Models;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Utilities;
using System.Collections.Generic;

namespace Parbad.Gateway.AsanPardakht.Internal
{
    internal static class AsanPardakhtHelper
    {
        //public const string PaymentPageUrl = "https://asan.shaparak.ir/";
        //public const string BaseServiceUrl = "https://services.asanpardakht.net/paygate/merchantservices.asmx";

        public static string CreateRequestData(Invoice invoice, AsanPardakhtGatewayAccount account, IAsanPardakhtCrypto crypto)
        {
            var requestToEncrypt = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                1,
                account.UserName,
                account.Password,
                invoice.TrackingNumber,
                invoice.Amount.ToLongString(),
                "datetime",
                "",
                invoice.CallbackUrl,
                "0"
            );

            var encryptedRequest = crypto.Encrypt(requestToEncrypt, account.Key, account.IV);

            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:RequestOperation>" +
                $"<tem:merchantConfigurationID>{account.MerchantConfigurationId}</tem:merchantConfigurationID>" +
                "<!--Optional:-->" +
                $"<tem:encryptedRequest>{XmlHelper.EncodeXmlValue(encryptedRequest)}</tem:encryptedRequest>" +
                "</tem:RequestOperation>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static PaymentRequestResult CreateRequestResult(
            string response,
            AsanPardakhtGatewayAccount account,
            HttpContext httpContext,
            AsanPardakhtGatewayOptions gatewayOptions,
            MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(response, "RequestOperationResult", "http://tempuri.org/");

            var splitedResult = result.Split(',');

            var isSucceed = splitedResult.Length == 2 && splitedResult[0] == "0";

            if (!isSucceed)
            {
                var message = AsanPardakhtResultTranslator.TranslateRequest(splitedResult[0], messagesOptions);

                return PaymentRequestResult.Failed(message, account.Name);
            }

            return PaymentRequestResult.SucceedWithPost(
                account.Name,
                httpContext,
                gatewayOptions.PaymentPageUrl,
                new Dictionary<string, string>
                {
                    {"RefId", splitedResult[1]}
                });
        }

        public static AsanPardakhtCallbackResult CreateCallbackResult(
            InvoiceContext context,
            AsanPardakhtGatewayAccount account,
            HttpRequest httpRequest,
            IAsanPardakhtCrypto crypto,
            MessagesOptions messagesOptions)
        {
            httpRequest.Form.TryGetValue("ReturningParams", out var returningParams);

            var isSucceed = false;
            PaymentVerifyResult verifyResult = null;
            string payGateTranId = null;
            string rrn = null;
            string lastFourDigitOfPAN = null;

            if (returningParams.IsNullOrEmpty())
            {
                verifyResult = new PaymentVerifyResult
                {
                    TrackingNumber = context.Payment.TrackingNumber,
                    IsSucceed = false,
                    Message = messagesOptions.InvalidDataReceivedFromGateway
                };
            }
            else
            {
                var decryptedResult = crypto.Decrypt(returningParams, account.Key, account.IV);

                var splitedResult = decryptedResult.Split(',');
                
                var amount = splitedResult[0];
                var preInvoiceID = splitedResult[1];
                var token = splitedResult[2];
                var resCode = splitedResult[3];
                var messageText = splitedResult[4];
                payGateTranId = splitedResult[5];
                rrn = splitedResult[6];
                lastFourDigitOfPAN = splitedResult[7];

                isSucceed = resCode == "0" || resCode == "00";
                string message = null;

                if (!isSucceed)
                {
                    message = messageText.IsNullOrEmpty()
                        ? AsanPardakhtResultTranslator.TranslateRequest(resCode, messagesOptions)
                        : messageText;
                }
                else
                {
                    if (long.TryParse(amount, out var longAmount))
                    {
                        if (longAmount != (long)context.Payment.Amount)
                        {
                            isSucceed = false;
                            message = "مبلغ پرداخت شده با مبلغ درخواست شده مطابقت ندارد.";
                        }
                    }
                    else
                    {
                        isSucceed = false;
                        message = "مبلغ پرداخت شده نامشخص است.";
                    }
                }

                if (!isSucceed)
                {
                    verifyResult = new PaymentVerifyResult
                    {
                        Status = PaymentVerifyResultStatus.Failed,
                        TrackingNumber = context.Payment.TrackingNumber,
                        TransactionCode = rrn,
                        Message = message
                    };
                }
            }

            return new AsanPardakhtCallbackResult
            {
                IsSucceed = isSucceed,
                PayGateTranId = payGateTranId,
                Rrn = rrn,
                LastFourDigitOfPAN = lastFourDigitOfPAN,
                Result = verifyResult
            };
        }

        public static string CreateVerifyData(
            AsanPardakhtCallbackResult callbackResult,
            AsanPardakhtGatewayAccount account,
            IAsanPardakhtCrypto crypto)
        {
            var requestToEncrypt = account.UserName + "," + account.Password;
            var encryptedRequest = crypto.Encrypt(requestToEncrypt, account.Key, account.IV);

            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:RequestVerification>" +
                $"<tem:merchantConfigurationID>{account.MerchantConfigurationId}</tem:merchantConfigurationID>" +
                "<!--Optional:-->" +
                $"<tem:encryptedCredentials>{XmlHelper.EncodeXmlValue(encryptedRequest)}</tem:encryptedCredentials>" +
                $"<tem:payGateTranID>{callbackResult.PayGateTranId}</tem:payGateTranID>" +
                "</tem:RequestVerification>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static AsanPardakhtVerifyResult CheckVerifyResult(
            string response,
            AsanPardakhtCallbackResult callbackResult,
            MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(response, "RequestVerificationResult", "http://tempuri.org/");

            var isSucceed = result == "500";

            PaymentVerifyResult verifyResult = null;

            if (!isSucceed)
            {
                var message = AsanPardakhtResultTranslator.TranslateVerification(result, messagesOptions);

                verifyResult = callbackResult.Result;
                verifyResult.Message = message;
            }

            return new AsanPardakhtVerifyResult
            {
                IsSucceed = isSucceed,
                Result = verifyResult
            };
        }

        public static string CreateSettleData(
            AsanPardakhtCallbackResult callbackResult,
            AsanPardakhtGatewayAccount account,
            IAsanPardakhtCrypto crypto)
        {
            var requestToEncrypt = $"{account.UserName},{account.Password}";
            var encryptedRequest = crypto.Encrypt(requestToEncrypt, account.Key, account.IV);

            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<tem:RequestReconciliation>" +
                $"<tem:merchantConfigurationID>{account.MerchantConfigurationId}</tem:merchantConfigurationID>" +
                "<!--Optional:-->" +
                $"<tem:encryptedCredentials>{XmlHelper.EncodeXmlValue(encryptedRequest)}</tem:encryptedCredentials>" +
                $"<tem:payGateTranID>{callbackResult.PayGateTranId}</tem:payGateTranID>" +
                "</tem:RequestReconciliation>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static PaymentVerifyResult CreateSettleResult(
            string response,
            AsanPardakhtCallbackResult callbackResult,
            MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(response, "RequestReconciliationResult", "http://tempuri.org/");

            var isSucceed = result == "600";
            string message;

            if (isSucceed)
            {
                message = messagesOptions.PaymentSucceed;
            }
            else
            {
                message = AsanPardakhtResultTranslator.TranslateReconcilation(result, messagesOptions) ??
                          messagesOptions.PaymentFailed;
            }

            var verifyResult = new PaymentVerifyResult
            {
                Status = isSucceed ? PaymentVerifyResultStatus.Succeed : PaymentVerifyResultStatus.Failed,
                TransactionCode = callbackResult.Rrn,
                Message = message
            };

            verifyResult.DatabaseAdditionalData.Add("PayGateTranId", callbackResult.PayGateTranId);
            verifyResult.DatabaseAdditionalData.Add("LastFourDigitOfPAN", callbackResult.LastFourDigitOfPAN);

            return verifyResult;
        }
    }
}
