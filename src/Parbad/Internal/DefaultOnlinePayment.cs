// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Exceptions;
using Parbad.Options;
using Parbad.PaymentTokenProviders;
using Parbad.Properties;
using Parbad.Storage.Abstractions;

namespace Parbad.Internal
{
    /// <inheritdoc />
    public class DefaultOnlinePayment : IOnlinePayment
    {
        private readonly IStorageManager _storageManager;
        private readonly IPaymentTokenProvider _tokenProvider;
        private readonly IGatewayProvider _gatewayProvider;
        private readonly IOptions<MessagesOptions> _messagesOptions;
        private readonly ILogger<IOnlinePayment> _logger;

        /// <summary>
        /// Initializes an instance of <see cref="DefaultOnlinePayment"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="storageManager"></param>
        /// <param name="tokenProvider"></param>
        /// <param name="gatewayProvider"></param>
        /// <param name="messagesOptions"></param>
        /// <param name="logger"></param>
        public DefaultOnlinePayment(
            IServiceProvider services,
            IStorageManager storageManager,
            IPaymentTokenProvider tokenProvider,
            IGatewayProvider gatewayProvider,
            IOptions<MessagesOptions> messagesOptions,
            ILogger<IOnlinePayment> logger)
        {
            Services = services;
            _storageManager = storageManager;
            _tokenProvider = tokenProvider;
            _messagesOptions = messagesOptions;
            _logger = logger;
            _storageManager = storageManager;
            _gatewayProvider = gatewayProvider;
        }

        /// <inheritdoc />
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public virtual async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            _logger.LogInformation(LoggingEvents.RequestPayment, $"Requesting the invoice {invoice.TrackingNumber} is started.");

            //  Check the tracking number
            if (await _storageManager.DoesPaymentExistAsync(invoice.TrackingNumber, cancellationToken).ConfigureAwaitFalse())
            {
                _logger.LogInformation(LoggingEvents.RequestPayment, _messagesOptions.Value.DuplicateTrackingNumber);

                return new PaymentRequestResult
                {
                    TrackingNumber = invoice.TrackingNumber,
                    IsSucceed = false,
                    Message = _messagesOptions.Value.DuplicateTrackingNumber,
                    GatewayTransporter = new NullGatewayTransporter()
                };
            }

            // Create a payment token
            var paymentToken = await _tokenProvider
                .ProvideTokenAsync(invoice, cancellationToken)
                .ConfigureAwaitFalse();

            //  Check the created payment token
            if (await _storageManager.DoesPaymentExistAsync(paymentToken, cancellationToken).ConfigureAwaitFalse())
            {
                var message = $"Requesting the invoice {invoice.TrackingNumber} is finished. The payment token \"{paymentToken}\" already exists.";

                _logger.LogError(LoggingEvents.RequestPayment, message);

                throw new PaymentTokenProviderException(message);
            }

            var gateway = _gatewayProvider.Provide(invoice.GatewayName);

            var newPayment = new Payment
            {
                TrackingNumber = invoice.TrackingNumber,
                Amount = invoice.Amount,
                IsCompleted = false,
                IsPaid = false,
                Token = paymentToken,
                GatewayName = gateway.GetRoutingGatewayName()
            };

            await _storageManager.CreatePaymentAsync(newPayment, cancellationToken).ConfigureAwaitFalse();

            PaymentRequestResult requestResult;

            try
            {
                requestResult = await gateway
                    .RequestAsync(invoice, cancellationToken)
                    .ConfigureAwaitFalse() as PaymentRequestResult;

                if (requestResult == null) throw new Exception($"Requesting the invoice {invoice.TrackingNumber} is finished. The gateway {gateway.GetCompleteGatewayName()} returns null instead of a result.");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);

                newPayment.IsCompleted = true;
                newPayment.IsPaid = false;

                requestResult = PaymentRequestResult.Failed(exception.Message);
            }

            requestResult.TrackingNumber = invoice.TrackingNumber;
            requestResult.Amount = invoice.Amount;
            requestResult.GatewayName = gateway.GetRoutingGatewayName();

            newPayment.GatewayAccountName = requestResult.GatewayAccountName;

            await _storageManager.UpdatePaymentAsync(newPayment, cancellationToken).ConfigureAwaitFalse();

            var newTransaction = new Transaction
            {
                Amount = invoice.Amount,
                Type = TransactionType.Request,
                IsSucceed = requestResult.IsSucceed,
                Message = requestResult.Message,
                AdditionalData = AdditionalDataConverter.ToJson(requestResult),
                PaymentId = newPayment.Id
            };

            await _storageManager.CreateTransactionAsync(newTransaction, cancellationToken).ConfigureAwaitFalse();

            _logger.LogInformation(LoggingEvents.RequestPayment, $"Requesting the invoice {invoice.TrackingNumber} is finished.");

            return requestResult;
        }

        /// <inheritdoc />
        public virtual async Task<IPaymentFetchResult> FetchAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(LoggingEvents.FetchPayment, "Fetching is started.");

            var paymentToken = await _tokenProvider.RetrieveTokenAsync(cancellationToken).ConfigureAwaitFalse();

            if (string.IsNullOrEmpty(paymentToken))
            {
                _logger.LogError(LoggingEvents.FetchPayment, "Fetching failed. No payment token is received.");

                throw new PaymentTokenProviderException("No Token is received.");
            }

            var payment = await _storageManager.GetPaymentByTokenAsync(paymentToken, cancellationToken).ConfigureAwaitFalse();

            if (payment == null)
            {
                _logger.LogError(LoggingEvents.FetchPayment, $"Fetching failed. The operation is not valid. No payment found with the token: {paymentToken}");

                throw new InvoiceNotFoundException(paymentToken);
            }

            _logger.LogInformation(LoggingEvents.FetchPayment, "Fetching is finished.");

            var isInvoiceValid = true;
            string message = null;

            if (payment.IsCompleted)
            {
                isInvoiceValid = false;

                message = "The requested payment is already processed before.";
            }

            return new PaymentFetchResult
            {
                TrackingNumber = payment.TrackingNumber,
                Amount = payment.Amount,
                GatewayName = payment.GatewayName,
                GatewayAccountName = payment.GatewayAccountName,
                IsSucceed = isInvoiceValid,
                Message = message
            };
        }

        /// <inheritdoc />
        public virtual async Task<IPaymentVerifyResult> VerifyAsync(long trackingNumber, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(LoggingEvents.VerifyPayment, $"Verifying the invoice {trackingNumber} is started.");

            var payment = await _storageManager
                .GetPaymentByTrackingNumberAsync(trackingNumber, cancellationToken)
                .ConfigureAwaitFalse();

            if (payment == null)
            {
                _logger.LogError(LoggingEvents.VerifyPayment, $"Verifying the invoice {trackingNumber} is failed. The operation is not valid. No payment found with the tracking number {trackingNumber}");

                throw new InvoiceNotFoundException(trackingNumber);
            }

            if (payment.IsCompleted)
            {
                _logger.LogInformation(LoggingEvents.VerifyPayment, $"Verifying the invoice {trackingNumber} is finished. The requested payment is already processed before.");

                return new PaymentVerifyResult
                {
                    TrackingNumber = payment.TrackingNumber,
                    Amount = payment.Amount,
                    GatewayName = payment.GatewayName,
                    GatewayAccountName = payment.GatewayAccountName,
                    TransactionCode = payment.TransactionCode,
                    IsSucceed = false,
                    Message = "The requested payment is already processed before."
                };
            }

            var gateway = _gatewayProvider.Provide(payment.GatewayName);

            var transactions = await _storageManager.GetTransactionsAsync(payment, cancellationToken).ConfigureAwaitFalse();
            var verifyContext = new InvoiceContext(payment, transactions);

            PaymentVerifyResult verifyResult;

            try
            {
                verifyResult = await gateway
                    .VerifyAsync(verifyContext, cancellationToken)
                    .ConfigureAwaitFalse() as PaymentVerifyResult;
            }
            catch (Exception exception)
            {
                _logger.LogError(LoggingEvents.VerifyPayment, exception, $"Verifying the invoice {trackingNumber} is finished. An error occurred during requesting.");
                throw;
            }

            if (verifyResult == null) throw new Exception($"The {gateway.GetCompleteGatewayName()} returns null instead of a result.");

            verifyResult.TrackingNumber = payment.TrackingNumber;
            verifyResult.Amount = payment.Amount;
            verifyResult.GatewayName = payment.GatewayName;
            verifyResult.GatewayAccountName = payment.GatewayAccountName;

            payment.IsCompleted = true;
            payment.IsPaid = verifyResult.IsSucceed;
            payment.TransactionCode = verifyResult.TransactionCode;

            await _storageManager.UpdatePaymentAsync(payment, cancellationToken).ConfigureAwaitFalse();

            var transaction = new Transaction
            {
                Amount = verifyResult.Amount,
                IsSucceed = verifyResult.IsSucceed,
                Message = verifyResult.Message,
                Type = TransactionType.Verify,
                AdditionalData = AdditionalDataConverter.ToJson(verifyResult),
                PaymentId = payment.Id
            };

            await _storageManager.CreateTransactionAsync(transaction, cancellationToken).ConfigureAwaitFalse();

            _logger.LogInformation(LoggingEvents.VerifyPayment, $"Verifying the invoice {trackingNumber} is finished.");

            return verifyResult;
        }

        /// <inheritdoc />
        public virtual async Task<IPaymentVerifyResult> VerifyAsync(Action<IPaymentVerifyingContext> context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(LoggingEvents.VerifyPayment, "Verifying the invoice is started.");

            var paymentToken = await _tokenProvider.RetrieveTokenAsync(cancellationToken).ConfigureAwaitFalse();

            if (string.IsNullOrEmpty(paymentToken))
            {
                _logger.LogError(LoggingEvents.VerifyPayment, "Verify Ends. No Payment Token is received.");

                throw new PaymentTokenProviderException("No Token is received.");
            }

            var payment = await _storageManager.GetPaymentByTokenAsync(paymentToken, cancellationToken).ConfigureAwaitFalse();

            if (payment == null)
            {
                _logger.LogError(LoggingEvents.VerifyPayment, $"Verify Ends. The operation is not valid. No payment found with the token: {paymentToken}");

                return new PaymentVerifyResult
                {
                    IsSucceed = false,
                    Message = "The operation is not valid. No payment found with the given token."
                };
            }

            if (payment.IsCompleted)
            {
                _logger.LogError(LoggingEvents.VerifyPayment, $"Verify Ends. Tracking Number {payment.TrackingNumber}. Result: The requested payment is already processed before.");

                return new PaymentVerifyResult
                {
                    TrackingNumber = payment.TrackingNumber,
                    Amount = payment.Amount,
                    GatewayName = payment.GatewayName,
                    GatewayAccountName = payment.GatewayAccountName,
                    TransactionCode = payment.TransactionCode,
                    IsSucceed = false,
                    Message = "The requested payment is already processed before."
                };
            }

            var c = new PaymentVerifyingContext
            {
                TrackingNumber = payment.TrackingNumber,
                GatewayName = payment.GatewayName
            };

            context(c);

            if (c.IsCancelled)
            {
                var message = c.CancellationReason ?? Resources.PaymentCanceledProgrammatically;

                _logger.LogInformation(LoggingEvents.VerifyPayment, message);

                payment.IsCompleted = true;
                payment.IsPaid = false;

                await _storageManager.UpdatePaymentAsync(payment, cancellationToken).ConfigureAwaitFalse();

                var newTransaction = new Transaction
                {
                    Amount = payment.Amount,
                    IsSucceed = false,
                    Message = message,
                    Type = TransactionType.Verify,
                    PaymentId = payment.Id
                };

                await _storageManager.CreateTransactionAsync(newTransaction, cancellationToken).ConfigureAwaitFalse();

                return new PaymentVerifyResult
                {
                    TrackingNumber = payment.TrackingNumber,
                    Amount = payment.Amount,
                    IsSucceed = false,
                    GatewayName = payment.GatewayName,
                    GatewayAccountName = payment.GatewayAccountName,
                    Message = message
                };
            }

            var gateway = _gatewayProvider.Provide(payment.GatewayName);

            var transactions = await _storageManager.GetTransactionsAsync(payment, cancellationToken).ConfigureAwaitFalse();
            var verifyContext = new InvoiceContext(payment, transactions);

            _logger.LogInformation(LoggingEvents.VerifyPayment, $"The payment with the tracking Number {payment.TrackingNumber} is about to verifying.");

            PaymentVerifyResult verifyResult;

            try
            {
                verifyResult = await gateway
                    .VerifyAsync(verifyContext, cancellationToken)
                    .ConfigureAwaitFalse() as PaymentVerifyResult;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Parbad exception. An error occurred during requesting.");
                throw;
            }

            if (verifyResult == null) throw new Exception($"The gateway {gateway.GetCompleteGatewayName()} returns null instead of a result.");

            verifyResult.TrackingNumber = payment.TrackingNumber;
            verifyResult.Amount = payment.Amount;
            verifyResult.GatewayName = payment.GatewayName;
            verifyResult.GatewayAccountName = payment.GatewayAccountName;

            _logger.LogInformation(LoggingEvents.VerifyPayment, "Verifying finished. " +
                                                                $"Tracking Number {payment.TrackingNumber}. " +
                                                                $"IsSucceed: {verifyResult.IsSucceed}" +
                                                                $"Message: {verifyResult.Message}" +
                                                                $"GatewayName: {verifyResult.GatewayName}");

            payment.IsCompleted = true;
            payment.IsPaid = verifyResult.IsSucceed;
            payment.TransactionCode = verifyResult.TransactionCode;

            await _storageManager.UpdatePaymentAsync(payment, cancellationToken).ConfigureAwaitFalse();

            var transaction = new Transaction
            {
                Amount = verifyResult.Amount,
                IsSucceed = verifyResult.IsSucceed,
                Message = verifyResult.Message,
                Type = TransactionType.Verify,
                AdditionalData = AdditionalDataConverter.ToJson(verifyResult),
                PaymentId = payment.Id
            };

            await _storageManager.CreateTransactionAsync(transaction, cancellationToken).ConfigureAwaitFalse();

            _logger.LogInformation(LoggingEvents.VerifyPayment, "Verify ends.");

            return verifyResult;
        }

        /// <inheritdoc />
        public virtual async Task<IPaymentCancelResult> CancelAsync(long trackingNumber, string cancellationReason = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(LoggingEvents.CancelPayment, $"Canceling the invoice {trackingNumber} is started.");

            var payment = await _storageManager
                .GetPaymentByTrackingNumberAsync(trackingNumber, cancellationToken)
                .ConfigureAwaitFalse();

            if (payment == null)
            {
                _logger.LogError(LoggingEvents.CancelPayment, $"Canceling the invoice {trackingNumber} is failed. The operation is not valid. No payment found with the tracking number {trackingNumber}");

                throw new InvoiceNotFoundException(trackingNumber);
            }

            if (payment.IsCompleted)
            {
                return new PaymentCancelResult
                {
                    TrackingNumber = payment.TrackingNumber,
                    Amount = payment.Amount,
                    GatewayName = payment.GatewayName,
                    GatewayAccountName = payment.GatewayAccountName,
                    IsSucceed = false,
                    Message = "The requested payment is already processed before."
                };
            }

            var message = cancellationReason ?? Resources.PaymentCanceledProgrammatically;

            _logger.LogInformation(LoggingEvents.CancelPayment, message);

            payment.IsCompleted = true;
            payment.IsPaid = false;

            await _storageManager.UpdatePaymentAsync(payment, cancellationToken).ConfigureAwaitFalse();

            var newTransaction = new Transaction
            {
                Amount = payment.Amount,
                IsSucceed = false,
                Message = message,
                Type = TransactionType.Verify,
                PaymentId = payment.Id
            };

            await _storageManager.CreateTransactionAsync(newTransaction, cancellationToken).ConfigureAwaitFalse();

            _logger.LogInformation(LoggingEvents.CancelPayment, $"Canceling the invoice {trackingNumber} is finished.");

            return new PaymentCancelResult
            {
                TrackingNumber = payment.TrackingNumber,
                Amount = payment.Amount,
                IsSucceed = true,
                GatewayName = payment.GatewayName,
                GatewayAccountName = payment.GatewayAccountName,
                Message = message
            };
        }

        /// <inheritdoc />
        public virtual async Task<IPaymentRefundResult> RefundAsync(RefundInvoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            _logger.LogInformation(LoggingEvents.RefundPayment, $"Refunding the invoice {invoice.TrackingNumber} is started.");

            var payment = await _storageManager.GetPaymentByTrackingNumberAsync(invoice.TrackingNumber, cancellationToken).ConfigureAwaitFalse();

            if (payment == null)
            {
                _logger.LogError(LoggingEvents.RefundPayment, $"Refunding the invoice {invoice.TrackingNumber} is failed. The operation is not valid. No payment found with the tracking number {invoice.TrackingNumber}");

                throw new InvoiceNotFoundException(invoice.TrackingNumber);
            }

            if (!payment.IsCompleted)
            {
                var message = $"The payment with the tracking number {invoice.TrackingNumber} is not completed yet. Only a completed payment can be refund.";

                _logger.LogInformation(LoggingEvents.RefundPayment, $"Refunding the invoice {invoice.TrackingNumber} is finished. {message}");

                return PaymentRefundResult.Failed(message);
            }

            Money amountToRefund;

            if (invoice.Amount == 0)
            {
                amountToRefund = payment.Amount;
            }
            else if (invoice.Amount > payment.Amount)
            {
                throw new Exception("Amount cannot be greater than the amount of the paid payment.");
            }
            else
            {
                amountToRefund = invoice.Amount;
            }

            var gateway = _gatewayProvider.Provide(payment.GatewayName);

            var transactions = await _storageManager.GetTransactionsAsync(payment, cancellationToken).ConfigureAwaitFalse();
            var verifyContext = new InvoiceContext(payment, transactions);

            PaymentRefundResult refundResult;

            try
            {
                refundResult = await gateway
                    .RefundAsync(verifyContext, amountToRefund, cancellationToken)
                    .ConfigureAwaitFalse() as PaymentRefundResult;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Parbad exception. An error occurred during requesting.");
                throw;
            }

            if (refundResult == null) throw new Exception($"Gateway {gateway.GetCompleteGatewayName()} returns null instead of a result.");

            refundResult.TrackingNumber = payment.TrackingNumber;
            refundResult.Amount = amountToRefund;
            refundResult.GatewayName = payment.GatewayName;
            refundResult.GatewayAccountName = payment.GatewayAccountName;

            var newtTransaction = new Transaction
            {
                Amount = refundResult.Amount,
                Type = TransactionType.Refund,
                IsSucceed = refundResult.IsSucceed,
                Message = refundResult.Message,
                AdditionalData = AdditionalDataConverter.ToJson(refundResult),
                PaymentId = payment.Id
            };

            await _storageManager.CreateTransactionAsync(newtTransaction, cancellationToken).ConfigureAwaitFalse();

            _logger.LogInformation(LoggingEvents.RefundPayment, $"Refunding the invoice {invoice.TrackingNumber} is finished.");

            return refundResult;
        }
    }
}
