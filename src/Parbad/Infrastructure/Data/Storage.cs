using System;
using System.Threading.Tasks;

namespace Parbad.Infrastructure.Data
{
    /// <summary>
    /// Storage for saving and loading payment's data.
    /// </summary>
    public abstract class Storage
    {
        protected internal abstract PaymentData SelectById(Guid id);
        protected internal abstract Task<PaymentData> SelectByIdAsync(Guid id);

        protected internal abstract PaymentData SelectByOrderNumber(long orderNumber);
        protected internal abstract Task<PaymentData> SelectByOrderNumberAsync(long orderNumber);

        protected internal abstract void Insert(PaymentData paymentData);
        protected internal abstract Task InsertAsync(PaymentData paymentData);

        protected internal abstract void Update(PaymentData paymentData);
        protected internal abstract Task UpdateAsync(PaymentData paymentData);
    }
}