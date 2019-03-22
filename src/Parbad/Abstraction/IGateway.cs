// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Parbad.Data.Domain.Payments;

namespace Parbad.Abstraction
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGateway
    {
        /// <summary>
        /// Performs a payment request using the given <paramref name="invoice"/>.
        /// </summary>
        /// <param name="invoice">The invoice which must be pay.</param>
        /// <param name="cancellationToken"></param>
        Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies the requested payment to check whether or not the invoice has was paid in the gateway by the client.
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="cancellationToken"></param>
        Task<IPaymentVerifyResult> VerifyAsync(Payment payment, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs a refund request for the given invoice.
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="amount"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IPaymentRefundResult> RefundAsync(Payment payment, Money amount, CancellationToken cancellationToken = default);
    }
}
