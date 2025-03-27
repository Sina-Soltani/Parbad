// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Internal;

public class PaymentFetchResult : PaymentResult, IPaymentFetchResult
{
    public string TransactionCode { get; set; }

    public PaymentFetchResultStatus Status { get; set; }

    public bool IsAlreadyVerified { get; set; }

    public override bool IsSucceed => Status == PaymentFetchResultStatus.ReadyForVerifying;

    public static PaymentFetchResult Failed(string message, string gatewayResponseCode = null)
    {
        return new PaymentFetchResult
               {
                   Status = PaymentFetchResultStatus.Failed,
                   Message = message
               };
    }

    public static PaymentFetchResult ReadyForVerifying()
    {
        return new PaymentFetchResult
               {
                   Status = PaymentFetchResultStatus.ReadyForVerifying
               };
    }
}
