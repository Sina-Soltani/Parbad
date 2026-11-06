// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

#nullable enable

namespace Parbad.Internal;

/// <summary>
/// The result of a verification.
/// <para>
/// A gateway reports what the bank verified, and Parbad decides: it compares <see cref="BankReportedAmount"/> and
/// <see cref="BankReportedOrderId"/> with the invoice, and accepts a <see cref="BankReference"/> only once per
/// gateway. Create a successful result with <see cref="Paid"/>, which requires the bank reference.
/// </para>
/// </summary>
public record PaymentVerifyResult : PaymentResult, IPaymentVerifyResult
{
    /// <inheritdoc />
    public string? TransactionCode { get; init; }

    /// <inheritdoc />
    public PaymentVerifyResultStatus Status { get; init; }

    /// <inheritdoc />
    public override bool IsSucceed => Status == PaymentVerifyResultStatus.Succeed;

    /// <inheritdoc />
    public string? BankReference { get; init; }

    /// <inheritdoc />
    public Money? BankReportedAmount { get; init; }

    /// <inheritdoc />
    public string? BankReportedOrderId { get; init; }

    /// <inheritdoc />
    public bool IsAlreadyVerifiedAtBank { get; init; }

    /// <summary>
    /// The bank confirmed the payment.
    /// </summary>
    /// <param name="transactionCode">The transaction code shown to the customer (e.g. the RRN).</param>
    /// <param name="bankReference">The reference the bank verified (often the same as
    /// <paramref name="transactionCode"/>). Parbad accepts it only once per gateway.</param>
    /// <param name="message"></param>
    /// <param name="gatewayResponseCode"></param>
    public static PaymentVerifyResult Paid(string? transactionCode,
                                           string bankReference,
                                           string? message,
                                           string? gatewayResponseCode = null)
    {
        return new PaymentVerifyResult
               {
                   Status = PaymentVerifyResultStatus.Succeed,
                   TransactionCode = transactionCode,
                   BankReference = bankReference,
                   Message = message,
                   GatewayResponseCode = gatewayResponseCode
               };
    }

    /// <summary>
    /// The bank answered "already verified", and the gateway cannot prove from the answer that the payment belongs to
    /// this invoice. Parbad records it as paid only if no other paid payment has the same
    /// <paramref name="bankReference"/>.
    /// </summary>
    public static PaymentVerifyResult AlreadyVerifiedAtBank(string? transactionCode,
                                                            string bankReference,
                                                            string? message,
                                                            string? gatewayResponseCode = null)
    {
        return new PaymentVerifyResult
               {
                   Status = PaymentVerifyResultStatus.Failed,
                   IsAlreadyVerifiedAtBank = true,
                   TransactionCode = transactionCode,
                   BankReference = bankReference,
                   Message = message,
                   GatewayResponseCode = gatewayResponseCode
               };
    }

    /// <summary>
    /// The verification failed.
    /// </summary>
    public static PaymentVerifyResult Failed(string? message, string? gatewayResponseCode = null)
    {
        return new PaymentVerifyResult
               {
                   Status = PaymentVerifyResultStatus.Failed,
                   Message = message,
                   GatewayResponseCode = gatewayResponseCode
               };
    }
}
