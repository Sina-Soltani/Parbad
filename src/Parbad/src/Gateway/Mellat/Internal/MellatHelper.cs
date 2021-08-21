// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Gateway.Mellat.Internal.Models;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.Mellat.Internal
{
    internal static class MellatHelper
    {
        private const string OkResult = "0";
        private const string DuplicateOrderNumberResult = "41";
        private const string AlreadyVerifiedResult = "43";
        private const string SettleSuccess = "45";

        internal static string CumulativeAccountsKey => "MellatCumulativeAccounts";
        internal static string AdditionalDataKey => "MellatAdditionalData";

        public static string CreateRequestData(Invoice invoice, MellatGatewayAccount account)
        {
            if (invoice.Properties == null || !invoice.Properties.ContainsKey(CumulativeAccountsKey))
            {
                return CreateSimpleRequestData(invoice, account);
            }

            return CreateCumulativeRequestData(invoice, account);
        }

        public static PaymentRequestResult CreateRequestResult(
            string webServiceResponse,
            Invoice invoice,
            HttpContext httpContext,
            MellatGatewayOptions gatewayOptions,
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

            var form = new Dictionary<string, string>
            {
                {"RefId", refId}
            };

            var additionalData = invoice.GetMellatAdditionalData();

            if (!string.IsNullOrWhiteSpace(additionalData?.MobileNumber))
            {
                form.Add("MobileNo", additionalData.MobileNumber);
            }

            return PaymentRequestResult.SucceedWithPost(
                account.Name,
                httpContext,
                gatewayOptions.PaymentPageUrl,
                form);
        }

        public static async Task<MellatCallbackResult> CrateCallbackResultAsync(
            HttpRequest httpRequest,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            var resCode = await httpRequest.TryGetParamAsync("ResCode", cancellationToken).ConfigureAwaitFalse();

            if (!resCode.Exists || resCode.Value.IsNullOrEmpty())
            {
                return new MellatCallbackResult
                {
                    IsSucceed = false,
                    Message = messagesOptions.InvalidDataReceivedFromGateway
                };
            }

            //  Reference ID
            var refId = await httpRequest.TryGetParamAsync("RefId", cancellationToken).ConfigureAwaitFalse();

            //  Transaction Code
            var saleReferenceId = await httpRequest.TryGetParamAsync("SaleReferenceId", cancellationToken).ConfigureAwaitFalse();

            var isSucceed = resCode.Value == OkResult;

            string message = null;

            if (!isSucceed)
            {
                message = MellatGatewayResultTranslator.Translate(resCode.Value, messagesOptions);
            }

            return new MellatCallbackResult
            {
                IsSucceed = isSucceed,
                RefId = refId.Value,
                SaleReferenceId = saleReferenceId.Value,
                Message = message
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

        private static string CreateSimpleRequestData(Invoice invoice, MellatGatewayAccount account)
        {
            var additionalData = invoice.GetMellatAdditionalData();

            var payerId = additionalData?.PayerId ?? "0";

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
                $"<additionalData>{additionalData.AdditionalData}</additionalData>" +
                "<!--Optional:-->" +
                $"<callBackUrl>{XmlHelper.EncodeXmlValue(invoice.CallbackUrl)}</callBackUrl>" +
                $"<payerId>{payerId}</payerId>" +
                "'</int:bpPayRequest>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        private static string CreateCumulativeRequestData(Invoice invoice, MellatGatewayAccount account)
        {
            var cumulativeAccounts = (List<MellatCumulativeDynamicAccount>)invoice.Properties[CumulativeAccountsKey];

            if (cumulativeAccounts.Count > 10)
            {
                throw new Exception("Cannot use more than 10 accounts for each Cumulative payment request.");
            }

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
