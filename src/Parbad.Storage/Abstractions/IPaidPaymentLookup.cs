// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.Storage.Abstractions;

/// <summary>
/// An optional capability of an <see cref="IStorage"/>: finding a paid payment by the transaction code the
/// bank reported for it.
/// <para>
/// A storage that implements this lets Parbad reject a verification whose bank transaction already settled
/// another invoice, which is what prevents one payment from settling two orders. Storages that do not
/// implement it keep working; Parbad then logs that the check was skipped.
/// </para>
/// </summary>
public interface IPaidPaymentLookup
{
    /// <summary>
    /// Returns a paid payment of the gateway <paramref name="gatewayName"/> whose <see cref="Payment.TransactionCode"/>
    /// equals <paramref name="transactionCode"/>, or null when no such payment exists.
    /// </summary>
    /// <param name="gatewayName">The gateway of the payment. Different gateways may issue the same reference number.</param>
    /// <param name="transactionCode">The transaction code (bank reference) to look for.</param>
    /// <param name="exceptTrackingNumber">The invoice that is being verified, which is never a match.</param>
    /// <param name="cancellationToken"></param>
    Task<Payment?> FindPaidPaymentByTransactionCodeAsync(string gatewayName,
                                                         string transactionCode,
                                                         long exceptTrackingNumber,
                                                         CancellationToken cancellationToken = default);
}
