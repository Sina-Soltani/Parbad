// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Storage.Abstractions
{
    public interface IStorageManager
    {
        Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default);

        Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default);

        Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);

        Task UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);

        Task<Payment> GetPaymentByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default);

        Task<Payment> GetPaymentByTokenAsync(string paymentToken, CancellationToken cancellationToken = default);

        Task<bool> DoesPaymentExistAsync(long trackingNumber, CancellationToken cancellationToken = default);

        Task<bool> DoesPaymentExistAsync(string paymentToken, CancellationToken cancellationToken = default);

        Task<List<Transaction>> GetTransactionsAsync(Payment payment, CancellationToken cancellationToken = default);
    }
}
