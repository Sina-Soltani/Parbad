// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Data.Context;
using Parbad.Data.Domain.Payments;
using Parbad.Data.Domain.Transactions;
using Parbad.Options;
using Parbad.Exceptions;
using Parbad.PaymentTokenProviders;
using Parbad.Properties;

namespace Parbad.Internal
{
    /// <inheritdoc />
    public class DefaultOnlinePayment : IOnlinePayment
    {
        private readonly ParbadDataContext _database;
        private readonly IPaymentTokenProvider _tokenProvider;
        private readonly IGatewayProvider _gatewayProvider;
        private readonly IOptions<MessagesOptions> _messagesOptions;
        private readonly ILogger<IOnlinePayment> _logger;

        /// <summary>
        /// Initializes an instance of <see cref="DefaultOnlinePayment"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="database"></param>
        /// <param name="tokenProvider"></param>
        /// <param name="gatewayProvider"></param>
        /// <param name="messagesOptions"></param>
        /// <param name="logger"></param>
        public DefaultOnlinePayment(
            IServiceProvider services,
            ParbadDataContext database,
            IPaymentTokenProvider tokenProvider,
            IGatewayProvider gatewayProvider,
            IOptions<MessagesOptions> messagesOptions,
            ILogger<IOnlinePayment> logger)
        {
            Services = services;
            _database = database;
            _tokenProvider = tokenProvider;
            _messagesOptions = messagesOptions;
            _logger = logger;
            _gatewayProvider = gatewayProvider;
        }

        /// <inheritdoc />
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public virtual async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            _logger.LogInformation(LoggingEvents.RequestPayment, $"New payment request with the tracking number {invoice.TrackingNumber} is started." +
                                                                    $"{nameof(invoice.Amount)}:{invoice.Amount}" +
                                                                    $"GatewayName:{GatewayHelper.GetNameByType(invoice.GatewayType)}");

            //  Check the tracking number
            if (await _database.Payments
                .AnyAsync(model => model.TrackingNumber == invoice.TrackingNumber, cancellationToken)
                .ConfigureAwaitFalse())
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
            if (await _database.Payments
                .AnyAsync(model => model.Token == paymentToken, cancellationToken)
                .ConfigureAwaitFalse())
            {
                var message = $"The payment token \"{paymentToken}\" already exists.";

                _logger.LogError(LoggingEvents.RequestPayment, message);

                throw new PaymentTokenProviderException(message);
            }

            var gateway = _gatewayProvider.Provide(invoice.GatewayType);

            //  Add database
            var newPayment = new Payment
            {
                TrackingNumber = invoice.TrackingNumber,
                Amount = invoice.Amount,
                IsCompleted = false,
                IsPaid = false,
                Token = paymentToken,
                GatewayName = gateway.GetName(),
                CreatedOn = DateTime.UtcNow
            };

            _database.Payments.Add(newPayment);

            if (await _database.SaveChangesAsync(cancellationToken).ConfigureAwaitFalse() == 0)
            {
                _logger.LogError(LoggingEvents.RequestPayment, "Nothing is saved into the database.");
                throw new DatabaseSaveRecordException();
            }

            _logger.LogInformation(LoggingEvents.RequestPayment, $"The payment with tracking number {invoice.TrackingNumber} is about to processing." +
                                                                    $"{nameof(invoice.Amount)}:{invoice.Amount}" +
                                                                    $"GatewayName:{GatewayHelper.GetNameByType(invoice.GatewayType)}");

            PaymentRequestResult requestResult;

            try
            {
                requestResult = await gateway
                    .RequestAsync(invoice, cancellationToken)
                    .ConfigureAwaitFalse() as PaymentRequestResult;

                if (requestResult == null) throw new Exception($"Gateway {gateway.GetName()} returns null instead of a result.");
            }
            catch (Exception exception)
            {
                string exceptionMessage;

                if (exception is OptionsValidationException)
                {
                    exceptionMessage = $"Gateway {gateway.GetName()} is not configured or has some validation errors.";
                }
                else
                {
                    exceptionMessage = exception.Message;
                }

                _logger.LogError(exception, exceptionMessage);

                newPayment.IsCompleted = true;
                newPayment.IsPaid = false;
                newPayment.UpdatedOn = DateTime.UtcNow;

                requestResult = PaymentRequestResult.Failed(exceptionMessage);
            }

            requestResult.TrackingNumber = invoice.TrackingNumber;
            requestResult.Amount = invoice.Amount;
            requestResult.GatewayName = gateway.GetName();

            newPayment.GatewayAccountName = requestResult.GatewayAccountName;
            newPayment.Transactions.Add(new Transaction
            {
                Amount = invoice.Amount,
                Type = TransactionType.Request,
                IsSucceed = requestResult.IsSucceed,
                Message = requestResult.Message,
                AdditionalData = AdditionalDataConverter.ToJson(requestResult),
                CreatedOn = DateTime.UtcNow
            });

            if (await _database.SaveChangesAsync(cancellationToken).ConfigureAwaitFalse() == 0)
            {
                _logger.LogError(LoggingEvents.RequestPayment, "Nothing is saved into database.");
                throw new DatabaseSaveRecordException();
            }

            return requestResult;
        }

        /// <inheritdoc />
        public virtual async Task<IPaymentVerifyResult> VerifyAsync(Action<IPaymentVerifyingContext> context, CancellationToken cancellationToken = default)
        {
            _logger.LogError(LoggingEvents.VerifyPayment, "Verify Starts.");

            var paymentToken = await _tokenProvider.RetrieveTokenAsync(cancellationToken).ConfigureAwaitFalse();

            if (string.IsNullOrEmpty(paymentToken))
            {
                _logger.LogError(LoggingEvents.VerifyPayment, "Verify Ends. No Payment Token is received.");

                throw new PaymentTokenProviderException("No Token is received.");
            }

            var payment = await _database.Payments
                .Include(model => model.Transactions)
                .SingleOrDefaultAsync(model => model.Token == paymentToken, cancellationToken)
                .ConfigureAwaitFalse();

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
                payment.UpdatedOn = DateTime.UtcNow;
                payment.Transactions.Add(new Transaction
                {
                    Amount = payment.Amount,
                    IsSucceed = false,
                    Message = message,
                    Type = TransactionType.Verify,
                    CreatedOn = DateTime.UtcNow
                });

                if (await _database.SaveChangesAsync(cancellationToken).ConfigureAwaitFalse() == 0)
                {
                    _logger.LogError(LoggingEvents.VerifyPayment, "Nothing is saved into database.");

                    throw new DatabaseSaveRecordException();
                }

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

            _logger.LogInformation(LoggingEvents.VerifyPayment, $"The payment with the tracking Number {payment.TrackingNumber} is about to verifying.");

            PaymentVerifyResult verifyResult;

            try
            {
                verifyResult = await gateway
                    .VerifyAsync(payment, cancellationToken)
                    .ConfigureAwaitFalse() as PaymentVerifyResult;
            }
            catch (OptionsValidationException)
            {
                throw new GatewayOptionsConfigurationException(gateway.GetName());
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Parbad exception. An error occurred during requesting.");
                throw;
            }

            if (verifyResult == null) throw new Exception($"Gateway {gateway.GetName()} returns null instead of a result.");

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
            payment.UpdatedOn = DateTime.UtcNow;
            payment.Transactions.Add(new Transaction
            {
                Amount = verifyResult.Amount,
                IsSucceed = verifyResult.IsSucceed,
                Message = verifyResult.Message,
                Type = TransactionType.Verify,
                AdditionalData = AdditionalDataConverter.ToJson(verifyResult),
                CreatedOn = DateTime.UtcNow
            });

            if (await _database.SaveChangesAsync(cancellationToken).ConfigureAwaitFalse() == 0)
            {
                _logger.LogError(LoggingEvents.VerifyPayment, "Nothing is saved into database.");

                throw new DatabaseSaveRecordException();
            }

            _logger.LogInformation(LoggingEvents.VerifyPayment, "Verify ends.");

            return verifyResult;
        }

        /// <inheritdoc />
        public virtual async Task<IPaymentRefundResult> RefundAsync(RefundInvoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            _logger.LogInformation(LoggingEvents.RefundPayment, $"Refund starts for the payment with tracking number {invoice.TrackingNumber}.");

            var payment = await _database.Payments
                .Include(model => model.Transactions)
                .SingleOrDefaultAsync(model => model.TrackingNumber == invoice.TrackingNumber, cancellationToken)
                .ConfigureAwaitFalse();

            if (payment == null)
            {
                var message = $"The operation is not valid. No payment found with the tracking number {invoice.TrackingNumber}.";

                _logger.LogError(LoggingEvents.RefundPayment, message);

                return PaymentRefundResult.Failed(message);
            }

            if (!payment.IsCompleted)
            {
                var message = $"The payment with the tracking number {invoice.TrackingNumber} is not completed yet. Only a completed payment can be refund.";

                _logger.LogError(LoggingEvents.RefundPayment, message);

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
                amountToRefund = payment.Amount;
            }

            var gateway = _gatewayProvider.Provide(payment.GatewayName);

            PaymentRefundResult refundResult;

            try
            {
                refundResult = await gateway
                    .RefundAsync(payment, amountToRefund, cancellationToken)
                    .ConfigureAwaitFalse() as PaymentRefundResult;
            }
            catch (OptionsValidationException)
            {
                throw new GatewayOptionsConfigurationException(gateway.GetName());
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Parbad exception. An error occurred during requesting.");
                throw;
            }

            if (refundResult == null) throw new Exception($"Gateway {gateway.GetName()} returns null instead of a result.");

            refundResult.TrackingNumber = payment.TrackingNumber;
            refundResult.Amount = amountToRefund;
            refundResult.GatewayName = payment.GatewayName;
            refundResult.GatewayAccountName = payment.GatewayAccountName;

            payment.Transactions.Add(new Transaction
            {
                Amount = refundResult.Amount,
                Type = TransactionType.Refund,
                IsSucceed = refundResult.IsSucceed,
                Message = refundResult.Message,
                AdditionalData = AdditionalDataConverter.ToJson(refundResult),
                CreatedOn = DateTime.UtcNow
            });

            if (await _database.SaveChangesAsync(cancellationToken).ConfigureAwaitFalse() == 0)
            {
                _logger.LogError(LoggingEvents.RefundPayment, "Refund ends. Nothing is saved into database.");

                throw new DatabaseSaveRecordException();
            }

            _logger.LogInformation(LoggingEvents.RefundPayment, "Refund ends.");

            return refundResult;
        }
    }
}
