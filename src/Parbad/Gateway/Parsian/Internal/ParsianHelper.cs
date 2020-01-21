// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Gateway.Parsian.Internal.Models;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Storage.Abstractions;
using Parbad.Utilities;

namespace Parbad.Gateway.Parsian.Internal
{
    internal static class ParsianHelper
    {
        private const string PaymentPageUrl = "https://pec.shaparak.ir/NewIPG/";
        public const string BaseServiceUrl = "https://pec.shaparak.ir/";
        public const string RequestServiceUrl = "/NewIPGServices/Sale/SaleService.asmx";
        public const string VerifyServiceUrl = "/NewIPGServices/Confirm/ConfirmService.asmx";
        public const string RefundServiceUrl = "/NewIPGServices/Reverse/ReversalService.asmx";

        public static string CreateRequestData(ParsianGatewayAccount account, Invoice invoice)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:sal=\"https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<sal:SalePaymentRequest>" +
                "<!--Optional:-->" +
                "<sal:requestData>" +
                "<!--Optional:-->" +
                $"<sal:LoginAccount>{XmlHelper.EncodeXmlValue(account.LoginAccount)}</sal:LoginAccount>" +
                $"<sal:Amount>{(long)invoice.Amount}</sal:Amount>" +
                $"<sal:OrderId>{invoice.TrackingNumber}</sal:OrderId>" +
                "<!--Optional:-->" +
                $"<sal:CallBackUrl>{invoice.CallbackUrl}</sal:CallBackUrl>" +
                "<!--Optional:-->" +
                "<sal:AdditionalData></sal:AdditionalData>" +
                "<!--Optional:-->" +
                "<sal:Originator></sal:Originator>" +
                "</sal:requestData>" +
                "</sal:SalePaymentRequest> " +
                "</soapenv:Body> " +
                "</soapenv:Envelope> ";
        }

        public static PaymentRequestResult CreateRequestResult(string webServiceResponse, IHttpContextAccessor httpContextAccessor, ParsianGatewayAccount account, MessagesOptions messagesOptions)
        {
            var token = XmlHelper.GetNodeValueFromXml(webServiceResponse, "Token", "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService");
            var status = XmlHelper.GetNodeValueFromXml(webServiceResponse, "Status", "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService");
            var message = XmlHelper.GetNodeValueFromXml(webServiceResponse, "Message", "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService");

            var isSucceed = !status.IsNullOrEmpty() &&
                            status == "0" &&
                            !token.IsNullOrEmpty();

            if (!isSucceed)
            {
                if (message == null)
                {
                    message = messagesOptions.PaymentFailed;
                }

                return PaymentRequestResult.Failed(message, account.Name);
            }

            var paymentPageUrl = $"{PaymentPageUrl}?Token={token}";

            var result = PaymentRequestResult.Succeed(new GatewayRedirect(httpContextAccessor, paymentPageUrl), account.Name);

            result.DatabaseAdditionalData.Add("token", token);

            return result;
        }

        public static ParsianCallbackResult CreateCallbackResult(HttpRequest httpRequest, InvoiceContext context, MessagesOptions messagesOptions)
        {
            httpRequest.Form.TryGetValue("token", out var token);
            httpRequest.Form.TryGetValue("status", out var status);
            httpRequest.Form.TryGetValue("orderId", out var orderId);
            httpRequest.Form.TryGetValue("amount", out var amount);

            var isSucceed = true;

            string message = null;

            if (status.IsNullOrEmpty())
            {
                isSucceed = false;
                message = "Error in Callback section. status is null or empty.";
            }
            else if (status != "0")
            {
                isSucceed = false;
                message = $"Error in Callback section. Status: {status}";
            }
            else
            {
                if (token.IsNullOrEmpty())
                {
                    isSucceed = false;
                    message = "Error in Callback section. Token is null or empty.";
                }
                else if (orderId.IsNullOrEmpty())
                {
                    isSucceed = false;
                    message = "Error in Callback section. OrderId is null or empty.";
                }
                else if (amount.IsNullOrEmpty())
                {
                    isSucceed = false;
                    message = "Error in Callback section. Amount is null or empty.";
                }
                else if (!long.TryParse(orderId, out var numberOrderNumber) ||
                         numberOrderNumber != context.Payment.TrackingNumber)
                {
                    isSucceed = false;
                    message = "Error in Callback section. OrderNumber is not equal with the data in database.";
                }
                else
                {
                    if (!long.TryParse(amount, NumberStyles.Any, CultureInfo.CurrentCulture, out var numberAmount))
                    {
                        isSucceed = false;
                        message = $"Error in Callback section. Cannot parse the amount value {amount}";
                    }
                    else if (numberAmount != (long)context.Payment.Amount)
                    {
                        isSucceed = false;
                        message = "Error in Callback section. Amount is not equal with the data in database.";
                    }
                }
            }

            PaymentVerifyResult verifyResult = null;

            if (!isSucceed)
            {
                verifyResult = PaymentVerifyResult.Failed(message);
            }

            return new ParsianCallbackResult
            {
                IsSucceed = isSucceed,
                Token = token,
                Result = verifyResult
            };
        }

        public static string CreateVerifyData(ParsianGatewayAccount account, ParsianCallbackResult callbackResult)
        {
            return
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:con=\"https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService\">" +
                "<soapenv:Header/>" +
                "<soapenv:Body>" +
                "<con:ConfirmPayment>" +
                "<!--Optional:-->" +
                "<con:requestData>" +
                "<!--Optional:-->" +
                $"<con:LoginAccount>{XmlHelper.EncodeXmlValue(account.LoginAccount)}</con:LoginAccount>" +
                $"<con:Token>{XmlHelper.EncodeXmlValue(callbackResult.Token)}</con:Token>" +
                "</con:requestData>" +
                "</con:ConfirmPayment>" +
                "</soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        public static PaymentVerifyResult CreateVerifyResult(string webServiceResponse, MessagesOptions messagesOptions)
        {
            var status = XmlHelper.GetNodeValueFromXml(webServiceResponse, "Status", "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService");
            var rrn = XmlHelper.GetNodeValueFromXml(webServiceResponse, "RRN", "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService");
            var token = XmlHelper.GetNodeValueFromXml(webServiceResponse, "Token", "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService");

            var isSucceed = !status.IsNullOrEmpty() &&
                            status == "0" &&
                            !rrn.IsNullOrEmpty();

            var message = isSucceed
                ? messagesOptions.PaymentSucceed
                : $"Error in Verifying section: {status}";

            var result = new PaymentVerifyResult
            {
                IsSucceed = isSucceed,
                TransactionCode = rrn,
                Message = message
            };

            result.DatabaseAdditionalData.Add("token", token);

            return result;
        }

        public static string CreateRefundData(ParsianGatewayAccount account, InvoiceContext context, Money amount)
        {
            var transaction = context.Transactions.SingleOrDefault(item => item.Type == TransactionType.Verify);

            if (transaction == null) throw new InvalidOperationException($"No transaction record found in database for payment with tracking number {context.Payment.TrackingNumber}.");

            if (!AdditionalDataConverter.ToDictionary(transaction).TryGetValue("token", out var token))
            {
                throw new InvalidOperationException($"No token found in database for payment with tracking number {context.Payment.TrackingNumber}.");
            }

            return
                "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:rev=\"https://pec.Shaparak.ir/NewIPGServices/Reversal/ReversalService\">" +
                "<soap:Header/>" +
                "<soap:Body>" +
                "<rev:ReversalRequest>" +
                "<!--Optional:-->" +
                "<rev:requestData>" +
                "<!--Optional:-->" +
                $"<rev:LoginAccount>{XmlHelper.EncodeXmlValue(account.LoginAccount)}</rev:LoginAccount>" +
                $"<rev:Token>{XmlHelper.EncodeXmlValue(token)}</rev:Token>" +
                "</rev:requestData>" +
                "</rev:ReversalRequest>" +
                "</soap:Body>" +
                "</soap:Envelope>";
        }

        public static PaymentRefundResult CreateRefundResult(string webServiceResponse, MessagesOptions messagesOptions)
        {
            var status = XmlHelper.GetNodeValueFromXml(webServiceResponse, "Status", "https://pec.Shaparak.ir/NewIPGServices/Reversal/ReversalService");
            var message = XmlHelper.GetNodeValueFromXml(webServiceResponse, "Message", "https://pec.Shaparak.ir/NewIPGServices/Reversal/ReversalService");
            var token = XmlHelper.GetNodeValueFromXml(webServiceResponse, "Token", "https://pec.Shaparak.ir/NewIPGServices/Reversal/ReversalService");

            if (message.IsNullOrEmpty())
            {
                message = $"Error {status}";
            }

            var result = new PaymentRefundResult
            {
                IsSucceed = status == "0",
                Message = message
            };

            result.DatabaseAdditionalData.Add("token", token);

            return result;
        }
    }
}
