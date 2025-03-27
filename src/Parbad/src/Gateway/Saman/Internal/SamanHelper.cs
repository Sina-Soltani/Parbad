// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Parbad.Gateway.Saman.Internal.Models;
using Parbad.Http;
using Parbad.Internal;

namespace Parbad.Gateway.Saman.Internal;

internal static class SamanHelper
{
    public static async Task<SamanCallbackResponse> BindCallbackResponse(HttpRequest httpRequest, CancellationToken cancellationToken)
    {
        var mid = await httpRequest.TryGetParamAsync("MID", cancellationToken).ConfigureAwaitFalse();
        var terminalId = await httpRequest.TryGetParamAsync("TerminalId", cancellationToken).ConfigureAwaitFalse();
        var state = await httpRequest.TryGetParamAsync("State", cancellationToken).ConfigureAwaitFalse();
        var status = await httpRequest.TryGetParamAsync("Status", cancellationToken).ConfigureAwaitFalse();
        var refNum = await httpRequest.TryGetParamAsync("RefNum", cancellationToken).ConfigureAwaitFalse();
        var resNum = await httpRequest.TryGetParamAsync("ResNum", cancellationToken).ConfigureAwaitFalse();
        var securePan = await httpRequest.TryGetParamAsync("SecurePan", cancellationToken).ConfigureAwaitFalse();
        var hashedCardNumber = await httpRequest.TryGetParamAsync("HashedCardNumber", cancellationToken).ConfigureAwaitFalse();
        var traceNo = await httpRequest.TryGetParamAsync("TraceNo", cancellationToken).ConfigureAwaitFalse();
        var rrn = await httpRequest.TryGetParamAsync("Rrn", cancellationToken).ConfigureAwaitFalse();
        var amount = await httpRequest.TryGetParamAsync("Amount", cancellationToken).ConfigureAwaitFalse();
        var wage = await httpRequest.TryGetParamAsync("Wage", cancellationToken).ConfigureAwaitFalse();

        return new SamanCallbackResponse
               {
                   MID = mid.Value,
                   State = state.Value,
                   Status = status.Value,
                   Rrn = rrn.Value,
                   RefNum = refNum.Value,
                   ResNum = resNum.Value,
                   TerminalId = terminalId.Value,
                   TraceNo = traceNo.Value,
                   Amount = amount.Value,
                   Wage = wage.Value,
                   SecurePan = securePan.Value,
                   HashedCardNumber = hashedCardNumber.Value,
               };
    }

    public static bool ValidateCallbackResponse(SamanCallbackResponse callbackResponse,
                                                InvoiceContext invoiceContext,
                                                SamanGatewayAccount gatewayAccount,
                                                out string failures)
    {
        const string nullOrEmptyString = "IsReceivedFromTheGatewayAsNullOrEmpty";

        var validationFailures = new StringBuilder();
        var isValid = true;

        if (string.IsNullOrWhiteSpace(callbackResponse.Status))
        {
            isValid = false;

            validationFailures.AppendLine($"{nameof(SamanCallbackResponse.Status)}{nullOrEmptyString}");
        }

        if (string.IsNullOrWhiteSpace(callbackResponse.TerminalId))
        {
            isValid = false;

            validationFailures.AppendLine($"{nameof(SamanCallbackResponse.TerminalId)}{nullOrEmptyString}");
        }
        else if (callbackResponse.TerminalId != gatewayAccount.TerminalId)
        {
            isValid = false;

            validationFailures.AppendLine($"ReceivedInvalidTerminalId: {callbackResponse.TerminalId}");
        }

        if (string.IsNullOrWhiteSpace(callbackResponse.RefNum))
        {
            isValid = false;

            validationFailures.AppendLine($"{nameof(SamanCallbackResponse.RefNum)}{nullOrEmptyString}");
        }
        else if (callbackResponse.ResNum != invoiceContext.Payment.TrackingNumber.ToString())
        {
            isValid = false;

            validationFailures.AppendLine($"ReceivedInvalidTrackingNumber: {callbackResponse.RefNum}");
        }

        if (string.IsNullOrWhiteSpace(callbackResponse.ResNum))
        {
            isValid = false;

            validationFailures.AppendLine($"{nameof(SamanCallbackResponse.ResNum)}{nullOrEmptyString}");
        }

        if (string.IsNullOrWhiteSpace(callbackResponse.Amount))
        {
            isValid = false;

            validationFailures.AppendLine($"{nameof(SamanCallbackResponse.Amount)}{nullOrEmptyString}");
        }
        else if (callbackResponse.Amount != ((long)invoiceContext.Payment.Amount).ToString())
        {
            isValid = false;

            validationFailures.AppendLine($"ReceivedInvalidAmount: {callbackResponse.Amount}");
        }

        if (string.IsNullOrWhiteSpace(callbackResponse.Rrn))
        {
            isValid = false;

            validationFailures.AppendLine($"{nameof(SamanCallbackResponse.Rrn)}{nullOrEmptyString}");
        }

        failures = validationFailures.ToString();

        return isValid;
    }

    public static SamanVerificationAdditionalData MapCallbackResponseToAdditionalData(SamanCallbackResponse callbackResponse)
    {
        return new SamanVerificationAdditionalData
               {
                   Rrn = callbackResponse.Rrn,
                   TraceNo = callbackResponse.TraceNo,
                   RefNum = callbackResponse.RefNum,
                   ResNum = callbackResponse.ResNum,
                   SecurePan = callbackResponse.SecurePan,
                   HashedCardNumber = callbackResponse.HashedCardNumber
               };
    }
}
