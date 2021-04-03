// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Abstraction;
using Parbad.Exceptions;
using Parbad.PaymentTokenProviders;

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
        /// Fetches the invoice from the incoming HTTP request.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <exception cref="PaymentTokenProviderException"></exception>
        /// <exception cref="InvoiceNotFoundException"></exception>
        Task<IPaymentFetchResult> FetchAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetches the invoice by the given tracking number.
        /// </summary>
        /// <param name="trackingNumber">Invoice's tracking number.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="PaymentTokenProviderException"></exception>
        /// <exception cref="InvoiceNotFoundException"></exception>
        Task<IPaymentFetchResult> FetchAsync(long trackingNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies the requested payment to check whether or not the invoice was paid in the gateway by the client.
        /// This method must be called when the fetch result equals to <see cref="PaymentFetchResultStatus.ReadyForVerifying"/>.
        /// </summary>
        /// <param name="trackingNumber">The tracking number of the invoice which must be verified.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        Task<IPaymentVerifyResult> VerifyAsync(long trackingNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancels the given invoice. No Verifying request will be sent to the gateway.
        /// </summary>
        /// <param name="trackingNumber">The tracking number of the invoice which must be verified.</param>
        /// <param name="cancellationReason">The reason for canceling the operation. It will be saved in Message field in database.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        Task<IPaymentCancelResult> CancelAsync(long trackingNumber, string cancellationReason = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs a refund request for the given invoice.
        /// </summary>
        /// <param name="invoice">The invoice that must be refunded.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        Task<IPaymentRefundResult> RefundAsync(RefundInvoice invoice, CancellationToken cancellationToken = default);
    }
}
