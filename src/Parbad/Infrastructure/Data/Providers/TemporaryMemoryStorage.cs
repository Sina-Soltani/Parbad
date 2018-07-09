using System;
using System.Linq;
using System.Threading.Tasks;
using Parbad.Infrastructure.Caching;

namespace Parbad.Infrastructure.Data.Providers
{
    /// <inheritdoc />
    /// <summary>
    /// An encrypted temporary memory that keeps data for a specific time.
    /// </summary>
    public class TemporaryMemoryStorage : Storage
    {
        private const string CachePrefixKey = "Parbad.Payment";
        private static readonly object LockObject = new object();

        /// <summary>
        /// Initializes new instance of TemporaryMemoryStorage
        /// </summary>
        /// <param name="invoiceLifeTime">The lifetime of each invoice. Minimum time is 20 minutes.</param>
        public TemporaryMemoryStorage(TimeSpan invoiceLifeTime)
        {
            if (invoiceLifeTime.TotalMinutes < 20)
            {
                throw new ArgumentOutOfRangeException(nameof(invoiceLifeTime), invoiceLifeTime.TotalMinutes, "Minimum time to keep data is 20 minutes.");
            }

            InvoiceLifeTime = invoiceLifeTime;
        }

        public static TimeSpan DefaultInvoiceLifetime => TimeSpan.FromMinutes(20);

        public TimeSpan InvoiceLifeTime { get; }

        protected internal override PaymentData SelectById(Guid id)
        {
            lock (LockObject)
            {
                var cacheKey = CreateCacheKey(id);

                var encryptedPaymentData = CacheManagerFactory.Instance.Get(cacheKey);

                return PaymentDataEncryptor.Decrypt(encryptedPaymentData.ToString());
            }
        }

        protected internal override Task<PaymentData> SelectByIdAsync(Guid id)
        {
            return Task.FromResult(SelectById(id));
        }

        protected internal override PaymentData SelectByOrderNumber(long orderNumber)
        {
            lock (LockObject)
            {
                var paymentCacheItems = CacheManagerFactory.Instance.GetAll().Where(item => item.Key.StartsWith(CachePrefixKey));

                foreach (var paymentCacheItem in paymentCacheItems)
                {
                    if (!PaymentDataEncryptor.TryDecrypt(paymentCacheItem.Value?.ToString(), out var paymentData))
                    {
                        continue;
                    }

                    if (paymentData?.OrderNumber == orderNumber)
                    {
                        return paymentData;
                    }
                }

                return null;
            }
        }

        protected internal override Task<PaymentData> SelectByOrderNumberAsync(long orderNumber)
        {
            return Task.FromResult(SelectByOrderNumber(orderNumber));
        }

        protected internal override void Insert(PaymentData paymentData)
        {
            Update(paymentData);
        }

        protected internal override Task InsertAsync(PaymentData paymentData)
        {
            return Task.Run(() => Insert(paymentData));
        }

        protected internal override void Update(PaymentData paymentData)
        {
            if (paymentData == null)
            {
                throw new ArgumentNullException(nameof(paymentData));
            }

            lock (LockObject)
            {
                var cacheKey = CreateCacheKey(paymentData.Id);

                CacheManagerFactory.Instance.AddOrUpdate(cacheKey, PaymentDataEncryptor.Encrypt(paymentData), InvoiceLifeTime);
            }
        }

        protected internal override Task UpdateAsync(PaymentData paymentData)
        {
            return Task.Run(() => Update(paymentData));
        }

        private static string CreateCacheKey(Guid id)
        {
            return $"{CachePrefixKey}#{id:N}";
        }
    }
}