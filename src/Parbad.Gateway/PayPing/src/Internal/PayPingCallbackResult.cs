// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.PayPing.Internal;

internal class PayPingCallbackResult
{
    public bool IsSucceed { get; set; }

    public string Message { get; set; }

    public long PaymentRefId { get; set; }

    /// <summary>The payment code stored at request time, which the callback has matched.</summary>
    public string PaymentCode { get; set; }
}