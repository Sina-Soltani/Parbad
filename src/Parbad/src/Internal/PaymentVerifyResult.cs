// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Internal;

public class PaymentVerifyResult : PaymentResult, IPaymentVerifyResult
{
    public string TransactionCode { get; set; }

    public PaymentVerifyResultStatus Status { get; set; }

    public override bool IsSucceed => Status == PaymentVerifyResultStatus.Succeed;

    /// <summary>
    /// The amount the bank reported for the verified transaction, in Rial, or null when the bank does not report it.
    /// When set, a successful verification is accepted only if it equals the amount of the invoice.
    /// </summary>
    public Money? BankReportedAmount { get; set; }

    /// <summary>
    /// The order id (tracking number) the bank reported for the verified transaction, or null when the bank does
    /// not report it. When set, a successful verification is accepted only if it equals the tracking number of the invoice.
    /// </summary>
    public string BankReportedOrderId { get; set; }

    /// <summary>
    /// Set by a gateway whose bank answered "already verified" and which cannot prove from the bank's answer that
    /// <see cref="TransactionCode"/> belongs to this invoice. The gateway returns a failed result; Parbad records it as
    /// paid only when the storage confirms that no other paid payment has the same <see cref="TransactionCode"/>.
    /// </summary>
    public bool IsAlreadyVerifiedAtBank { get; set; }

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
