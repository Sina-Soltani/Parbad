// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Storage.Abstractions.Models;
using Parbad.Storage.EntityFrameworkCore.Domain;

namespace Parbad.Storage.EntityFrameworkCore.Internal
{
    internal static class Mapper
    {
        public static PaymentEntity ToEntity(this Payment payment)
        {
            return new PaymentEntity
            {
                TrackingNumber = payment.TrackingNumber,
                Amount = payment.Amount,
                Token = payment.Token,
                TransactionCode = payment.TransactionCode,
                GatewayName = payment.GatewayName,
                GatewayAccountName = payment.GatewayAccountName,
                IsCompleted = payment.IsCompleted,
                IsPaid = payment.IsPaid
            };
        }

        public static void ToEntity(Payment dataRecord, PaymentEntity dbRecord)
        {
            dbRecord.TrackingNumber = dataRecord.TrackingNumber;
            dbRecord.Amount = dataRecord.Amount;
            dbRecord.Token = dataRecord.Token;
            dbRecord.TransactionCode = dataRecord.TransactionCode;
            dbRecord.GatewayName = dataRecord.GatewayName;
            dbRecord.GatewayAccountName = dataRecord.GatewayAccountName;
            dbRecord.IsCompleted = dataRecord.IsCompleted;
            dbRecord.IsPaid = dataRecord.IsPaid;
        }

        public static Payment ToModel(this PaymentEntity payment)
        {
            return new Payment
            {
                TrackingNumber = payment.TrackingNumber,
                Amount = payment.Amount,
                Token = payment.Token,
                TransactionCode = payment.TransactionCode,
                GatewayName = payment.GatewayName,
                GatewayAccountName = payment.GatewayAccountName,
                IsCompleted = payment.IsCompleted,
                IsPaid = payment.IsPaid
            };
        }

        public static TransactionEntity ToEntity(this Transaction transaction)
        {
            return new TransactionEntity
            {
                Amount = transaction.Amount,
                Type = transaction.Type,
                IsSucceed = transaction.IsSucceed,
                Message = transaction.Message,
                AdditionalData = transaction.AdditionalData,
                PaymentId = transaction.PaymentId
            };
        }

        public static void ToEntity(Transaction dataRecord, TransactionEntity dbRecord)
        {
            dbRecord.Amount = dataRecord.Amount;
            dbRecord.Type = dataRecord.Type;
            dbRecord.IsSucceed = dataRecord.IsSucceed;
            dbRecord.Message = dataRecord.Message;
            dbRecord.AdditionalData = dataRecord.AdditionalData;
        }

        public static Transaction ToModel(this TransactionEntity transaction)
        {
            return new Transaction
            {
                Amount = transaction.Amount,
                Type = transaction.Type,
                IsSucceed = transaction.IsSucceed,
                Message = transaction.Message,
                AdditionalData = transaction.AdditionalData,
                PaymentId = transaction.PaymentId
            };
        }
    }
}
