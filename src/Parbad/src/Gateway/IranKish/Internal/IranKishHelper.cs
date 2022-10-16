// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Gateway.IranKish.Internal.Models;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;
using Parbad.Utilities;

namespace Parbad.Gateway.IranKish.Internal
{
    internal static class IranKishHelper
    {
        private const string SuccessResponseCode = "00";

        internal static string CmsPreservationIdKey => "IranKishCmsPreservationId";

        public static IranKishTokenRequest CreateRequestData(Invoice invoice, IranKishGatewayAccount account)
        {
            var requestInfo = new IranKishTokenRequestInfo
            {
                AcceptorId = account.AcceptorId,
                Amount = invoice.Amount,
                CmsPreservationId = invoice.GetCmsPreservationId(),
                PaymentId = invoice.TrackingNumber.ToString(),
                RequestId = Guid.NewGuid().ToString("N").Substring(0, 20),
                RequestTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                RevertUri = invoice.CallbackUrl,
                TerminalId = account.TerminalId,
                TransactionType = "Purchase",
            };

            return new IranKishTokenRequest
            {
                AuthenticationEnvelope = GetAuthenticationEnvelope(requestInfo, account),
                Request = requestInfo
            };
        }

        private static IranKishAuthenticationEnvelope GetAuthenticationEnvelope(IranKishTokenRequestInfo requestInfo, IranKishGatewayAccount account)
        {
            var isMultiplex = requestInfo.MultiplexParameters != null;

            var baseString =
                requestInfo.TerminalId +
                account.PassPhrase +
                requestInfo.Amount.ToString().PadLeft(12, '0') +
                (isMultiplex ? "01" : "00") +
                (isMultiplex
                    ? requestInfo.MultiplexParameters.Select(t =>
                            $"{t.Iban.Replace("IR", "2718")}{t.Amount.ToString().PadLeft(12, '0')}")
                        .Aggregate((a, b) => $"{a}{b}")
                    : string.Empty);

            var encryptedString = IranKishCrypto.EncryptAuthenticationEnvelope(baseString, account.PublicKey, out var iv);
            return new IranKishAuthenticationEnvelope
            {
                Data = encryptedString,
                Iv = iv
            };
        }

        public static PaymentRequestResult CreateRequestResult(
            IranKishTokenResult result,
            HttpContext httpContext,
            IranKishGatewayAccount account,
            IranKishGatewayOptions gatewayOptions,
            MessagesOptions messagesOptions)
        {
            if (result == null)
            {
                return PaymentRequestResult.Failed(messagesOptions.UnexpectedErrorText);
            }

            var isSucceed = result.ResponseCode == SuccessResponseCode;

            if (!isSucceed)
            {
                var message = IranKishGatewayResultTranslator.Translate(result.ResponseCode, messagesOptions);

                return PaymentRequestResult.Failed(message, account.Name);
            }

            var form = new Dictionary<string, string>
            {
                {"tokenIdentity", result.Result.Token}
            };

            return PaymentRequestResult.SucceedWithPost(
                account.Name,
                httpContext,
                gatewayOptions.PaymentPageUrl,
                form);
        }

        public static async Task<IranKishCallbackResult> CreateCallbackResultAsync(
            InvoiceContext context,
            IranKishGatewayAccount account,
            HttpRequest httpRequest,
            MessagesOptions messagesOptions,
            CancellationToken cancellationToken)
        {
            var token = await httpRequest.TryGetParamAsync("Token", cancellationToken).ConfigureAwaitFalse();
            var acceptorId = await httpRequest.TryGetParamAsync("AcceptorId", cancellationToken).ConfigureAwaitFalse();
            var responseCode = await httpRequest.TryGetParamAsync("ResponseCode", cancellationToken).ConfigureAwaitFalse();
            var paymentId = await httpRequest.TryGetParamAsync("PaymentId", cancellationToken).ConfigureAwaitFalse();
            var requestId = await httpRequest.TryGetParamAsync("RequestId", cancellationToken).ConfigureAwaitFalse();
            var sha256OfPan = await httpRequest.TryGetParamAsync("Sha256OfPan", cancellationToken).ConfigureAwaitFalse();
            var retrievalReferenceNumber = await httpRequest.TryGetParamAsync("RetrievalReferenceNumber", cancellationToken).ConfigureAwaitFalse();
            var amount = await httpRequest.TryGetParamAsync("Amount", cancellationToken).ConfigureAwaitFalse();
            var maskedPan = await httpRequest.TryGetParamAsync("MaskedPan", cancellationToken).ConfigureAwaitFalse();
            var systemTraceAuditNumber = await httpRequest.TryGetParamAsync("SystemTraceAuditNumber", cancellationToken).ConfigureAwaitFalse();

            var isSucceed = responseCode.Value.Equals(SuccessResponseCode, StringComparison.OrdinalIgnoreCase);
            var message = isSucceed
                ? null
                : IranKishGatewayResultTranslator.Translate(responseCode.Value, messagesOptions);

            return new IranKishCallbackResult
            {
                IsSucceed = isSucceed,
                Message = message,
                Token = token.Value,
                AcceptorId = acceptorId.Value,
                ResponseCode = responseCode.Value,
                PaymentId = paymentId.Value,
                RequestId = requestId.Value,
                Sha256OfPan = sha256OfPan.Value,
                RetrievalReferenceNumber = retrievalReferenceNumber.Value,
                Amount = amount.Value,
                MaskedPan = maskedPan.Value,
                SystemTraceAuditNumber = systemTraceAuditNumber.Value
            };
        }

        public static PaymentVerifyResult CreateVerifyResult(IranKishVerifyResult result, MessagesOptions messagesOptions)
        {
            if (result == null)
            {
                return PaymentVerifyResult.Failed(messagesOptions.UnexpectedErrorText);
            }

            var responseCode = result.Result.ResponseCode;
            PaymentVerifyResultStatus status;
            string message;
            if (responseCode == SuccessResponseCode)
            {
                status = PaymentVerifyResultStatus.Succeed;
                message = messagesOptions.PaymentSucceed;
            }
            else
            {
                status = PaymentVerifyResultStatus.Failed;
                message = IranKishGatewayResultTranslator.Translate(responseCode, messagesOptions);
            }

            return new PaymentVerifyResult
            {
                Status = status,
                TransactionCode = result.Result.RetrievalReferenceNumber,
                Message = message
            };
        }
    }
}
