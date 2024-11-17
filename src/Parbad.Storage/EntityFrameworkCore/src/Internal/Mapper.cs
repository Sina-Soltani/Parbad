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
        entity.IsCompleted = model.IsCompleted;
        entity.IsPaid = model.IsPaid;
    }

    public static Expression<Func<PaymentEntity, Payment>> ToPaymentModel()
    {
        return entity => new Payment
                         {
                             Id = entity.Id,
                             TrackingNumber = entity.TrackingNumber,
                             Amount = entity.Amount,
                             Token = entity.Token,
                             TransactionCode = entity.TransactionCode,
                             GatewayName = entity.GatewayName,
                             GatewayAccountName = entity.GatewayAccountName,
                             IsCompleted = entity.IsCompleted,
                             IsPaid = entity.IsPaid
                         };
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
                   IsCompleted = paymentEntity.IsCompleted,
                   IsPaid = paymentEntity.IsPaid
               };
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
