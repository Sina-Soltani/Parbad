// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Gateway.AsanPardakht.Internal.Models;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parbad.Gateway.AsanPardakht.Internal
{
    internal class AsanPardakhtHelper
    {
        public static string CreateEncryptData(string key, string iv, string input)
        {
            return $@"<soap:Envelope xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
                <soap:Body>
                <EncryptInAES xmlns ='http://tempuri.org/'>
                <aesKey>{key}</aesKey>
                <aesVector>{iv}</aesVector>
                <toBeEncrypted>{input}</toBeEncrypted>
                </EncryptInAES>
                </soap:Body>
                </soap:Envelope>";
        }

        public static string CreateDecryptData(string key, string iv, string input)
        {
            return $@"<soap:Envelope xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
                <soap:Body>
                <DecryptInAES xmlns ='http://tempuri.org/'>
                <aesKey>{key}</aesKey>
                <aesVector>{iv}</aesVector>
                <toBeDecrypted>{input}</toBeDecrypted>
                </DecryptInAES>
                </soap:Body>
                </soap:Envelope>";
        }

        public static async Task<string> CreateRequestData(Invoice invoice, AsanPardakhtGatewayAccount account, IAsanPardakhtCrypto crypto)
        {
            var requestToEncrypt = string.Format("{0},{1},{2},{3},{4},{5:yyyyMMdd hhmmss},{6},{7},{8}",
                1,
                account.UserName,
                account.Password,
                invoice.TrackingNumber,
                invoice.Amount.ToLongString(),
                DateTime.Now,
                "",
                invoice.CallbackUrl,
                "0"
            );

            var encryptedRequest = await crypto.Encrypt(requestToEncrypt, account.Key, account.IV);

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

        public static async Task<AsanPardakhtCallbackResult> CreateCallbackResult(
            InvoiceContext context,
            AsanPardakhtGatewayAccount account,
            HttpRequest httpRequest,
            IAsanPardakhtCrypto crypto,
            MessagesOptions messagesOptions)
        {
            httpRequest.Form.TryGetValue("ReturningParams", out var returningParams);

            var isSucceed = false;
            string message = null;
            string payGateTranId = null;
            string rrn = null;
            string lastFourDigitOfPAN = null;

            if (returningParams.IsNullOrEmpty())
            {
                isSucceed = false;

                message = messagesOptions.InvalidDataReceivedFromGateway;
            }
            else
            {
                var decryptedResult = await crypto.Decrypt(returningParams, account.Key, account.IV);

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
            }

            return new AsanPardakhtCallbackResult
            {
                IsSucceed = isSucceed,
                PayGateTranId = payGateTranId,
                Rrn = rrn,
                LastFourDigitOfPAN = lastFourDigitOfPAN,
                Message = message
            };
        }

        public static async Task<string> CreateVerifyData(
            AsanPardakhtCallbackResult callbackResult,
            AsanPardakhtGatewayAccount account,
            IAsanPardakhtCrypto crypto)
        {
            var requestToEncrypt = account.UserName + "," + account.Password;
            var encryptedRequest = await crypto.Encrypt(requestToEncrypt, account.Key, account.IV);

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
            if (!callbackResult.IsSucceed)
            {
                return new AsanPardakhtVerifyResult
                {
                    IsSucceed = false,
                    Result = PaymentVerifyResult.Failed(callbackResult.Message)
                };
            }

            var result = XmlHelper.GetNodeValueFromXml(response, "RequestVerificationResult", "http://tempuri.org/");

            var isSucceed = result == "500";

            PaymentVerifyResult verifyResult = null;

            if (!isSucceed)
            {
                var message = AsanPardakhtResultTranslator.TranslateVerification(result, messagesOptions);

                verifyResult = PaymentVerifyResult.Failed(message);
                verifyResult.Message = message;
            }

            return new AsanPardakhtVerifyResult
            {
                IsSucceed = isSucceed,
                Result = verifyResult
            };
        }

        public static async Task<string> CreateSettleData(
            AsanPardakhtCallbackResult callbackResult,
            AsanPardakhtGatewayAccount account,
            IAsanPardakhtCrypto crypto)
        {
            var requestToEncrypt = $"{account.UserName},{account.Password}";
            var encryptedRequest = await crypto.Encrypt(requestToEncrypt, account.Key, account.IV);

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
