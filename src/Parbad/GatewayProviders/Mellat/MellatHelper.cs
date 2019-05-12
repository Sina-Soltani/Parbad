// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Data.Domain.Payments;
using Parbad.GatewayProviders.Mellat.Models;
using Parbad.Http;
using Parbad.Utilities;
using Parbad.Internal;
using Parbad.Options;

namespace Parbad.GatewayProviders.Mellat
{
    internal static class MellatHelper
    {
        private const string OkResult = "0";
        private const string DuplicateOrderNumberResult = "41";
        private const string AlreadyVerifiedResult = "43";
        private const string SettleSuccess = "45";

        internal const string CumulativeAccountsKey = "MellatCumulativeAccounts";

        public const string PaymentPageUrl = "https://bpm.shaparak.ir/pgwchannel/startpay.mellat";
        public const string BaseServiceUrl = "https://bpm.shaparak.ir/";
        public const string WebServiceUrl = "/pgwchannel/services/pgw";
        public const string TestWebServiceUrl = "/pgwchannel/services/pgwtest";

        public static string CreateRequestData(Invoice invoice, MellatGatewayOptions options)
        {
            if (invoice.AdditionalData == null || !invoice.AdditionalData.ContainsKey(CumulativeAccountsKey))
            {
                return CreateSimpleRequestData(invoice, options);
            }

            return CreateCumulativeRequestData(invoice, options);
        }

        public static PaymentRequestResult CreateRequestResult(string webServiceResponse, IHttpContextAccessor httpContextAccessor, MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "return");

            var arrayResult = result.Split(',');

            var resCode = arrayResult[0];
            var refId = arrayResult.Length > 1 ? arrayResult[1] : string.Empty;

            var isSucceed = resCode == OkResult;

            if (!isSucceed)
            {
                var message = resCode == DuplicateOrderNumberResult
                    ? messagesOptions.DuplicateTrackingNumber
                    : MellatGatewayResultTranslator.Translate(resCode, messagesOptions);

                return PaymentRequestResult.Failed(message);
            }

            var transporter = new GatewayPost(
                httpContextAccessor,
                PaymentPageUrl,
                new Dictionary<string, string>
                {
                    {"RefId", refId}
                });

            return PaymentRequestResult.Succeed(transporter);
        }

        public static MellatCallbackResult CrateCallbackResult(HttpRequest httpRequest, MessagesOptions messagesOptions)
        {
            httpRequest.TryGetParam("ResCode", out var resCode);

            if (resCode.IsNullOrEmpty())
            {
                return new MellatCallbackResult
                {
                    IsSucceed = false,
                    Result = PaymentVerifyResult.Failed(messagesOptions.InvalidDataReceivedFromGateway)
                };
            }

            //  Reference ID
            httpRequest.TryGetParam("RefId", out var refId);

            //  Transaction Code
            httpRequest.TryGetParam("SaleReferenceId", out var saleReferenceId);

            var isSucceed = resCode == OkResult;

            PaymentVerifyResult result = null;

            if (!isSucceed)
            {
                var message = MellatGatewayResultTranslator.Translate(resCode, messagesOptions);

                result = PaymentVerifyResult.Failed(message);
            }


            return new MellatCallbackResult
            {
                IsSucceed = isSucceed,
                RefId = refId,
                SaleReferenceId = saleReferenceId,
                Result = result
            };
        }

        public static string CreateVerifyData(Payment payment, MellatGatewayOptions options, MellatCallbackResult callbackResult)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpVerifyRequest>" +
                $"<terminalId>{options.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{options.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{options.UserPassword}</userPassword>" +
                $"<orderId>{payment.TrackingNumber}</orderId>" +
                $"<saleOrderId>{payment.TrackingNumber}</saleOrderId>" +
                $"<saleReferenceId>{callbackResult.SaleReferenceId}</saleReferenceId>" +
                "</int:bpVerifyRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static MellatVerifyResult CheckVerifyResult(string webServiceResponse, MellatCallbackResult callbackResult, MessagesOptions messagesOptions)
        {
            var serviceResult = XmlHelper.GetNodeValueFromXml(webServiceResponse, "return");

            var isSucceed = serviceResult == OkResult;

            PaymentVerifyResult verifyResult = null;

            if (!isSucceed)
            {
                var message = MellatGatewayResultTranslator.Translate(serviceResult, messagesOptions);

                verifyResult = PaymentVerifyResult.Failed(message);
            }

            return new MellatVerifyResult
            {
                IsSucceed = isSucceed,
                Result = verifyResult
            };
        }

        public static string CreateSettleData(Payment payment, MellatCallbackResult callbackResult, MellatGatewayOptions options)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpSettleRequest>" +
                $"<terminalId>{options.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{options.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{options.UserPassword}</userPassword>" +
                $"<orderId>{payment.TrackingNumber}</orderId>" +
                $"<saleOrderId>{payment.TrackingNumber}</saleOrderId>" +
                $"<saleReferenceId>{callbackResult.SaleReferenceId}</saleReferenceId>" +
                "</int:bpSettleRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static PaymentVerifyResult CreateSettleResult(string webServiceResponse, MellatCallbackResult callbackResult, MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "return");

            var isSuccess = result == OkResult || result == SettleSuccess;

            var message = isSuccess
                ? messagesOptions.PaymentSucceed
                : MellatGatewayResultTranslator.Translate(result, messagesOptions);

            return new PaymentVerifyResult
            {
                IsSucceed = isSuccess,
                TransactionCode = callbackResult.SaleReferenceId,
                Message = message,
            };
        }

        public static string CreateRefundData(Payment payment, MellatGatewayOptions options)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpReversalRequest>" +
                $"<terminalId>{options.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{options.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{options.UserPassword}</userPassword>" +
                $"<orderId>{payment.TrackingNumber}</orderId>" +
                $"<saleOrderId>{payment.TrackingNumber}</saleOrderId>" +
                $"<saleReferenceId>{payment.TransactionCode}</saleReferenceId>" +
                "</int:bpReversalRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static PaymentRefundResult CreateRefundResult(string webServiceResponse, MessagesOptions messagesOptions)
        {
            var result = XmlHelper.GetNodeValueFromXml(webServiceResponse, "return");

            var isSuccess = result == OkResult;

            var message = MellatGatewayResultTranslator.Translate(result, messagesOptions);

            return new PaymentRefundResult
            {
                IsSucceed = isSuccess,
                Message = message
            };
        }

        private static string CreateSimpleRequestData(Invoice invoice, MellatGatewayOptions options)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpPayRequest>" +
                $"<terminalId>{options.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{options.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{options.UserPassword}</userPassword>" +
                $"<orderId>{invoice.TrackingNumber}</orderId>" +
                $"<amount>{(long)invoice.Amount}</amount>" +
                "<!--Optional:-->" +
                $"<localDate>{DateTime.Now:yyyyMMdd}</localDate>" +
                "<!--Optional:-->" +
                $"<localTime>{DateTime.Now:HHmmss}</localTime>" +
                "<!--Optional:-->" +
                "<additionalData></additionalData>" +
                "<!--Optional:-->" +
                $"<callBackUrl>{invoice.CallbackUrl}</callBackUrl>" +
                "<payerId>0</payerId>" +
                "'</int:bpPayRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        private static string CreateCumulativeRequestData(Invoice invoice, MellatGatewayOptions options)
        {
            var accounts = (IList<MellatCumulativeDynamicAccount>)invoice.AdditionalData[CumulativeAccountsKey];

            var totalAmount = accounts.Sum(account => account.Amount);

            if (totalAmount != invoice.Amount)
            {
                throw new Exception("The total amount of Mellat Cumulative accounts is not equals to the amount of the invoice." +
                                    $"Invoice amount: {invoice.Amount}." +
                                    $"Accounts total amount: {totalAmount}");
            }

            var additionalData = accounts.Aggregate("", (current, account) => current + $"{account};");

            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpCumulativeDynamicPayRequest>" +
                $"<terminalId>{options.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{options.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{options.UserPassword}</userPassword>" +
                $"<orderId>{invoice.TrackingNumber}</orderId>" +
                $"<amount>{(long)invoice.Amount}</amount>" +
                "<!--Optional:-->" +
                $"<localDate>{DateTime.Now:yyyyMMdd}</localDate>" +
                "<!--Optional:-->" +
                $"<localTime>{DateTime.Now:HHmmss}</localTime>" +
                "<!--Optional:-->" +
                $"<additionalData>{additionalData}</additionalData>" +
                "<!--Optional:-->" +
                $"<callBackUrl>{invoice.CallbackUrl}</callBackUrl>" +
                "</int:bpCumulativeDynamicPayRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }
    }
}
