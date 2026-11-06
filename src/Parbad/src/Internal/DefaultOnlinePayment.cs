// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.Exceptions;
using Parbad.Options;
using Parbad.PaymentTokenProviders;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Abstractions.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parbad.Internal;

/// <inheritdoc />
public class DefaultOnlinePayment : IOnlinePayment
{
    // Verifications and cancellations of one payment run one at a time within this process, so a double click or
    // a refreshed callback page does not ask the bank twice. Across servers, the guarded write in
    // CompletePaymentAsync keeps the first result.
    private static readonly KeyedAsyncLock<long> PaymentLocks = new KeyedAsyncLock<long>();

    private readonly IStorage _storage;
    private readonly IPaymentTokenProvider _tokenProvider;
    private readonly IGatewayProvider _gatewayProvider;
    private readonly ParbadOptions _options;
    private readonly IParbadLogger<DefaultOnlinePayment> _logger;

    /// <summary>
    /// Initializes an instance of <see cref="DefaultOnlinePayment"/>.
    /// </summary>
    public DefaultOnlinePayment(
        IServiceProvider services,
        IStorage storage,
        IPaymentTokenProvider tokenProvider,
        IGatewayProvider gatewayProvider,
        IOptions<ParbadOptions> options,
        IParbadLogger<DefaultOnlinePayment> logger)
    {
        Services = services;
        _storage = storage;
        _tokenProvider = tokenProvider;
        _options = options.Value;
        _logger = logger;
        _gatewayProvider = gatewayProvider;
    }

    /// <inheritdoc />
    public IServiceProvider Services { get; }

    /// <inheritdoc />
    public virtual async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        if (invoice == null) throw new ArgumentNullException(nameof(invoice));

        _logger.LogInformation(LoggingEvents.RequestPayment, "Requesting the invoice {TrackingNumber} is started", invoice.TrackingNumber);

        if (await _storage.DoesPaymentExistAsync(invoice.TrackingNumber, cancellationToken).ConfigureAwaitFalse())
        {
            _logger.LogInformation(LoggingEvents.RequestPayment, _options.Messages.DuplicateTrackingNumber);

            return new PaymentRequestResult
                   {
                       TrackingNumber = invoice.TrackingNumber,
                       Status = PaymentRequestResultStatus.TrackingNumberAlreadyExists,
                       Message = _options.Messages.DuplicateTrackingNumber,
                       GatewayTransporter = new NullGatewayTransporter()
                   };
        }

        var paymentToken = await _tokenProvider
                                .ProvideTokenAsync(invoice, cancellationToken)
                                .ConfigureAwaitFalse();

        if (paymentToken.IsNullOrEmpty())
        {
            throw new PaymentTokenProviderException($"The Payment Token Provider '{_tokenProvider.GetType().Name}' didn't provide any token.");
        }

        //  Check the created payment token
        if (await _storage.DoesPaymentExistAsync(paymentToken, cancellationToken).ConfigureAwaitFalse())
        {
            var message = $"Requesting the invoice {invoice.TrackingNumber} is finished. The payment token \"{paymentToken}\" already exists.";

            _logger.LogError(LoggingEvents.RequestPayment,
                             "Requesting the invoice {TrackingNumber} is finished. The payment token \"{PaymentToken}\" already exists",
                             invoice.TrackingNumber,
                             paymentToken);

            throw new PaymentTokenProviderException(message);
        }

        var gateway = _gatewayProvider.Provide(invoice.GatewayName);

        var newPayment = new Payment
                         {
                             TrackingNumber = invoice.TrackingNumber,
                             Amount = invoice.Amount,
                             Status = PaymentStatus.Requested,
                             Token = paymentToken,
                             GatewayName = gateway.GetRoutingGatewayName()
                         };

        await _storage.CreatePaymentAsync(newPayment, cancellationToken).ConfigureAwaitFalse();

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

            requestResult = PaymentRequestResult.Failed(exception.Message);
        }

        // A rejected request is final: the user never reaches the bank, so there is nothing to verify.
        if (!requestResult.IsSucceed) newPayment.Status = PaymentStatus.Failed;

        requestResult.TrackingNumber = invoice.TrackingNumber;
        requestResult.Amount = invoice.Amount;
        requestResult.GatewayName = gateway.GetRoutingGatewayName();

        newPayment.GatewayAccountName = requestResult.GatewayAccountName;

        await _storage.UpdatePaymentAsync(newPayment, cancellationToken).ConfigureAwaitFalse();

        var newTransaction = new Transaction
                             {
                                 Amount = invoice.Amount,
                                 Type = TransactionType.Request,
                                 IsSucceed = requestResult.IsSucceed,
                                 Message = requestResult.Message,
                                 AdditionalData = AdditionalDataConverter.ToJson(requestResult),
                                 PaymentId = newPayment.Id
                             };

        await _storage.CreateTransactionAsync(newTransaction, cancellationToken).ConfigureAwaitFalse();

        _logger.LogInformation(LoggingEvents.RequestPayment, "Requesting the invoice {TrackingNumber} is finished", invoice.TrackingNumber);

        return requestResult;
    }

    /// <inheritdoc />
    public virtual async Task<IPaymentFetchResult> FetchAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(LoggingEvents.FetchPayment, "Fetching from the current HTTP request is started");

        var paymentToken = await _tokenProvider.RetrieveTokenAsync(cancellationToken).ConfigureAwaitFalse();

        if (string.IsNullOrEmpty(paymentToken))
        {
            _logger.LogError(LoggingEvents.FetchPayment, "Fetching failed. No payment token is received");

            throw new PaymentTokenProviderException("No Token is received.");
        }

        var payment = await _storage.GetPaymentByTokenAsync(paymentToken, cancellationToken).ConfigureAwaitFalse();

        if (payment == null)
        {
            _logger.LogError(LoggingEvents.FetchPayment, "Fetching failed. The operation is not valid. No payment found with the token: {PaymentToken}", paymentToken);

            throw new InvoiceNotFoundException(paymentToken);
        }

        var result = await FetchAsync(payment, cancellationToken);

        return result;
    }

    /// <inheritdoc />
    public async Task<IPaymentFetchResult> FetchAsync(long trackingNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(LoggingEvents.FetchPayment, "Fetching from database for invoice number {TrackingNumber} is started", trackingNumber);

        var payment = await _storage.GetPaymentByTrackingNumberAsync(trackingNumber, cancellationToken).ConfigureAwaitFalse();

        if (payment == null)
        {
            _logger.LogError(LoggingEvents.FetchPayment,
                             "Fetching failed. The operation is not valid. No payment found for the given tracking number: {TrackingNumber}",
                             trackingNumber);

            throw new InvoiceNotFoundException(trackingNumber);
        }

        var result = await FetchAsync(payment, cancellationToken);

        return result;
    }

    /// <inheritdoc />
    public virtual async Task<IPaymentVerifyResult> VerifyAsync(long trackingNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(LoggingEvents.VerifyPayment, "Verifying the invoice {TrackingNumber} is started", trackingNumber);

        using var paymentLock = await PaymentLocks.AcquireAsync(trackingNumber, cancellationToken).ConfigureAwaitFalse();

        var payment = await _storage
                           .GetPaymentByTrackingNumberAsync(trackingNumber, cancellationToken)
                           .ConfigureAwaitFalse();

        if (payment == null)
        {
            _logger.LogError(LoggingEvents.VerifyPayment,
                             "Verifying the invoice {TrackingNumber} is failed. The operation is not valid. No payment found with for the given tracking number",
                             trackingNumber);

            throw new InvoiceNotFoundException(trackingNumber);
        }

        if (payment.IsCompleted)
        {
            _logger.LogInformation(LoggingEvents.VerifyPayment,
                                   "Verifying the invoice {TrackingNumber} is finished. The requested payment is already processed before",
                                   trackingNumber);

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

        var transactions = await _storage.GetTransactionsAsync(payment, cancellationToken).ConfigureAwaitFalse();
        var invoiceContext = new InvoiceContext(payment, transactions);

        PaymentVerifyResult verifyResult;

        try
        {
            verifyResult = await gateway
                                .VerifyAsync(invoiceContext, cancellationToken)
                                .ConfigureAwaitFalse() as PaymentVerifyResult;
        }
        catch (Exception exception)
        {
            _logger.LogError(LoggingEvents.VerifyPayment, exception, "Verifying the invoice {TrackingNumber} is finished. An error occurred during requesting", trackingNumber);
            throw;
        }

        if (verifyResult == null) throw new Exception($"The {gateway.GetCompleteGatewayName()} returns null instead of a result.");

        verifyResult.TrackingNumber = payment.TrackingNumber;
        verifyResult.Amount = payment.Amount;
        verifyResult.GatewayName = payment.GatewayName;
        verifyResult.GatewayAccountName = payment.GatewayAccountName;

        await EnforceVerificationRulesAsync(payment, verifyResult, cancellationToken).ConfigureAwaitFalse();

        var status = verifyResult.IsSucceed ? PaymentStatus.Paid : PaymentStatus.Failed;

        // Only a paid payment claims its bank reference: one bank reference settles only one payment.
        var bankReference = verifyResult.IsSucceed && !verifyResult.TransactionCode.IsNullOrEmpty() ? verifyResult.TransactionCode : null;

        if (!await _storage
                  .TryCompletePaymentAsync(payment.Id, status, verifyResult.TransactionCode, bankReference, cancellationToken)
                  .ConfigureAwaitFalse())
        {
            // Another verification or a cancellation completed the payment first, e.g. on another server.
            var storedPayment = await _storage
                                     .GetPaymentByTrackingNumberAsync(trackingNumber, cancellationToken)
                                     .ConfigureAwaitFalse();

            _logger.LogWarning(LoggingEvents.VerifyPayment,
                               "Verifying the invoice {TrackingNumber}: another operation completed the payment first (paid: {StoredIsPaid}). This verification's result ({Status}) is not stored",
                               trackingNumber,
                               storedPayment.IsPaid,
                               verifyResult.Status);

            return new PaymentVerifyResult
                   {
                       TrackingNumber = storedPayment.TrackingNumber,
                       Amount = storedPayment.Amount,
                       GatewayName = storedPayment.GatewayName,
                       GatewayAccountName = storedPayment.GatewayAccountName,
                       TransactionCode = storedPayment.TransactionCode,
                       Status = storedPayment.IsPaid ? PaymentVerifyResultStatus.AlreadyVerified : PaymentVerifyResultStatus.Failed,
                       Message = _options.Messages.PaymentIsAlreadyProcessedBefore
                   };
        }

        var transaction = new Transaction
                          {
                              Amount = verifyResult.Amount,
                              IsSucceed = verifyResult.IsSucceed,
                              Message = verifyResult.Message,
                              Type = TransactionType.Verify,
                              AdditionalData = AdditionalDataConverter.ToJson(verifyResult),
                              PaymentId = payment.Id
                          };

        await _storage.CreateTransactionAsync(transaction, cancellationToken).ConfigureAwaitFalse();

        _logger.LogInformation(LoggingEvents.VerifyPayment, "Verifying the invoice {TrackingNumber} is finished", trackingNumber);

        return verifyResult;
    }

    /// <inheritdoc />
    public virtual async Task<IPaymentCancelResult> CancelAsync(long trackingNumber, string cancellationReason = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(LoggingEvents.CancelPayment, "Canceling the invoice {TrackingNumber} is started", trackingNumber);

        using var paymentLock = await PaymentLocks.AcquireAsync(trackingNumber, cancellationToken).ConfigureAwaitFalse();

        var payment = await _storage
                           .GetPaymentByTrackingNumberAsync(trackingNumber, cancellationToken)
                           .ConfigureAwaitFalse();

        if (payment == null)
        {
            _logger.LogError(LoggingEvents.CancelPayment,
                             "Canceling the invoice {TrackingNumber} is failed. The operation is not valid. No payment found for the given tracking number",
                             trackingNumber);

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

        _logger.LogInformation(LoggingEvents.CancelPayment, message);

        if (!await _storage
                  .TryCompletePaymentAsync(payment.Id, PaymentStatus.Cancelled, payment.TransactionCode, bankReference: null, cancellationToken)
                  .ConfigureAwaitFalse())
        {
            // A verification completed the payment first, e.g. on another server.
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

        var newTransaction = new Transaction
                             {
                                 Amount = payment.Amount,
                                 IsSucceed = false,
                                 Message = message,
                                 Type = TransactionType.Cancel,
                                 PaymentId = payment.Id
                             };

        await _storage.CreateTransactionAsync(newTransaction, cancellationToken).ConfigureAwaitFalse();

        _logger.LogInformation(LoggingEvents.CancelPayment, "Canceling the invoice {TrackingNumber} is finished", trackingNumber);

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

        _logger.LogInformation(LoggingEvents.RefundPayment, "Refunding the invoice {TrackingNumber} is started", invoice.TrackingNumber);

        // Refunds of one payment run one at a time within this process, so two of them cannot both pass the check of
        // the remaining amount.
        using var paymentLock = await PaymentLocks.AcquireAsync(invoice.TrackingNumber, cancellationToken).ConfigureAwaitFalse();

        var payment = await _storage.GetPaymentByTrackingNumberAsync(invoice.TrackingNumber, cancellationToken).ConfigureAwaitFalse();

        if (payment == null)
        {
            _logger.LogError(LoggingEvents.RefundPayment,
                             "Refunding the invoice {TrackingNumber} is failed. The operation is not valid. No payment found for the given tracking number",
                             invoice.TrackingNumber);

            throw new InvoiceNotFoundException(invoice.TrackingNumber);
        }

        if (!payment.IsCompleted)
        {
            var message = $"{_options.Messages.OnlyCompletedPaymentCanBeRefunded} Tracking number: {invoice.TrackingNumber}.";

            _logger.LogInformation(LoggingEvents.RefundPayment,
                                   "Refunding the invoice {TrackingNumber} is finished. Only a completed payment can be refunded",
                                   invoice.TrackingNumber);

            return PaymentRefundResult.Failed(message);
        }

        // A completed payment can also be a failed or cancelled one, which has nothing to refund.
        if (!payment.IsPaid)
        {
            _logger.LogInformation(LoggingEvents.RefundPayment,
                                   "Refunding the invoice {TrackingNumber} is finished. Only a paid payment can be refunded",
                                   invoice.TrackingNumber);

            return PaymentRefundResult.Failed($"{_options.Messages.OnlyPaidPaymentCanBeRefunded} Tracking number: {invoice.TrackingNumber}.");
        }

        var transactions = await _storage.GetTransactionsAsync(payment, cancellationToken).ConfigureAwaitFalse();

        // The refund transactions also count, for payments refunded before Parbad 4.0 kept RefundedAmount.
        var alreadyRefunded = Math.Max(payment.RefundedAmount,
                                       transactions
                                          .Where(transaction => transaction.Type == TransactionType.Refund && transaction.IsSucceed)
                                          .Sum(transaction => transaction.Amount));

        var remainingAmount = payment.Amount - alreadyRefunded;

        Money amountToRefund;

        if (invoice.Amount > payment.Amount)
        {
            throw new Exception("Amount cannot be greater than the amount of the paid payment.");
        }

        if (remainingAmount <= 0 || invoice.Amount > remainingAmount)
        {
            _logger.LogInformation(LoggingEvents.RefundPayment,
                                   "Refunding the invoice {TrackingNumber} is finished. The refund exceeds the remaining amount {RemainingAmount}",
                                   invoice.TrackingNumber,
                                   remainingAmount);

            return PaymentRefundResult.Failed($"{_options.Messages.RefundExceedsRemainingAmount} Tracking number: {invoice.TrackingNumber}.");
        }

        // Zero means "refund completely", i.e. everything that has not been refunded yet.
        amountToRefund = invoice.Amount == 0 ? remainingAmount : invoice.Amount;

        var gateway = _gatewayProvider.Provide(payment.GatewayName);

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
            _logger.LogError(exception, "Parbad exception. An error occurred during requesting");
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

        await _storage.CreateTransactionAsync(newtTransaction, cancellationToken).ConfigureAwaitFalse();

        if (refundResult.IsSucceed)
        {
            payment.RefundedAmount = alreadyRefunded + amountToRefund;
            payment.Status = payment.RefundedAmount >= payment.Amount ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;

            await _storage.UpdatePaymentAsync(payment, cancellationToken).ConfigureAwaitFalse();
        }

        _logger.LogInformation(LoggingEvents.RefundPayment, "Refunding the invoice {TrackingNumber} is finished", invoice.TrackingNumber);

        return refundResult;
    }

    /// <summary>
    /// Checks that every gateway gets for free, so a gateway cannot forget them: the bank-reported amount and
    /// order id must match the invoice, and one bank transaction can settle only one payment. An "already
    /// verified" answer that the gateway could not tie to this invoice is accepted only after the last check.
    /// </summary>
    private async Task EnforceVerificationRulesAsync(Payment payment, PaymentVerifyResult verifyResult, CancellationToken cancellationToken)
    {
        if (!verifyResult.IsSucceed && !verifyResult.IsAlreadyVerifiedAtBank) return;

        string failure = null;

        if (verifyResult.BankReportedAmount.HasValue && verifyResult.BankReportedAmount.Value.Value != payment.Amount)
        {
            failure = _options.Messages.BankReportedDifferentAmount;
        }
        else if (verifyResult.BankReportedOrderId != null &&
                 verifyResult.BankReportedOrderId != payment.TrackingNumber.ToString(CultureInfo.InvariantCulture))
        {
            failure = _options.Messages.BankReportedDifferentInvoice;
        }
        else if (!verifyResult.TransactionCode.IsNullOrEmpty())
        {
            var otherPayment = await _storage
                                    .FindPaidPaymentByBankReferenceAsync(payment.GatewayName,
                                                                         verifyResult.TransactionCode,
                                                                         payment.TrackingNumber,
                                                                         cancellationToken)
                                    .ConfigureAwaitFalse();

            if (otherPayment != null)
            {
                _logger.LogWarning(LoggingEvents.VerifyPayment,
                                   "Verifying the invoice {TrackingNumber}: the bank reference {BankReference} already settled the invoice {OtherTrackingNumber}",
                                   payment.TrackingNumber,
                                   verifyResult.TransactionCode,
                                   otherPayment.TrackingNumber);

                failure = _options.Messages.TransactionCodeAlreadyUsed;
            }
        }
        else if (verifyResult.IsAlreadyVerifiedAtBank)
        {
            failure = _options.Messages.AlreadyVerifiedPaymentCannotBeConfirmed;
        }

        if (failure != null)
        {
            _logger.LogWarning(LoggingEvents.VerifyPayment,
                               "Verifying the invoice {TrackingNumber} is rejected by Parbad: {Reason}",
                               payment.TrackingNumber,
                               failure);

            verifyResult.Status = PaymentVerifyResultStatus.Failed;
            verifyResult.Message = failure;

            return;
        }

        if (verifyResult.IsAlreadyVerifiedAtBank)
        {
            verifyResult.Status = PaymentVerifyResultStatus.Succeed;
            verifyResult.Message = _options.Messages.PaymentSucceed;
        }
    }

    private async Task<IPaymentFetchResult> FetchAsync(Payment payment, CancellationToken cancellationToken)
    {
        var fetchResult = new PaymentFetchResult
                          {
                              TrackingNumber = payment.TrackingNumber,
                              Amount = payment.Amount,
                              GatewayName = payment.GatewayName,
                              GatewayAccountName = payment.GatewayAccountName,
                              IsAlreadyVerified = payment.IsPaid
                          };

        if (payment.IsCompleted)
        {
            fetchResult.Status = PaymentFetchResultStatus.AlreadyProcessed;
            fetchResult.Message = _options.Messages.PaymentIsAlreadyProcessedBefore;
            fetchResult.TransactionCode = payment.TransactionCode;

            return fetchResult;
        }

        var gateway = _gatewayProvider.Provide(payment.GatewayName);

        var transactions = await _storage.GetTransactionsAsync(payment, cancellationToken).ConfigureAwaitFalse();
        var invoiceContext = new InvoiceContext(payment, transactions);

        PaymentFetchResult gatewayFetchResult;

        try
        {
            gatewayFetchResult = await gateway
                                      .FetchAsync(invoiceContext, cancellationToken)
                                      .ConfigureAwaitFalse() as PaymentFetchResult;
        }
        catch (Exception exception)
        {
            _logger.LogError(LoggingEvents.VerifyPayment,
                             exception,
                             "Fetching the invoice {TrackingNumber} is finished. An error occurred during requesting",
                             payment.TrackingNumber);
            throw;
        }

        if (gatewayFetchResult == null) throw new Exception($"The {gateway.GetCompleteGatewayName()} returns null instead of a result.");

        _logger.LogInformation(LoggingEvents.FetchPayment, "Fetching is finished");

        fetchResult.Status = gatewayFetchResult.Status;

        string message = null;
        if (gatewayFetchResult.Status != PaymentFetchResultStatus.ReadyForVerifying)
        {
            message = gatewayFetchResult.Message ?? _options.Messages.PaymentFailed;
        }
        fetchResult.Message = message;

        fetchResult.GatewayResponseCode = gatewayFetchResult.GatewayResponseCode;

        return fetchResult;
    }
}