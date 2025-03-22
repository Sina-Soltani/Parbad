// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Internal;

public class PaymentVerifyResult : PaymentResult, IPaymentVerifyResult
{
    public string TransactionCode { get; set; }

    public PaymentVerifyResultStatus Status { get; set; }

    public override bool IsSucceed => Status == PaymentVerifyResultStatus.Succeed;

    public static PaymentVerifyResult Succeed(string transactionCode,
                                              string message,
                                              string gatewayResponseCode = null)
    {
        return new PaymentVerifyResult
               {
                   Status = PaymentVerifyResultStatus.Succeed,
                   TransactionCode = transactionCode,
                   Message = message,
                   GatewayResponseCode = gatewayResponseCode
               };
    }

    public static PaymentVerifyResult Failed(string message, string gatewayResponseCode = null)
    {
        return new PaymentVerifyResult
               {
                   Status = PaymentVerifyResultStatus.Failed,
                   Message = message,
                   GatewayResponseCode = gatewayResponseCode
               };
    }
}
