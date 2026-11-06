// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Abstractions.Models;
using Parbad.Storage.EntityFrameworkCore.Domain;
using Parbad.Storage.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Storage.EntityFrameworkCore;

/// <summary>
/// EntityFramework Core implementation of <see cref="IStorage"/>, on any <see cref="DbContext"/> whose model contains
/// Parbad's entities (<c>modelBuilder.UseParbad()</c>).
/// <para>
/// Every operation works on a new, short-lived context from <see cref="IDbContextFactory{TContext}"/>. Parbad's
/// <c>SaveChanges</c> therefore only saves Parbad's own entities, never changes that the application has pending on
/// its own context instance, and it does not take part in a transaction the application opened there.
/// </para>
/// </summary>
/// <typeparam name="TContext">The context that contains Parbad's entities.</typeparam>
public class EntityFrameworkCoreStorage<TContext> : IStorage where TContext : DbContext
{
    private readonly IDbContextFactory<TContext> _contextFactory;
    private volatile bool _modelChecked;

    /// <summary>
    /// Initializes an instance of <see cref="EntityFrameworkCoreStorage{TContext}"/>.
    /// </summary>
    public EntityFrameworkCoreStorage(IDbContextFactory<TContext> contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    /// <inheritdoc />
    public virtual async Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (payment == null) throw new ArgumentNullException(nameof(payment));

        await using var context = await CreateContextAsync(cancellationToken);

        var entity = payment.ToPaymentEntity();
        entity.CreatedOn = DateTime.UtcNow;

        context.Set<PaymentEntity>().Add(entity);

        await context.SaveChangesAsync(cancellationToken);

        payment.Id = entity.Id;
    }

    /// <inheritdoc />
    public virtual async Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (payment == null) throw new ArgumentNullException(nameof(payment));

        await using var context = await CreateContextAsync(cancellationToken);

        var record = await context.Set<PaymentEntity>().SingleOrDefaultAsync(model => model.Id == payment.Id, cancellationToken);

        if (record == null) throw new InvalidOperationException($"No payment records found in database with id {payment.Id}");

        Mapper.ToPaymentEntity(payment, record);
        record.UpdatedOn = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task DeletePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (payment == null) throw new ArgumentNullException(nameof(payment));

        await using var context = await CreateContextAsync(cancellationToken);

        var record = await context.Set<PaymentEntity>().SingleOrDefaultAsync(model => model.Id == payment.Id, cancellationToken);

        if (record == null) throw new InvalidOperationException($"No payment records found in database with id {payment.Id}");

        context.Set<PaymentEntity>().Remove(record);

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));

        await using var context = await CreateContextAsync(cancellationToken);

        var entity = transaction.ToTransactionEntity();
        entity.CreatedOn = DateTime.UtcNow;

        context.Set<TransactionEntity>().Add(entity);

        await context.SaveChangesAsync(cancellationToken);

        transaction.Id = entity.Id;
    }

    /// <inheritdoc />
    public virtual async Task UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));

        await using var context = await CreateContextAsync(cancellationToken);

        var record = await context.Set<TransactionEntity>().SingleOrDefaultAsync(model => model.Id == transaction.Id, cancellationToken);

        if (record == null) throw new InvalidOperationException($"No transaction records found in database with id {transaction.Id}");

        Mapper.ToTransactionEntity(transaction, record);
        record.UpdatedOn = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task DeleteTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));

        await using var context = await CreateContextAsync(cancellationToken);

        var record = await context.Set<TransactionEntity>().SingleOrDefaultAsync(model => model.Id == transaction.Id, cancellationToken);

        if (record == null) throw new InvalidOperationException($"No transaction records found in database with id {transaction.Id}");

        context.Set<TransactionEntity>().Remove(record);

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<Payment?> GetPaymentByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);

        var paymentEntity = await context.Set<PaymentEntity>()
                                         .AsNoTracking()
                                         .SingleOrDefaultAsync(payment => payment.TrackingNumber == trackingNumber, cancellationToken);

        return paymentEntity?.ToPaymentModel();
    }

    /// <inheritdoc />
    public virtual async Task<Payment?> GetPaymentByTokenAsync(string paymentToken, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);

        var paymentEntity = await context.Set<PaymentEntity>()
                                         .AsNoTracking()
                                         .SingleOrDefaultAsync(payment => payment.Token == paymentToken, cancellationToken);

        return paymentEntity?.ToPaymentModel();
    }

    /// <inheritdoc />
    public virtual async Task<Payment?> FindPaidPaymentByBankReferenceAsync(string gatewayName,
                                                                            string bankReference,
                                                                            long exceptTrackingNumber,
                                                                            CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);

        // Payments stored before Parbad 4.0 have no bank reference; their transaction code is compared instead.
        var paymentEntity = await context.Set<PaymentEntity>()
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(payment => payment.IsPaid &&
                                                                         payment.TrackingNumber != exceptTrackingNumber &&
                                                                         payment.GatewayName == gatewayName &&
                                                                         (payment.BankReference == bankReference ||
                                                                          (payment.BankReference == null && payment.TransactionCode == bankReference)),
                                                              cancellationToken);

        return paymentEntity?.ToPaymentModel();
    }

    /// <inheritdoc />
    public virtual async Task<bool> TryCompletePaymentAsync(long paymentId,
                                                            PaymentStatus status,
                                                            string? transactionCode,
                                                            string? bankReference,
                                                            CancellationToken cancellationToken = default)
    {
        if (status is not (PaymentStatus.Paid or PaymentStatus.Failed or PaymentStatus.Cancelled))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "A payment is completed as paid, failed or cancelled.");
        }

        var isPaid = status == PaymentStatus.Paid;

        await using var context = await CreateContextAsync(cancellationToken);

        var payments = context.Set<PaymentEntity>();

        // A paid result may replace an unpaid one; any other result needs an open payment.
        var completable = isPaid
            ? payments.Where(payment => payment.Id == paymentId && !payment.IsPaid)
            : payments.Where(payment => payment.Id == paymentId && !payment.IsCompleted && !payment.IsPaid);

        bool updated;

        if (context.Database.IsRelational())
        {
            updated = await TryCompleteWithSqlAsync(context, paymentId, status, transactionCode, bankReference, cancellationToken) > 0;
        }
        else
        {
            // Providers without SQL (e.g. InMemory): the same rule, but not one atomic step.
            var record = await completable.SingleOrDefaultAsync(cancellationToken);

            if (record != null)
            {
                record.Status = status;
                record.IsCompleted = true;
                record.IsPaid = isPaid;
                record.TransactionCode = transactionCode;
                record.BankReference = bankReference;
                record.UpdatedOn = DateTime.UtcNow;

                await context.SaveChangesAsync(cancellationToken);
            }

            updated = record != null;
        }

        if (!updated && !await payments.AnyAsync(payment => payment.Id == paymentId, cancellationToken))
        {
            throw new InvalidOperationException($"No payment records found in database with id {paymentId}");
        }

        return updated;
    }

    /// <summary>
    /// One conditional UPDATE, so two concurrent completions cannot both succeed. The table and column names come from
    /// the model, so they follow the application's <c>UseParbad(...)</c> options.
    /// <para>
    /// Plain SQL instead of <c>ExecuteUpdateAsync</c>: its binary signature differs between EF Core 8, 9 and 10, and
    /// this package is built against EF Core 8 but runs with whichever EF Core the application uses.
    /// </para>
    /// </summary>
    private static Task<int> TryCompleteWithSqlAsync(TContext context,
                                                     long paymentId,
                                                     PaymentStatus status,
                                                     string? transactionCode,
                                                     string? bankReference,
                                                     CancellationToken cancellationToken)
    {
        var sql = context.GetService<ISqlGenerationHelper>();
        var entityType = context.Model.FindEntityType(typeof(PaymentEntity))!;
        var table = sql.DelimitIdentifier(entityType.GetTableName()!, entityType.GetSchema());

        string Column(string propertyName) => sql.DelimitIdentifier(entityType.FindProperty(propertyName)!.GetColumnName()!);

        var id = Column(nameof(PaymentEntity.Id));
        var isCompletedColumn = Column(nameof(PaymentEntity.IsCompleted));
        var isPaidColumn = Column(nameof(PaymentEntity.IsPaid));
        var transactionCodeColumn = Column(nameof(PaymentEntity.TransactionCode));
        var bankReferenceColumn = Column(nameof(PaymentEntity.BankReference));
        var statusColumn = Column(nameof(PaymentEntity.Status));
        var isPaid = status == PaymentStatus.Paid;
        var updatedOn = Column(nameof(PaymentEntity.UpdatedOn));

        // {5} is false. A paid result may replace an unpaid one; any other result needs an open payment. The condition
        // uses is_completed/is_paid, which payments stored before Parbad 4.0 have too.
        var condition = isPaid
            ? $"{isPaidColumn} = {{5}}"
            : $"{isCompletedColumn} = {{5}} AND {isPaidColumn} = {{5}}";

        var command = $"UPDATE {table} " +
                      $"SET {statusColumn} = {{6}}, {isCompletedColumn} = {{0}}, {isPaidColumn} = {{1}}, " +
                      $"{transactionCodeColumn} = {(transactionCode == null ? "NULL" : "{2}")}, " +
                      $"{bankReferenceColumn} = {(bankReference == null ? "NULL" : "{7}")}, {updatedOn} = {{3}} " +
                      $"WHERE {id} = {{4}} AND {condition}";

        // A null parameter has no type mapping on every provider, so missing values are written as NULL.
        var parameters = new object[]
                         {
                             true, isPaid, transactionCode ?? string.Empty, DateTime.UtcNow, paymentId, false, (byte)status, bankReference ?? string.Empty
                         };

        return context.Database.ExecuteSqlRawAsync(command, parameters, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> DoesPaymentExistAsync(long trackingNumber, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);

        return await context.Set<PaymentEntity>().AnyAsync(payment => payment.TrackingNumber == trackingNumber, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> DoesPaymentExistAsync(string paymentToken, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);

        return await context.Set<PaymentEntity>().AnyAsync(payment => payment.Token == paymentToken, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<long?> GetLargestTrackingNumberAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);

        return await context.Set<PaymentEntity>().MaxAsync(payment => (long?)payment.TrackingNumber, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<List<Transaction>> GetTransactionsAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (payment == null) throw new ArgumentNullException(nameof(payment));

        await using var context = await CreateContextAsync(cancellationToken);

        return await context.Set<TransactionEntity>()
                            .Where(transaction => transaction.PaymentId == payment.Id)
                            .AsNoTracking()
                            .Select(Mapper.ToTransactionModel())
                            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Creates the context for one operation. The caller disposes it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The model of <typeparamref name="TContext"/> has no Parbad entities.</exception>
    protected virtual async Task<TContext> CreateContextAsync(CancellationToken cancellationToken)
    {
        var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        if (!_modelChecked)
        {
            if (context.Model.FindEntityType(typeof(PaymentEntity)) == null)
            {
                await context.DisposeAsync();

                throw new InvalidOperationException($"The model of {typeof(TContext).Name} does not contain Parbad's entities. " +
                                                    $"Call modelBuilder.UseParbad() in {typeof(TContext).Name}.OnModelCreating.");
            }

            _modelChecked = true;
        }

        return context;
    }
}
