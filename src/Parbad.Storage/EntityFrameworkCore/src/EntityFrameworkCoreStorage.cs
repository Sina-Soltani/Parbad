// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Abstractions.Models;
using Parbad.Storage.EntityFrameworkCore.Configuration;
using Parbad.Storage.EntityFrameworkCore.Context;
using Parbad.Storage.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Storage.EntityFrameworkCore;

/// <summary>
/// EntityFramework Core implementation of <see cref="IStorage"/>.
/// </summary>
public class EntityFrameworkCoreStorage : IStorage, IPaidPaymentLookup, IPaymentCompletion
{
    /// <summary>
    /// Initializes an instance of <see cref="EntityFrameworkCoreStorage"/>.
    /// </summary>
    /// <param name="context"></param>
    public EntityFrameworkCoreStorage(ParbadDataContext context)
    {
        Context = context;
    }

    /// <inheritdoc />
    [Obsolete("This property will be removed in a future release. The usages are implemented as methods now.")]
    public virtual IQueryable<Payment> Payments => Context.Payments.Select(Mapper.ToPaymentModel());

    /// <inheritdoc />
    [Obsolete("This property will be removed in a future release. The usages are implemented as methods now.")]
    public virtual IQueryable<Transaction> Transactions => Context.Transactions.Select(Mapper.ToTransactionModel());

    /// <summary>
    /// Gets Parbad DbContext.
    /// </summary>
    protected ParbadDataContext Context { get; }

    /// <inheritdoc />
    public virtual async Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (payment == null) throw new ArgumentNullException(nameof(payment));

        var entity = payment.ToPaymentEntity();
        entity.CreatedOn = DateTime.UtcNow;

        Context.Payments.Add(entity);

        await Context.SaveChangesAsync(cancellationToken);

        Context.Entry(entity).State = EntityState.Detached;

        payment.Id = entity.Id;
    }

    /// <inheritdoc />
    public virtual async Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (payment == null) throw new ArgumentNullException(nameof(payment));

        var record = await Context
                          .Payments
                          .AsNoTracking()
                          .SingleOrDefaultAsync(model => model.Id == payment.Id, cancellationToken);

        if (record == null) throw new InvalidOperationException($"No payment records found in database with id {payment.Id}");

        Mapper.ToPaymentEntity(payment, record);
        record.UpdatedOn = DateTime.UtcNow;

        Context.Payments.Update(record);

        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task DeletePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (payment == null) throw new ArgumentNullException(nameof(payment));

        var record = await Context
                          .Payments
                          .AsNoTracking()
                          .SingleOrDefaultAsync(model => model.Id == payment.Id, cancellationToken);

        if (record == null) throw new InvalidOperationException($"No payment records found in database with id {payment.Id}");

        Context.Payments.Remove(record);

        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));

        var entity = transaction.ToTransactionEntity();
        entity.CreatedOn = DateTime.UtcNow;

        Context.Transactions.Add(entity);

        await Context.SaveChangesAsync(cancellationToken);

        transaction.Id = entity.Id;
    }

    /// <inheritdoc />
    public virtual async Task UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));

        var record = await Context
                          .Transactions
                          .SingleOrDefaultAsync(model => model.Id == transaction.Id, cancellationToken);

        if (record == null) throw new InvalidOperationException($"No transaction records found in database with id {transaction.Id}");

        Mapper.ToTransactionEntity(transaction, record);
        record.UpdatedOn = DateTime.UtcNow;

        Context.Transactions.Update(record);

        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task DeleteTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));

        var record = await Context
                          .Transactions
                          .SingleOrDefaultAsync(model => model.Id == transaction.Id, cancellationToken);

        if (record == null) throw new InvalidOperationException($"No transaction records found in database with id {transaction.Id}");

        Context.Transactions.Remove(record);

        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<Payment?> GetPaymentByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default)
    {
        var paymentEntity = await Context.Payments
                                         .AsNoTracking()
                                         .SingleOrDefaultAsync(payment => payment.TrackingNumber == trackingNumber, cancellationToken);

        return paymentEntity?.ToPaymentModel();
    }

    /// <inheritdoc />
    public virtual async Task<Payment?> GetPaymentByTokenAsync(string paymentToken, CancellationToken cancellationToken = default)
    {
        var paymentEntity = await Context.Payments
                                         .AsNoTracking()
                                         .SingleOrDefaultAsync(payment => payment.Token == paymentToken, cancellationToken);

        return paymentEntity?.ToPaymentModel();
    }

    /// <inheritdoc />
    public virtual async Task<Payment?> FindPaidPaymentByTransactionCodeAsync(string gatewayName,
                                                                              string transactionCode,
                                                                              long exceptTrackingNumber,
                                                                              CancellationToken cancellationToken = default)
    {
        var paymentEntity = await Context.Payments
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(payment => payment.IsPaid &&
                                                                         payment.TrackingNumber != exceptTrackingNumber &&
                                                                         payment.TransactionCode == transactionCode &&
                                                                         payment.GatewayName == gatewayName,
                                                              cancellationToken);

        return paymentEntity?.ToPaymentModel();
    }

    /// <inheritdoc />
    public virtual async Task<bool> TryCompletePaymentAsync(long paymentId,
                                                            bool isPaid,
                                                            string? transactionCode,
                                                            CancellationToken cancellationToken = default)
    {
        if (!Context.Database.IsRelational())
        {
            return await TryCompletePaymentWithoutSqlAsync(paymentId, isPaid, transactionCode, cancellationToken);
        }

        // One conditional UPDATE, so two concurrent completions cannot both succeed, without a schema change.
        var sql = Context.GetService<ISqlGenerationHelper>();
        var options = Context.EntityFrameworkCoreOptions;
        var schema = string.IsNullOrWhiteSpace(options.PaymentTableOptions.Schema) ? options.DefaultSchema : options.PaymentTableOptions.Schema;

        string Column(string name) => sql.DelimitIdentifier(name);

        // {5} is false. A paid result may replace an unpaid one; any other result needs an open payment.
        var condition = isPaid
            ? $"{Column(PaymentColumnNames.IsPaid)} = {{5}}"
            : $"{Column(PaymentColumnNames.IsCompleted)} = {{5}} AND {Column(PaymentColumnNames.IsPaid)} = {{5}}";

        var command = $"UPDATE {sql.DelimitIdentifier(options.PaymentTableOptions.Name, schema)} " +
                      $"SET {Column(PaymentColumnNames.IsCompleted)} = {{0}}, {Column(PaymentColumnNames.IsPaid)} = {{1}}, " +
                      $"{Column(PaymentColumnNames.TransactionCode)} = {(transactionCode == null ? "NULL" : "{2}")}, {Column(PaymentColumnNames.UpdatedOn)} = {{3}} " +
                      $"WHERE {Column(PaymentColumnNames.Id)} = {{4}} AND {condition}";

        // A null parameter has no type mapping on every provider, so a missing transaction code is written as NULL.
        var parameters = new object[] { true, isPaid, transactionCode ?? string.Empty, DateTime.UtcNow, paymentId, false };

        var updatedRows = await Context.Database.ExecuteSqlRawAsync(command, parameters, cancellationToken);

        if (updatedRows == 0 && !await Context.Payments.AnyAsync(payment => payment.Id == paymentId, cancellationToken))
        {
            throw new InvalidOperationException($"No payment records found in database with id {paymentId}");
        }

        return updatedRows > 0;
    }

    /// <summary>
    /// The same rule as <see cref="TryCompletePaymentAsync"/> for providers without SQL (e.g. InMemory), where it
    /// cannot be one atomic step.
    /// </summary>
    private async Task<bool> TryCompletePaymentWithoutSqlAsync(long paymentId,
                                                               bool isPaid,
                                                               string? transactionCode,
                                                               CancellationToken cancellationToken)
    {
        var record = await Context.Payments.SingleOrDefaultAsync(payment => payment.Id == paymentId, cancellationToken);

        if (record == null) throw new InvalidOperationException($"No payment records found in database with id {paymentId}");

        try
        {
            if (record.IsPaid || (record.IsCompleted && !isPaid)) return false;

            record.IsCompleted = true;
            record.IsPaid = isPaid;
            record.TransactionCode = transactionCode;
            record.UpdatedOn = DateTime.UtcNow;

            await Context.SaveChangesAsync(cancellationToken);

            return true;
        }
        finally
        {
            Context.Entry(record).State = EntityState.Detached;
        }
    }

    /// <inheritdoc />
    public virtual Task<bool> DoesPaymentExistAsync(long trackingNumber, CancellationToken cancellationToken = default)
    {
        return Context.Payments.AnyAsync(payment => payment.TrackingNumber == trackingNumber, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<bool> DoesPaymentExistAsync(string paymentToken, CancellationToken cancellationToken = default)
    {
        return Context.Payments.AnyAsync(payment => payment.Token == paymentToken, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<List<Transaction>> GetTransactionsAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        return Context.Transactions
                      .Where(transaction => transaction.PaymentId == payment.Id)
                      .AsNoTracking()
                      .Select(Mapper.ToTransactionModel())
                      .ToListAsync(cancellationToken);
    }
}
