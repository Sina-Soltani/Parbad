// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Abstraction;

namespace Parbad
{
    /// <summary>
    /// Provides an easy solution to perform payment request, verify the requested payment and
    /// refund a payment.
    /// </summary>
    public interface IOnlinePayment
    {
        /// <summary>
        /// Defines a mechanism for retrieving a service object; that is, an object that
        /// provides custom support to other objects.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Performs a payment request using the given <paramref name="invoice"/>.
        /// </summary>
        /// <param name="invoice">The invoice that must be paid.</param>
        /// <param name="cancellationToken"></param>
        Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies the requested payment to check whether or not the invoice has was paid in the gateway by the client.
        /// </summary>
        /// <param name="context">Describes the information of requested payment.</param>
        /// <param name="cancellationToken"></param>
        Task<IPaymentVerifyResult> VerifyAsync(Action<IPaymentVerifyingContext> context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs a refund request for the given invoice.
        /// </summary>
        /// <param name="invoice">The invoice that must be refunded.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IPaymentRefundResult> RefundAsync(RefundInvoice invoice, CancellationToken cancellationToken = default);
    }
}
