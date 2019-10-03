// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Abstraction;
using Parbad.Exceptions;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using Parbad.PaymentTokenProviders;

namespace Parbad
{
    public static class OnlinePaymentExtensions
    {
        /// <summary>
        /// Performs a new payment request with the given data.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="gateway">The gateway which the client must pay the invoice in.</param>
        /// <param name="trackingNumber">
        /// A tracking number for this request. It will be sent to the gateway.
        /// <para>Note: It must be unique for each requests.</para>
        /// </param>
        /// <param name="amount">The amount of the payment request.</param>
        /// <param name="callbackUrl">
        /// A complete URL of your website. It will be used by the gateway for redirecting the client again to your website.
        /// <para>A complete URL would be like: "http://www.mywebsite.com/foo/bar/"</para>
        /// </param>
        public static IPaymentRequestResult Request(
            this IOnlinePayment onlinePayment,
            Gateway gateway,
            long trackingNumber,
            decimal amount,
            string callbackUrl) =>
            onlinePayment.RequestAsync(gateway, trackingNumber, amount, callbackUrl)
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// Performs a new payment request with the given data.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="configureInvoice">A builder which helps to build an invoice.</param>
        public static IPaymentRequestResult Request(this IOnlinePayment onlinePayment, Action<IInvoiceBuilder> configureInvoice)
            => onlinePayment.RequestAsync(configureInvoice)
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// Performs a new payment request with the given invoice.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="invoice">The invoice that must be paid.</param>
        public static IPaymentRequestResult Request(this IOnlinePayment onlinePayment, Invoice invoice) =>
            onlinePayment.RequestAsync(invoice)
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// Performs a new payment request with the given data.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="gateway">The gateway which the client must pay the invoice in.</param>
        /// <param name="trackingNumber">
        /// A tracking number for this request. It will be sent to the gateway.
        /// <para>Note: It must be unique for each requests.</para>
        /// </param>
        /// <param name="amount">The amount of the payment request.</param>
        /// <param name="callbackUrl">
        /// A complete URL of your website. It will be used by the gateway for redirecting the client again to your website.
        /// <para>A complete URL would be like: "http://www.mywebsite.com/foo/bar/"</para>
        /// </param>
        /// <param name="cancellationToken"></param>
        public static Task<IPaymentRequestResult> RequestAsync(
            this IOnlinePayment onlinePayment,
            Gateway gateway,
            long trackingNumber,
            decimal amount,
            string callbackUrl,
            CancellationToken cancellationToken = default)
        {
            return onlinePayment.RequestAsync(builder =>
            {
                builder
                    .SetTrackingNumber(trackingNumber)
                    .SetAmount(amount)
                    .SetCallbackUrl(callbackUrl)
                    .UseGateway(gateway);
            }, cancellationToken);
        }

        /// <summary>
        /// Performs a new payment request by using an invoice builder.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="configureInvoice">A builder which helps to build an invoice.</param>
        /// <param name="cancellationToken"></param>
        public static async Task<IPaymentRequestResult> RequestAsync(
            this IOnlinePayment onlinePayment,
            Action<IInvoiceBuilder> configureInvoice,
            CancellationToken cancellationToken = default)
        {
            if (onlinePayment == null) throw new ArgumentNullException(nameof(onlinePayment));
            if (configureInvoice == null) throw new ArgumentNullException(nameof(configureInvoice));

            IInvoiceBuilder invoiceBuilder = new DefaultInvoiceBuilder(onlinePayment.Services);

            configureInvoice(invoiceBuilder);

            var invoice = await invoiceBuilder.BuildAsync(cancellationToken).ConfigureAwaitFalse();

            return await onlinePayment.RequestAsync(invoice, cancellationToken);
        }

        /// <summary>
        /// Fetches the invoice from the incoming request.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <exception cref="PaymentTokenProviderException"></exception>
        /// <exception cref="InvoiceNotFoundException"></exception>
        public static IPaymentFetchResult Fetch(this IOnlinePayment onlinePayment)
            => onlinePayment.FetchAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Verifies the given invoice.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="trackingNumber">The tracking number of the invoice which must be verified.</param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        public static IPaymentVerifyResult Verify(this IOnlinePayment onlinePayment, long trackingNumber)
            => onlinePayment.VerifyAsync(trackingNumber).GetAwaiter().GetResult();

        /// <summary>
        /// Verifies the given invoice.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="invoice"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        public static Task<IPaymentVerifyResult> VerifyAsync(
            this IOnlinePayment onlinePayment,
            IPaymentFetchResult invoice,
            CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            return onlinePayment.VerifyAsync(invoice.TrackingNumber, cancellationToken);
        }

        /// <summary>
        /// Verifies the given invoice.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="invoice"></param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        public static IPaymentVerifyResult Verify(this IOnlinePayment onlinePayment, IPaymentFetchResult invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            return onlinePayment.VerifyAsync(invoice.TrackingNumber).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Cancels the given invoice. No Verifying request will be sent to the gateway.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="trackingNumber">The tracking number of the invoice which must be verified.</param>
        /// <param name="cancellationReason">The reason for cancelling the operation. It will be saved in Message field in database.</param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        public static IPaymentCancelResult Cancel(
            this IOnlinePayment onlinePayment,
            long trackingNumber,
            string cancellationReason = null)
            => onlinePayment.CancelAsync(trackingNumber, cancellationReason).GetAwaiter().GetResult();

        /// <summary>
        /// Cancels the given invoice. No Verifying request will be sent to the gateway.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="invoice"></param>
        /// <param name="cancellationReason">The reason for cancelling the operation. It will be saved in Message field in database.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        public static Task<IPaymentCancelResult> CancelAsync(
            this IOnlinePayment onlinePayment,
            IPaymentFetchResult invoice,
            string cancellationReason = null,
            CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            return onlinePayment.CancelAsync(invoice.TrackingNumber, cancellationReason, cancellationToken);
        }

        /// <summary>
        /// Cancels the given invoice. No Verifying request will be sent to the gateway.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="invoice"></param>
        /// <param name="cancellationReason">The reason for cancelling the operation. It will be saved in Message field in database.</param>
        /// <exception cref="InvoiceNotFoundException"></exception>
        public static IPaymentCancelResult Cancel(
            this IOnlinePayment onlinePayment,
            IPaymentFetchResult invoice,
            string cancellationReason = null)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            return onlinePayment.CancelAsync(invoice.TrackingNumber, cancellationReason).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Verifies the requested payment to check whether or not the invoice has was paid in the gateway by the client.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="paymentVerifyingContext">Describes the information of requested payment.</param>
        [Obsolete("This method is obsolete and will be removed in a future version. The recommended alternative is Verify(long trackingNumber).", error: false)]
        public static IPaymentVerifyResult Verify(
            this IOnlinePayment onlinePayment,
            Action<IPaymentVerifyingContext> paymentVerifyingContext)
            => onlinePayment.VerifyAsync(paymentVerifyingContext)
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// Performs a refund request for the given invoice.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="invoice">The invoice that must be refunded.</param>
        public static IPaymentRefundResult Refund(this IOnlinePayment onlinePayment, RefundInvoice invoice)
            => onlinePayment.RefundAsync(invoice)
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// Refunds completely a specific payment with the given tracking number.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="trackingNumber">The tracking number of the payment that must be refunded.</param>
        public static IPaymentRefundResult RefundCompletely(this IOnlinePayment onlinePayment, long trackingNumber) =>
            onlinePayment.RefundAsync(new RefundInvoice(trackingNumber))
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// Refunds completely the paid payment.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="verifyResult"></param>
        public static IPaymentRefundResult RefundCompletely(this IOnlinePayment onlinePayment, IPaymentVerifyResult verifyResult)
        {
            if (onlinePayment == null) throw new ArgumentNullException(nameof(onlinePayment));
            if (verifyResult == null) throw new ArgumentNullException(nameof(verifyResult));

            return onlinePayment.Refund(new RefundInvoice(verifyResult.TrackingNumber));
        }

        /// <summary>
        /// Refunds a specific amount of a  with the given tracking number.
        /// <para>Note: Only Saman Gateway supports this operation.</para>
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="trackingNumber">The tracking number of the payment which must be refunded.</param>
        /// <param name="amount">Amount of refund.</param>
        public static IPaymentRefundResult RefundSpecificAmount(
            this IOnlinePayment onlinePayment,
            long trackingNumber,
            decimal amount) =>
            onlinePayment.RefundSpecificAmountAsync(trackingNumber, amount)
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// Refunds completely a specific payment with the given tracking number.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="trackingNumber">The tracking number of the payment that must be refunded.</param>
        /// <param name="cancellationToken"></param>
        public static Task<IPaymentRefundResult> RefundCompletelyAsync(
            this IOnlinePayment onlinePayment,
            long trackingNumber,
            CancellationToken cancellationToken = default) =>
            onlinePayment.RefundAsync(new RefundInvoice(trackingNumber), cancellationToken);

        /// <summary>
        /// Refunds a specific amount of a payment with the given tracking number.
        /// <para>Note: Only Saman Gateway supports this operation.</para>
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="trackingNumber">The tracking number of the payment that must be refunded.</param>
        /// <param name="amount">Amount of refund.</param>
        /// <param name="cancellationToken"></param>
        public static Task<IPaymentRefundResult> RefundSpecificAmountAsync(
            this IOnlinePayment onlinePayment,
            long trackingNumber,
            decimal amount,
            CancellationToken cancellationToken = default) =>
            onlinePayment.RefundAsync(new RefundInvoice(trackingNumber, amount), cancellationToken);

        /// <summary>
        /// Refunds completely the paid payment.
        /// </summary>
        /// <param name="onlinePayment"></param>
        /// <param name="verifyResult"></param>
        /// <param name="cancellationToken"></param>
        public static Task<IPaymentRefundResult> RefundCompletelyAsync(
            this IOnlinePayment onlinePayment,
            IPaymentVerifyResult verifyResult,
            CancellationToken cancellationToken = default)
        {
            if (onlinePayment == null) throw new ArgumentNullException(nameof(onlinePayment));
            if (verifyResult == null) throw new ArgumentNullException(nameof(verifyResult));

            return onlinePayment.RefundAsync(new RefundInvoice(verifyResult.TrackingNumber), cancellationToken);
        }
    }
}
