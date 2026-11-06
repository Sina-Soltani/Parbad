// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Storage.Abstractions.Models;
using Parbad.Storage.EntityFrameworkCore.Domain;
using System;
using System.Linq.Expressions;

namespace Parbad.Storage.EntityFrameworkCore.Internal;

internal static class Mapper
{
    public static PaymentEntity ToPaymentEntity(this Payment model)
    {
        return new PaymentEntity
               {
                   TrackingNumber = model.TrackingNumber,
                   Amount = model.Amount,
                   Token = model.Token,
                   TransactionCode = model.TransactionCode,
                   GatewayName = model.GatewayName,
                   GatewayAccountName = model.GatewayAccountName,
                   BankReference = model.BankReference,
                   Status = model.Status,
                   RefundedAmount = model.RefundedAmount,
                   IsCompleted = model.IsCompleted,
                   IsPaid = model.IsPaid
               };
    }

    public static void ToPaymentEntity(Payment model, PaymentEntity entity)
    {
        entity.TrackingNumber = model.TrackingNumber;
        entity.Amount = model.Amount;
        entity.Token = model.Token;
        entity.TransactionCode = model.TransactionCode;
        entity.GatewayName = model.GatewayName;
        entity.GatewayAccountName = model.GatewayAccountName;
        entity.BankReference = model.BankReference;
        entity.Status = model.Status;
        entity.RefundedAmount = model.RefundedAmount;
        entity.IsCompleted = model.IsCompleted;
        entity.IsPaid = model.IsPaid;
    }


    public static Payment ToPaymentModel(this PaymentEntity paymentEntity)
    {
        if (paymentEntity == null) throw new ArgumentNullException(nameof(paymentEntity));

        return new()
               {
                   Id = paymentEntity.Id,
                   TrackingNumber = paymentEntity.TrackingNumber,
                   Amount = paymentEntity.Amount,
                   Token = paymentEntity.Token,
                   TransactionCode = paymentEntity.TransactionCode,
                   GatewayName = paymentEntity.GatewayName,
                   GatewayAccountName = paymentEntity.GatewayAccountName,
                   BankReference = paymentEntity.BankReference,
                   Status = GetStatus(paymentEntity),
                   RefundedAmount = paymentEntity.RefundedAmount
               };
    }

    /// <summary>
    /// A row written before Parbad 4.0 has no status yet (Requested) but may be completed; its status then follows
    /// from <see cref="PaymentEntity.IsCompleted"/> and <see cref="PaymentEntity.IsPaid"/>.
    /// </summary>
    private static PaymentStatus GetStatus(PaymentEntity entity)
    {
        if (entity.Status != PaymentStatus.Requested || !entity.IsCompleted) return entity.Status;

        return entity.IsPaid ? PaymentStatus.Paid : PaymentStatus.Failed;
    }

    public static TransactionEntity ToTransactionEntity(this Transaction model)
    {
        return new TransactionEntity
               {
                   Amount = model.Amount,
                   Type = model.Type,
                   IsSucceed = model.IsSucceed,
                   Message = model.Message,
                   AdditionalData = model.AdditionalData,
                   PaymentId = model.PaymentId
               };
    }

    public static void ToTransactionEntity(Transaction model, TransactionEntity entity)
    {
        entity.Amount = model.Amount;
        entity.Type = model.Type;
        entity.IsSucceed = model.IsSucceed;
        entity.Message = model.Message;
        entity.AdditionalData = model.AdditionalData;
    }

    public static Expression<Func<TransactionEntity, Transaction>> ToTransactionModel()
    {
        return entity => new Transaction
                         {
                             Id = entity.Id,
                             Amount = entity.Amount,
                             Type = entity.Type,
                             IsSucceed = entity.IsSucceed,
                             Message = entity.Message,
                             AdditionalData = entity.AdditionalData,
                             PaymentId = entity.PaymentId
                         };
    }
}
