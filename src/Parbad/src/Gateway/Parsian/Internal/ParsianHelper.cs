// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Gateway.Parsian.Internal.Models;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Storage.Abstractions.Models;
using Parbad.Utilities;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Gateway.Parsian.Internal
{
    internal static class ParsianHelper
    {
        // "The transaction was confirmed before". Taken from several integrations' copies of the PEC error list; the
        // official document could not be found.
        private const string AlreadyConfirmedStatus = "-1533";

        public static string CreateRequestData(ParsianGatewayAccount account, Invoice invoice)
        {
            var additionalData = invoice.GetParsianAdditionalData();

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
                $"<sal:CallBackUrl>{XmlHelper.EncodeXmlValue(invoice.CallbackUrl)}</sal:CallBackUrl>" +
                "<!--Optional:-->" +
                $"<sal:AdditionalData>{XmlHelper.EncodeXmlValue(additionalData?.AdditionalData)}</sal:AdditionalData>" +
                "<!--Optional:-->" +
                $"<sal:Originator>{XmlHelper.EncodeXmlValue(additionalData?.Originator)}</sal:Originator>" +
                "</sal:requestData>" +
                "</sal:SalePaymentRequest> " +
                "</soapenv:Body> " +
                "</soapenv:Envelope> ";
        }

        public static PaymentRequestResult CreateRequestResult(
            string webServiceResponse,
            HttpContext httpContext,
            ParsianGatewayAccount account,
            ParsianGatewayOptions gatewayOptions,
            MessagesOptions messagesOptions)
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

            var paymentPageUrl = $"{gatewayOptions.PaymentPageUrl}?Token={token}";

            var result = PaymentRequestResult.SucceedWithRedirect(account.Name, httpContext, paymentPageUrl);

            result.DatabaseAdditionalData.Add("token", token);

            return result;
        }

        public static async Task<ParsianCallbackResult> CreateCallbackResultAsync(HttpRequest httpRequest,
                                                                                InvoiceContext context,
                                                                                MessagesOptions messagesOptions,
                                                                                CancellationToken cancellationToken)
        {
            // Read asynchronously: a synchronous Request.Form blocks a thread, and throws on a GET callback.
            var (_, token) = await httpRequest.TryGetParamAsync("token", cancellationToken).ConfigureAwaitFalse();
            var (_, status) = await httpRequest.TryGetParamAsync("status", cancellationToken).ConfigureAwaitFalse();
            var (_, orderId) = await httpRequest.TryGetParamAsync("orderId", cancellationToken).ConfigureAwaitFalse();
            var (_, amount) = await httpRequest.TryGetParamAsync("amount", cancellationToken).ConfigureAwaitFalse();

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
                else if (!IsTokenIssuedForThisInvoice(token, context))
                {
                    // ConfirmPayment confirms whatever token it receives, so a token of another (paid) invoice
                    // would settle this one.
                    isSucceed = false;
                    message = "Error in Callback section. Token is not equal with the data in database.";
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
                else if (!long.TryParse(orderId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numberOrderNumber) ||
                         numberOrderNumber != context.Payment.TrackingNumber)
                {
                    isSucceed = false;
                    message = "Error in Callback section. OrderNumber is not equal with the data in database.";
                }
                else
                {
                    if (!long.TryParse(amount, NumberStyles.Integer | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var numberAmount))
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

            return new ParsianCallbackResult
            {
                IsSucceed = isSucceed,
                Token = token,
                Message = message
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

        public static PaymentVerifyResult CreateVerifyResult(string webServiceResponse,
                                                             ParsianCallbackResult callbackResult,
                                                             InvoiceContext context,
                                                             MessagesOptions messagesOptions)
        {
            var status = XmlHelper.GetNodeValueFromXml(webServiceResponse, "Status", "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService");
            var rrn = XmlHelper.GetNodeValueFromXml(webServiceResponse, "RRN", "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService");
            var token = XmlHelper.GetNodeValueFromXml(webServiceResponse, "Token", "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService");

            var isSucceed = !status.IsNullOrEmpty() &&
                            status == "0" &&
                            !rrn.IsNullOrEmpty();

            PaymentVerifyResult result;

            if (isSucceed)
            {
                result = PaymentVerifyResult.Succeed(rrn, messagesOptions.PaymentSucceed, status);
            }
            else if (status == AlreadyConfirmedStatus)
            {
                // The payment was confirmed before (e.g. the first response was lost). The bank may not return the
                // RRN again, so a "0" is not stored as the transaction code.
                var transactionCode = rrn.IsNullOrEmpty() || rrn == "0" ? null : rrn;

                if (TryGetIssuedToken(context, out var issuedToken) &&
                    string.Equals(issuedToken, callbackResult.Token, StringComparison.Ordinal))
                {
                    // The confirmed token is the one issued for this invoice, so it is this invoice's payment.
                    result = PaymentVerifyResult.Succeed(transactionCode, messagesOptions.PaymentSucceed, status);
                }
                else
                {
                    // Invoices requested before the token was stored cannot be tied to the token; Parbad decides.
                    result = PaymentVerifyResult.Failed($"Error in Verifying section: {status}", status);
                    result.TransactionCode = transactionCode;
                    result.IsAlreadyVerifiedAtBank = true;
                }
            }
            else
            {
                result = PaymentVerifyResult.Failed($"Error in Verifying section: {status}", status);
                result.TransactionCode = rrn;
            }

            result.DatabaseAdditionalData.Add("token", token);

            return result;
        }

        public static string CreateRefundData(ParsianGatewayAccount account, InvoiceContext context, Money amount)
        {
            var transaction = context.Transactions.LastOrDefault(item => item.Type == TransactionType.Verify && item.IsSucceed);

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
                Status = status == "0" ? PaymentRefundResultStatus.Succeed : PaymentRefundResultStatus.Failed,
                Message = message
            };

            result.DatabaseAdditionalData.Add("token", token);

            return result;
        }

        /// <summary>
        /// Checks that the callback's token is the one stored when this invoice was requested. Invoices without
        /// a stored token have nothing to compare with and are accepted as before.
        /// </summary>
        private static bool IsTokenIssuedForThisInvoice(string callbackToken, InvoiceContext context)
        {
            if (!TryGetIssuedToken(context, out var issuedToken)) return true;

            return string.Equals(issuedToken, callbackToken, StringComparison.Ordinal);
        }

        private static bool TryGetIssuedToken(InvoiceContext context, out string issuedToken)
        {
            issuedToken = null;

            var requestTransaction = context.Transactions?.SingleOrDefault(transaction => transaction.Type == TransactionType.Request);

            return !string.IsNullOrEmpty(requestTransaction?.AdditionalData) &&
                   requestTransaction.ToDictionary().TryGetValue("token", out issuedToken);
        }
    }
}
