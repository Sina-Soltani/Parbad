// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Gateway.Melli.Internal.Models;
using Parbad.Gateway.Melli.Internal.ResultTranslator;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;

namespace Parbad.Gateway.Melli.Internal
{
    internal static class MelliHelper
    {
        public const string PaymentPageUrl = "https://sadad.shaparak.ir/VPG/Purchase";
        public const string BaseServiceUrl = "https://sadad.shaparak.ir";
        public const string ServiceRequestUrl = "/VPG/api/v0/Request/PaymentRequest";
        public const string ServiceVerifyUrl = "/VPG/api/v0/Advice/Verify";

        private const int SuccessCode = 0;
        private const int DuplicateTrackingNumberCode = 1011;
        public static string MultiplexingAccountsKey = "MultiplexingData";

        public static object CreateRequestData(Invoice invoice, MelliGatewayAccount account)
        {
            var signedData = SignRequestData(account.TerminalId, account.TerminalKey, invoice.TrackingNumber,
                invoice.Amount);
            if (invoice.AdditionalData == null || !invoice.AdditionalData.ContainsKey(MultiplexingAccountsKey))
                return CreateRequestObject(
                    account.TerminalId,
                    account.MerchantId,
                    invoice.Amount,
                    signedData,
                    invoice.CallbackUrl,
                    invoice.TrackingNumber);

            return CreateMultiplexingRequestData(invoice, account, signedData);
        }

        public static PaymentRequestResult CreateRequestResult(MelliApiRequestResult result, HttpContext httpContext,
            MelliGatewayAccount account, MessagesOptions messagesOptions)
        {
            if (result == null) return PaymentRequestResult.Failed(messagesOptions.UnexpectedErrorText);

            var isSucceed = result.ResCode == SuccessCode;

            if (!isSucceed)
            {
                string message;

                if (result.ResCode == DuplicateTrackingNumberCode)
                    message = messagesOptions.DuplicateTrackingNumber;
                else
                    message = !result.Description.IsNullOrEmpty()
                        ? result.Description
                        : MelliRequestResultTranslator.Translate(result.ResCode, messagesOptions);

                return PaymentRequestResult.Failed(message, account.Name);
            }

            var paymentPageUrl = $"{PaymentPageUrl}/Index?token={result.Token}";

            var transporterDescriptor = GatewayTransporterDescriptor.CreateRedirect(paymentPageUrl);

            var transporter = new DefaultGatewayTransporter(httpContext, transporterDescriptor);

            return PaymentRequestResult.Succeed(transporter, account.Name);
        }

        public static async Task<MelliCallbackResult> CreateCallbackResultAsync(
            InvoiceContext context,
            HttpRequest httpRequest,
            MelliGatewayAccount account,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            var apiResponseCode = await httpRequest.TryGetParamAsAsync<int>("ResCode", cancellationToken)
                .ConfigureAwaitFalse();

            if (!apiResponseCode.Exists || apiResponseCode.Value != SuccessCode)
                return new MelliCallbackResult
                {
                    IsSucceed = false,
                    Result = PaymentVerifyResult.Failed(messagesOptions.PaymentFailed)
                };

            var apiToken = await httpRequest.TryGetParamAsync("Token", cancellationToken).ConfigureAwaitFalse();
            var apiOrderId = await httpRequest.TryGetParamAsAsync<long>("OrderId", cancellationToken)
                .ConfigureAwaitFalse();

            if (!apiOrderId.Exists || apiOrderId.Value != context.Payment.TrackingNumber)
                return new MelliCallbackResult
                {
                    IsSucceed = false,
                    Token = apiToken.Value,
                    Result = PaymentVerifyResult.Failed(messagesOptions.InvalidDataReceivedFromGateway)
                };

            var signedData = SignVerifyData(account.TerminalKey, apiToken.Value);

            var dataToVerify = CreateVerifyObject(apiToken.Value, signedData);

            return new MelliCallbackResult
            {
                IsSucceed = true,
                Token = apiToken.Value,
                JsonDataToVerify = dataToVerify
            };
        }

        public static PaymentVerifyResult CreateVerifyResult(MelliApiVerifyResult result,
            MessagesOptions messagesOptions)
        {
            if (result == null) return PaymentVerifyResult.Failed(messagesOptions.UnexpectedErrorText);

            var message = !result.Description.IsNullOrEmpty() ? result.Description : MelliVerifyResultTranslator.Translate(result.ResCode, messagesOptions);

            var status = result.ResCode == SuccessCode
                ? PaymentVerifyResultStatus.Succeed
                : PaymentVerifyResultStatus.Failed;

            return new PaymentVerifyResult
            {
                Status = status,
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

        private static object CreateRequestObject(string terminalId, string merchantId, long amount, string signedData,
            string callbackUrl, long orderId)
        {
            return new
            {
                TerminalId = terminalId,
                MerchantId = merchantId,
                Amount = amount,
                SignData = signedData,
                ReturnUrl = callbackUrl,
                LocalDateTime = DateTime.Now,
                OrderId = orderId.ToString()
            };
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

        private static object CreateVerifyObject(string apiToken, string signedData)
        {
            return new
            {
                token = apiToken,
                SignData = signedData
            };
        }

        private static object CreateMultiplexingRequestData(Invoice invoice, MelliGatewayAccount account,
            string signedData)
        {
            var multiplexingAccount = (MelliMultiplexInvoice) invoice.AdditionalData[MultiplexingAccountsKey];

            //Check if share account do not more than 10 row
            if (multiplexingAccount.MultiplexingRows.Count > 10)
                throw new Exception("Cannot use more than 10 accounts for each Cumulative payment request.");

            //Create invoice info that have sharing 
            var invoiceInfo = new
            {
                account.TerminalId,
                account.MerchantId,
                Amount = invoice.Amount.Value,
                SignData = signedData,
                ReturnUrl = invoice.CallbackUrl.Url,
                LocalDateTime = DateTime.Now,
                OrderId = invoice.TrackingNumber,
                EnableMultiplexing = true,
                MultiplexingData = new
                {
                    multiplexingAccount.Type,
                    multiplexingAccount.MultiplexingRows
                }
            };
            return invoiceInfo;
        }
    }
}