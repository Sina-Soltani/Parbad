// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Gateway.Mellat.Internal.Models;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Utilities;

namespace Parbad.Gateway.Mellat.Internal
{
    internal static class MellatHelper
    {
        private const string OkResult = "0";
        private const string DuplicateOrderNumberResult = "41";
        private const string AlreadyVerifiedResult = "43";
        private const string SettleSuccess = "45";

        internal const string CumulativeAccountsKey = "MellatCumulativeAccounts";

        public const string BaseServiceUrl = "https://bpm.shaparak.ir/";
        public const string WebServiceUrl = "/pgwchannel/services/pgw";
        public const string PaymentPageUrl = "https://bpm.shaparak.ir/pgwchannel/startpay.mellat";
        public const string TestWebServiceUrl = "/pgwchannel/services/pgwtest";

        public static string CreateRequestData(Invoice invoice, MellatGatewayAccount account)
        {
            if (invoice.AdditionalData == null || !invoice.AdditionalData.ContainsKey(CumulativeAccountsKey))
            {
                return CreateSimpleRequestData(invoice, account);
            }

            return CreateCumulativeRequestData(invoice, account);
        }

        public static PaymentRequestResult CreateRequestResult(
            string webServiceResponse,
            IHttpContextAccessor httpContextAccessor,
            MessagesOptions messagesOptions,
            GatewayAccount account)
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

                return PaymentRequestResult.Failed(message, account.Name);
            }

            var transporter = new GatewayPost(
                httpContextAccessor,
                PaymentPageUrl,
                new Dictionary<string, string>
                {
                    {"RefId", refId}
                });

            return PaymentRequestResult.Succeed(transporter, account.Name);
        }

        public static async Task<MellatCallbackResult> CrateCallbackResultAsync(HttpRequest httpRequest,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            var resCode = await httpRequest.TryGetParamAsync("ResCode", cancellationToken).ConfigureAwaitFalse();

            if (!resCode.Exists || resCode.Value.IsNullOrEmpty())
            {
                return new MellatCallbackResult
                {
                    IsSucceed = false,
                    Result = PaymentVerifyResult.Failed(messagesOptions.InvalidDataReceivedFromGateway)
                };
            }

            //  Reference ID
            var refId = await httpRequest.TryGetParamAsync("RefId", cancellationToken).ConfigureAwaitFalse();

            //  Transaction Code
            var saleReferenceId = await httpRequest.TryGetParamAsync("SaleReferenceId", cancellationToken).ConfigureAwaitFalse();

            var isSucceed = resCode.Value == OkResult;

            PaymentVerifyResult result = null;

            if (!isSucceed)
            {
                var message = MellatGatewayResultTranslator.Translate(resCode.Value, messagesOptions);

                result = PaymentVerifyResult.Failed(message);
            }


            return new MellatCallbackResult
            {
                IsSucceed = isSucceed,
                RefId = refId.Value,
                SaleReferenceId = saleReferenceId.Value,
                Result = result
            };
        }

        public static string CreateVerifyData(InvoiceContext context, MellatGatewayAccount account, MellatCallbackResult callbackResult)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpVerifyRequest>" +
                $"<terminalId>{account.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{account.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{account.UserPassword}</userPassword>" +
                $"<orderId>{context.Payment.TrackingNumber}</orderId>" +
                $"<saleOrderId>{context.Payment.TrackingNumber}</saleOrderId>" +
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

        public static string CreateSettleData(InvoiceContext context, MellatCallbackResult callbackResult, MellatGatewayAccount account)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpSettleRequest>" +
                $"<terminalId>{account.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{account.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{account.UserPassword}</userPassword>" +
                $"<orderId>{context.Payment.TrackingNumber}</orderId>" +
                $"<saleOrderId>{context.Payment.TrackingNumber}</saleOrderId>" +
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
                Status = isSuccess ? PaymentVerifyResultStatus.Succeed : PaymentVerifyResultStatus.Failed,
                TransactionCode = callbackResult.SaleReferenceId,
                Message = message,
            };
        }

        public static string CreateRefundData(InvoiceContext context, MellatGatewayAccount account)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpReversalRequest>" +
                $"<terminalId>{account.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{account.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{account.UserPassword}</userPassword>" +
                $"<orderId>{context.Payment.TrackingNumber}</orderId>" +
                $"<saleOrderId>{context.Payment.TrackingNumber}</saleOrderId>" +
                $"<saleReferenceId>{context.Payment.TransactionCode}</saleReferenceId>" +
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
                Status = isSuccess ? PaymentRefundResultStatus.Succeed : PaymentRefundResultStatus.Failed,
                Message = message
            };
        }

        public static string GetWebServiceUrl(bool isTestTerminal)
        {
            return isTestTerminal
                ? TestWebServiceUrl
                : WebServiceUrl;
        }

        private static string CreateSimpleRequestData(Invoice invoice, MellatGatewayAccount account)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpPayRequest>" +
                $"<terminalId>{account.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{account.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{account.UserPassword}</userPassword>" +
                $"<orderId>{invoice.TrackingNumber}</orderId>" +
                $"<amount>{(long)invoice.Amount}</amount>" +
                "<!--Optional:-->" +
                $"<localDate>{DateTime.Now:yyyyMMdd}</localDate>" +
                "<!--Optional:-->" +
                $"<localTime>{DateTime.Now:HHmmss}</localTime>" +
                "<!--Optional:-->" +
                "<additionalData></additionalData>" +
                "<!--Optional:-->" +
                $"<callBackUrl>{XmlHelper.EncodeXmlValue(invoice.CallbackUrl)}</callBackUrl>" +
                "<payerId>0</payerId>" +
                "'</int:bpPayRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        private static string CreateCumulativeRequestData(Invoice invoice, MellatGatewayAccount account)
        {
            var cumulativeAccounts = (IList<MellatCumulativeDynamicAccount>)invoice.AdditionalData[CumulativeAccountsKey];

            if (cumulativeAccounts.Count > 10)
            {
                throw new Exception("Cannot use more than 10 accounts for each Cumulative payment request.");
            }

            //var totalAmount = cumulativeAccounts.Sum(cumulativeAccount => cumulativeAccount.Amount);

            //if (totalAmount != invoice.Amount)
            //{
            //    throw new Exception("The total amount of Mellat Cumulative accounts is not equals to the amount of the invoice." +
            //                        $"Invoice amount: {invoice.Amount}." +
            //                        $"Accounts total amount: {totalAmount}");
            //}

            var additionalData = cumulativeAccounts.Aggregate("", (current, cumulativeAccount) => current + $"{cumulativeAccount};");

            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:int=\"http://interfaces.core.sw.bps.com/\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<int:bpCumulativeDynamicPayRequest>" +
                $"<terminalId>{account.TerminalId}</terminalId>" +
                "<!--Optional:-->" +
                $"<userName>{account.UserName}</userName>" +
                "<!--Optional:-->" +
                $"<userPassword>{account.UserPassword}</userPassword>" +
                $"<orderId>{invoice.TrackingNumber}</orderId>" +
                $"<amount>{(long)invoice.Amount}</amount>" +
                "<!--Optional:-->" +
                $"<localDate>{DateTime.Now:yyyyMMdd}</localDate>" +
                "<!--Optional:-->" +
                $"<localTime>{DateTime.Now:HHmmss}</localTime>" +
                "<!--Optional:-->" +
                $"<additionalData>{additionalData}</additionalData>" +
                "<!--Optional:-->" +
                $"<callBackUrl>{XmlHelper.EncodeXmlValue(invoice.CallbackUrl)}</callBackUrl>" +
                "</int:bpCumulativeDynamicPayRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }
    }
}
