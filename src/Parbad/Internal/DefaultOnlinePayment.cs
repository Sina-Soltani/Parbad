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
using Parbad.Storage.Abstractions;

namespace Parbad.Internal
{
    /// <inheritdoc />
    public class DefaultOnlinePayment : IOnlinePayment
    {
        private readonly IStorageManager _storageManager;
        private readonly IPaymentTokenProvider _tokenProvider;
        private readonly IGatewayProvider _gatewayProvider;
        private readonly ParbadOptions _options;
        private readonly ILogger<DefaultOnlinePayment> _logger;

        /// <summary>
        /// Initializes an instance of <see cref="DefaultOnlinePayment"/>.
        /// </summary>
        public DefaultOnlinePayment(
            IServiceProvider services,
            IStorageManager storageManager,
            IPaymentTokenProvider tokenProvider,
            IGatewayProvider gatewayProvider,
            IOptions<ParbadOptions> options,
            ILogger<DefaultOnlinePayment> logger)
        {
            Services = services;
            _storageManager = storageManager;
            _tokenProvider = tokenProvider;
            _options = options.Value;
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

            Log(logger => logger.LogInformation(LoggingEvents.RequestPayment, $"Requesting the invoice {invoice.TrackingNumber} is started."));

            //  Check the tracking number
            if (await _storageManager.DoesPaymentExistAsync(invoice.TrackingNumber, cancellationToken).ConfigureAwaitFalse())
            {
                Log(logger => logger.LogInformation(LoggingEvents.RequestPayment, _options.Messages.DuplicateTrackingNumber));

                return new PaymentRequestResult
                {
                    TrackingNumber = invoice.TrackingNumber,
                    Status = PaymentRequestResultStatus.TrackingNumberAlreadyExists,
                    Message = _options.Messages.DuplicateTrackingNumber,
                    GatewayTransporter = new NullGatewayTransporter()
                };
            }

            // Create a payment token
            var paymentToken = await _tokenProvider
                .ProvideTokenAsync(invoice, cancellationToken)
                .ConfigureAwaitFalse();

            if (paymentToken.IsNullOrEmpty())
            {
                throw new PaymentTokenProviderException($"The Payment Token Provider '{_tokenProvider.GetType().Name}' didn't provide any token.");
            }

            //  Check the created payment token
            if (await _storageManager.DoesPaymentExistAsync(paymentToken, cancellationToken).ConfigureAwaitFalse())
            {
                var message = $"Requesting the invoice {invoice.TrackingNumber} is finished. The payment token \"{paymentToken}\" already exists.";

                Log(logger => logger.LogError(LoggingEvents.RequestPayment, message));

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
                Log(logger => logger.LogError(exception, exception.Message));

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

            Log(logger => logger.LogInformation(LoggingEvents.RequestPayment, $"Requesting the invoice {invoice.TrackingNumber} is finished."));

            return requestResult;
        }

        /// <inheritdoc />
        public virtual async Task<IPaymentFetchResult> FetchAsync(CancellationToken cancellationToken = default)
        {
            Log(logger => logger.LogInformation(LoggingEvents.FetchPayment, "Fetching is started."));

            var paymentToken = await _tokenProvider.RetrieveTokenAsync(cancellationToken).ConfigureAwaitFalse();

            if (string.IsNullOrEmpty(paymentToken))
            {
                Log(logger => logger.LogError(LoggingEvents.FetchPayment, "Fetching failed. No payment token is received."));

                throw new PaymentTokenProviderException("No Token is received.");
            }

            var payment = await _storageManager.GetPaymentByTokenAsync(paymentToken, cancellationToken).ConfigureAwaitFalse();

            if (payment == null)
            {
                Log(logger => logger.LogError(LoggingEvents.FetchPayment, $"Fetching failed. The operation is not valid. No payment found with the token: {paymentToken}"));

                throw new InvoiceNotFoundException(paymentToken);
            }

            Log(logger => logger.LogInformation(LoggingEvents.FetchPayment, "Fetching is finished."));

            string message = null;

            if (payment.IsCompleted)
            {
                message = _options.Messages.PaymentIsAlreadyProcessedBefore;
            }

            return new PaymentFetchResult
            {
                TrackingNumber = payment.TrackingNumber,
                Amount = payment.Amount,
                GatewayName = payment.GatewayName,
                GatewayAccountName = payment.GatewayAccountName,
                Status = payment.IsCompleted ? PaymentFetchResultStatus.AlreadyProcessed : PaymentFetchResultStatus.ReadyForVerifying,
                IsAlreadyVerified = payment.IsPaid,
                Message = message
            };
        }

        /// <inheritdoc />
        public virtual async Task<IPaymentVerifyResult> VerifyAsync(long trackingNumber, CancellationToken cancellationToken = default)
        {
            Log(logger => logger.LogInformation(LoggingEvents.VerifyPayment, $"Verifying the invoice {trackingNumber} is started."));

            var payment = await _storageManager
                .GetPaymentByTrackingNumberAsync(trackingNumber, cancellationToken)
                .ConfigureAwaitFalse();

            if (payment == null)
            {
                Log(logger => logger.LogError(LoggingEvents.VerifyPayment, $"Verifying the invoice {trackingNumber} is failed. The operation is not valid. No payment found with the tracking number {trackingNumber}"));

                throw new InvoiceNotFoundException(trackingNumber);
            }

            if (payment.IsCompleted)
            {
                Log(logger => logger.LogInformation(LoggingEvents.VerifyPayment, $"Verifying the invoice {trackingNumber} is finished. The requested payment is already processed before."));

                return new PaymentVerifyResult
                {
                    TrackingNumber = payment.TrackingNumber,
                    Amount = payment.Amount,
                    GatewayName = payment.GatewayName,
                    GatewayAccountName = payment.GatewayAccountName,
                    TransactionCode = payment.TransactionCode,
                    Status = payment.IsPaid ? PaymentVerifyResultStatus.AlreadyVerified : PaymentVerifyResultStatus.Failed,
                    Message = _options.Messages.PaymentIsAlreadyProcessedBefore
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
                Log(logger => logger.LogError(LoggingEvents.VerifyPayment, exception, $"Verifying the invoice {trackingNumber} is finished. An error occurred during requesting."));
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

            Log(logger => logger.LogInformation(LoggingEvents.VerifyPayment, $"Verifying the invoice {trackingNumber} is finished."));

            return verifyResult;
        }

        /// <inheritdoc />
        public virtual async Task<IPaymentCancelResult> CancelAsync(long trackingNumber, string cancellationReason = null, CancellationToken cancellationToken = default)
        {
            Log(logger => logger.LogInformation(LoggingEvents.CancelPayment, $"Canceling the invoice {trackingNumber} is started."));

            var payment = await _storageManager
                .GetPaymentByTrackingNumberAsync(trackingNumber, cancellationToken)
                .ConfigureAwaitFalse();

            if (payment == null)
            {
                Log(logger => logger.LogError(LoggingEvents.CancelPayment, $"Canceling the invoice {trackingNumber} is failed. The operation is not valid. No payment found with the tracking number {trackingNumber}"));

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
                    Message = _options.Messages.PaymentIsAlreadyProcessedBefore
                };
            }

            var message = cancellationReason ?? _options.Messages.PaymentCanceledProgrammatically;

            Log(logger => logger.LogInformation(LoggingEvents.CancelPayment, message));

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

            Log(logger => logger.LogInformation(LoggingEvents.CancelPayment, $"Canceling the invoice {trackingNumber} is finished."));

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

            Log(logger => logger.LogInformation(LoggingEvents.RefundPayment, $"Refunding the invoice {invoice.TrackingNumber} is started."));

            var payment = await _storageManager.GetPaymentByTrackingNumberAsync(invoice.TrackingNumber, cancellationToken).ConfigureAwaitFalse();

            if (payment == null)
            {
                Log(logger => logger.LogError(LoggingEvents.RefundPayment, $"Refunding the invoice {invoice.TrackingNumber} is failed. The operation is not valid. No payment found with the tracking number {invoice.TrackingNumber}"));

                throw new InvoiceNotFoundException(invoice.TrackingNumber);
            }

            if (!payment.IsCompleted)
            {
                var message = $"{_options.Messages.OnlyCompletedPaymentCanBeRefunded} Tracking number: {invoice.TrackingNumber}.";

                Log(logger => logger.LogInformation(LoggingEvents.RefundPayment, $"Refunding the invoice {invoice.TrackingNumber} is finished. {message}"));

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
                Log(logger => logger.LogError(exception, "Parbad exception. An error occurred during requesting."));
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

            Log(logger => logger.LogInformation(LoggingEvents.RefundPayment, $"Refunding the invoice {invoice.TrackingNumber} is finished."));

            return refundResult;
        }

        private void Log(Action<ILogger> logger)
        {
            if (!_options.EnableLogging) return;

            logger(_logger);
        }
    }
}
