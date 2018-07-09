using System;
using Parbad.Core;
using Parbad.Infrastructure.Data.Providers;
using Parbad.Providers;

namespace Parbad.Infrastructure.Data
{
    internal static class PaymentDataExtensions
    {
        public static PaymentData CreatePaymentData(this Invoice invoice, Gateway gateway)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }

            return new PaymentData
            {
                Id = Guid.NewGuid(),
                Gateway = gateway,
                Amount = invoice.Amount,
                OrderNumber = invoice.OrderNumber,
                CreatedOn = DateTime.Now,
                Status = PaymentDataStatus.Requested
            };
        }

        public static bool IsExpired(this PaymentData paymentData)
        {
            if (paymentData == null)
            {
                throw new ArgumentNullException(nameof(paymentData));
            }

            return DateTime.Now.Subtract(paymentData.CreatedOn).TotalMinutes >
                   TemporaryMemoryStorage.DefaultInvoiceLifetime.TotalMinutes;
        }
    }
}