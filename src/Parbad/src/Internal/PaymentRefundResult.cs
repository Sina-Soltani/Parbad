// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Internal;

public class PaymentRefundResult : PaymentResult, IPaymentRefundResult
{
    public PaymentRefundResultStatus Status { get; set; }

    public override bool IsSucceed => Status == PaymentRefundResultStatus.Succeed;

    public static PaymentRefundResult Succeed(string message = null, string gatewayResponseCode = null)
    {
        return new PaymentRefundResult
               {
                   Status = PaymentRefundResultStatus.Succeed,
                   Message = message ?? string.Empty,
                   GatewayResponseCode = gatewayResponseCode
               };
    }

    public static PaymentRefundResult Failed(string message, string gatewayResponseCode = null)
    {
        return new PaymentRefundResult
               {
                   Status = PaymentRefundResultStatus.Failed,
                   Message = message ?? string.Empty,
                   GatewayResponseCode = gatewayResponseCode
               };
    }
}
