// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Storage.Abstractions;

/// <summary>
/// An optional capability of an <see cref="IStorage"/>: completing a payment in one atomic step, only if no other
/// operation completed it in a way that must be kept.
/// <para>
/// Two verifications of the same payment can run at the same time, e.g. after a double click, a refreshed callback
/// page, a bank that retries the callback, or with several servers. A storage that implements this lets Parbad make
/// sure the slower one cannot overwrite the result of the faster one. Storages that do not implement it keep
/// working; Parbad then writes the result without this guard and logs that.
/// </para>
/// </summary>
public interface IPaymentCompletion
{
    /// <summary>
    /// Marks the payment with the id <paramref name="paymentId"/> as completed and stores <paramref name="isPaid"/>
    /// and <paramref name="transactionCode"/>, if the stored payment allows it:
    /// <list type="bullet">
    /// <item>a payment that is not completed can be completed with any result;</item>
    /// <item>a payment that is completed but not paid can only be completed again as paid, because the bank
    /// confirmed the money;</item>
    /// <item>a paid payment is never changed.</item>
    /// </list>
    /// </summary>
    /// <returns>True if the payment was updated; false if the stored payment did not allow it, in which case
    /// nothing is changed.</returns>
    Task<bool> TryCompletePaymentAsync(long paymentId,
                                       bool isPaid,
                                       string? transactionCode,
                                       CancellationToken cancellationToken = default);
}
