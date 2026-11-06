// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Parbad.Storage.Abstractions.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Storage.Abstractions;

/// <summary>
/// A storage which used by Parbad for saving and loading data.
/// </summary>
public interface IStorage
{
    /// <summary>
    /// Creates a new payment.
    /// </summary>
    /// <param name="payment"></param>
    /// <param name="cancellationToken"></param>
    Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the given <paramref name="payment"/>.
    /// </summary>
    /// <param name="payment"></param>
    /// <param name="cancellationToken"></param>
    Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the given <paramref name="payment"/>.
    /// </summary>
    /// <param name="payment"></param>
    /// <param name="cancellationToken"></param>
    Task DeletePaymentAsync(Payment payment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new transaction.
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the given <paramref name="transaction"/>.
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    Task UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the given <paramref name="transaction"/>.
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    Task DeleteTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a payment by its tracking number.
    /// </summary>
    /// <param name="trackingNumber"></param>
    /// <param name="cancellationToken"></param>
    Task<Payment?> GetPaymentByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a payment by its token.
    /// </summary>
    /// <param name="paymentToken"></param>
    /// <param name="cancellationToken"></param>
    Task<Payment?> GetPaymentByTokenAsync(string paymentToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a payment exists with the given <paramref name="trackingNumber"/>.
    /// </summary>
    /// <param name="trackingNumber"></param>
    /// <param name="cancellationToken"></param>
    Task<bool> DoesPaymentExistAsync(long trackingNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a payment exists with the given <paramref name="paymentToken"/>.
    /// </summary>
    /// <param name="paymentToken"></param>
    /// <param name="cancellationToken"></param>
    Task<bool> DoesPaymentExistAsync(string paymentToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the largest tracking number of all payments, or <see langword="null"/> when there are no payments.
    /// </summary>
    /// <param name="cancellationToken"></param>
    Task<long?> GetLargestTrackingNumberAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paid payment of the gateway <paramref name="gatewayName"/> that the bank reference
    /// <paramref name="bankReference"/> settled, or null when no such payment exists. Parbad uses it to reject a
    /// verification whose bank transaction already settled another invoice, so one payment cannot settle two orders.
    /// <para>
    /// Payments stored before Parbad 4.0 have no <see cref="Payment.BankReference"/>; for them, the
    /// <see cref="Payment.TransactionCode"/> is compared instead.
    /// </para>
    /// </summary>
    /// <param name="gatewayName">The gateway of the payment. Different gateways may issue the same reference.</param>
    /// <param name="bankReference">The bank reference to look for.</param>
    /// <param name="exceptTrackingNumber">The invoice that is being verified, which is never a match.</param>
    /// <param name="cancellationToken"></param>
    Task<Payment?> FindPaidPaymentByBankReferenceAsync(string gatewayName,
                                                       string bankReference,
                                                       long exceptTrackingNumber,
                                                       CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes the payment with the id <paramref name="paymentId"/> with <paramref name="status"/>
    /// (<see cref="PaymentStatus.Paid"/>, <see cref="PaymentStatus.Failed"/> or <see cref="PaymentStatus.Cancelled"/>),
    /// in one atomic step, if the stored payment allows it:
    /// <list type="bullet">
    /// <item>a payment that is not completed can be completed with any status;</item>
    /// <item>a failed or cancelled payment can only be completed again as paid, because the bank confirmed the money;</item>
    /// <item>a paid payment is never changed.</item>
    /// </list>
    /// Two verifications of the same payment can run at the same time, e.g. after a refreshed callback page or on
    /// several servers; this guarantees that the slower one cannot overwrite the result of the faster one.
    /// </summary>
    /// <param name="paymentId"></param>
    /// <param name="status">The new status.</param>
    /// <param name="transactionCode">The transaction code to store.</param>
    /// <param name="bankReference">The bank reference to store; set for a paid payment only.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>True if the payment was updated; false if the stored payment did not allow it, in which case nothing
    /// is changed.</returns>
    /// <exception cref="InvalidOperationException">No payment has the id <paramref name="paymentId"/>.</exception>
    Task<bool> TryCompletePaymentAsync(long paymentId,
                                       PaymentStatus status,
                                       string? transactionCode,
                                       string? bankReference,
                                       CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of transactions of the given <paramref name="payment"/>.
    /// </summary>
    /// <param name="payment"></param>
    /// <param name="cancellationToken"></param>
    Task<List<Transaction>> GetTransactionsAsync(Payment payment, CancellationToken cancellationToken = default);
}
