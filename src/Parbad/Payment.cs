using System;
using System.Threading.Tasks;
using System.Web;
using Parbad.Configurations;
using Parbad.Core;
using Parbad.Exceptions;
using Parbad.Infrastructure.Data;
using Parbad.Infrastructure.Logging;
using Parbad.Providers;
using Parbad.Web;

namespace Parbad
{
    /// <summary>
    /// General Payment class
    /// </summary>
    public static class Payment
    {
        /// <summary>
        /// Sends pay request to selected gateway.
        /// </summary>
        /// <param name="gateway">Gateway to pay</param>
        /// <param name="invoice">Invoice object</param>
        public static RequestResult Request(Gateway gateway, Invoice invoice)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }

            ThrowExceptionIfGatewayIsNotConfigured(gateway);

            var paymentData = SelectPaymentDataByOrderNumber(invoice.OrderNumber);

            if (paymentData != null)
            {
                //  Log
                TryLog(() => new Log
                {
                    Type = LogType.Request,
                    Gateway = gateway,
                    OrderNumber = invoice.OrderNumber,
                    Amount = invoice.Amount,
                    Message = $"Order Number ({invoice.OrderNumber}) is used before and you cannot use it again. It must be unique for each requests.",
                    CreatedOn = DateTime.Now,
                    ReferenceId = string.Empty,
                    TransactionId = string.Empty,
                    Status = RequestResultStatus.DuplicateOrderNumber.ToString()
                });

                return new RequestResult(RequestResultStatus.DuplicateOrderNumber, $"The order number ({invoice.OrderNumber}) is already exists and you cannot use it again. It must be unique for each requests.");
            }

            var gatewayBase = GatewayFactory.CreateGateway(gateway);

            paymentData = invoice.CreatePaymentData(gateway);

            invoice.CallbackUrl = CreateCallbackUrl(invoice.CallbackUrl, paymentData.Id);

            try
            {
                var result = gatewayBase.Request(invoice);

                //  Set ReferenceId
                paymentData.ReferenceId = result.ReferenceId;
                paymentData.AdditionalData = result.AdditionalData;

                if (!result.IsSuccess())
                {
                    paymentData.Status = PaymentDataStatus.Failed;
                }

                //  Log
                TryLog(() => new Log
                {
                    Type = LogType.Request,
                    Gateway = gateway,
                    OrderNumber = invoice.OrderNumber,
                    Amount = invoice.Amount,
                    Message = result.Message,
                    CreatedOn = DateTime.Now,
                    ReferenceId = result.ReferenceId,
                    TransactionId = string.Empty,
                    Status = result.Status.ToString()
                });

                return result;
            }
            catch (Exception exception)
            {
                //  Log
                TryLog(() => new Log
                {
                    Type = LogType.Error,
                    Gateway = gateway,
                    OrderNumber = invoice.OrderNumber,
                    Amount = invoice.Amount,
                    Message = exception.Message,
                    CreatedOn = DateTime.Now
                });

                paymentData.Status = PaymentDataStatus.Failed;

                return new RequestResult(RequestResultStatus.Failed, "An error occurred.");
            }
            finally
            {
                InsertPaymentData(paymentData);
            }
        }

        /// <summary>
        /// Sends pay request to selected gateway.
        /// </summary>
        /// <param name="gateway">Gateway to pay</param>
        /// <param name="invoice">Invoice object</param>
        public static async Task<RequestResult> RequestAsync(Gateway gateway, Invoice invoice)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }

            ThrowExceptionIfGatewayIsNotConfigured(gateway);

            var paymentData = await SelectPaymentDataByOrderNumberAsync(invoice.OrderNumber);

            if (paymentData != null)
            {
                //  Log
                TryLog(() => new Log
                {
                    Type = LogType.Request,
                    Gateway = gateway,
                    OrderNumber = invoice.OrderNumber,
                    Amount = invoice.Amount,
                    Message = $"Order Number ({invoice.OrderNumber}) is used before and you cannot use it again. It must be unique for each requests.",
                    CreatedOn = DateTime.Now,
                    ReferenceId = string.Empty,
                    TransactionId = string.Empty,
                    Status = RequestResultStatus.DuplicateOrderNumber.ToString()
                });

                return new RequestResult(RequestResultStatus.DuplicateOrderNumber, $"The order number ({invoice.OrderNumber}) is already exists and you cannot use it again. It must be unique for each requests.");
            }

            var gatewayBase = GatewayFactory.CreateGateway(gateway);

            paymentData = invoice.CreatePaymentData(gateway);

            invoice.CallbackUrl = CreateCallbackUrl(invoice.CallbackUrl, paymentData.Id);

            try
            {
                var result = await gatewayBase.RequestAsync(invoice);

                //  Set ReferenceId
                paymentData.ReferenceId = result.ReferenceId;
                paymentData.AdditionalData = result.AdditionalData;

                if (!result.IsSuccess())
                {
                    paymentData.Status = PaymentDataStatus.Failed;
                }

                //  Log
                TryLog(() => new Log
                {
                    Type = LogType.Request,
                    Gateway = gateway,
                    OrderNumber = invoice.OrderNumber,
                    Amount = invoice.Amount,
                    Message = result.Message,
                    CreatedOn = DateTime.Now,
                    ReferenceId = result.ReferenceId,
                    TransactionId = string.Empty,
                    Status = result.Status.ToString()
                });

                return result;
            }
            catch (Exception exception)
            {
                //  Log
                TryLog(() => new Log
                {
                    Type = LogType.Error,
                    Gateway = gateway,
                    OrderNumber = invoice.OrderNumber,
                    Amount = invoice.Amount,
                    Message = exception.Message,
                    CreatedOn = DateTime.Now
                });

                paymentData.Status = PaymentDataStatus.Failed;

                return new RequestResult(RequestResultStatus.Failed, "An error occurred.");
            }
            finally
            {
                await InsertPaymentDataAsync(paymentData);
            }
        }

        /// <summary>
        /// Verifies request that comes from a gateway.
        /// </summary>
        /// <param name="httpContext">HttpContext object of current request.</param>
        /// <param name="paymentVerifyingHandler">Describes the invoice which sent by the gateway. You can compare its data with your database and also cancel the payment operation if you need.</param>
        public static VerifyResult Verify(HttpContext httpContext, Action<IPaymentVerifyingContext> paymentVerifyingHandler = null)
        {
            return Verify(new HttpContextWrapper(httpContext), paymentVerifyingHandler);
        }

        /// <summary>
        /// Verifies request that comes from a gateway.
        /// </summary>
        /// <param name="httpContext">HttpContext object of current request.</param>
        /// <param name="paymentVerifyingHandler">Describes the invoice which sent by the gateway. You can compare its data with your database and also cancel the payment operation if you need.</param>
        public static VerifyResult Verify(HttpContextBase httpContext, Action<IPaymentVerifyingContext> paymentVerifyingHandler = null)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            //  1) Get PaymentData's ID from HttpRequest
            if (!httpContext.Request.TryGetPaymentDataId(out var paymentId))
            {
                return new VerifyResult(0, string.Empty, string.Empty, VerifyResultStatus.NotValid, "Payment's ID is not valid.");
            }

            //  2) Load PaymentData by ID from storage
            var paymentData = SelectPaymentDataById(paymentId);

            if (paymentData == null)
            {
                return new VerifyResult(0, string.Empty, string.Empty, VerifyResultStatus.NotValid, $"No payment found with ID: {paymentId:N}.");
            }

            ThrowExceptionIfGatewayIsNotConfigured(paymentData.Gateway);

            //  Some paymentData's checks...

            if (paymentData.Status != PaymentDataStatus.Requested)
            {
                if (paymentData.Status == PaymentDataStatus.Verified)
                {
                    return new VerifyResult(paymentData.Gateway, paymentData.ReferenceId, paymentData.TransactionId, VerifyResultStatus.AlreadyVerified, "The payment is already verified.");
                }

                return new VerifyResult(paymentData.Gateway, paymentData.ReferenceId, paymentData.TransactionId, VerifyResultStatus.NotValid, "Payment is not valid.");
            }

            if (paymentData.IsExpired())
            {
                return new VerifyResult(paymentData.Gateway, paymentData.ReferenceId, paymentData.TransactionId, VerifyResultStatus.PaymentTimeExpired, "The time of payment is expired.");
            }

            //  Let developer decides to continue or stop the payment
            if (paymentVerifyingHandler != null)
            {
                var paymentVerifyContext = new PaymentVerifyingContext(paymentData.Gateway, paymentData.OrderNumber, paymentData.ReferenceId);

                paymentVerifyingHandler.Invoke(paymentVerifyContext);

                if (paymentVerifyContext.IsCanceled)
                {
                    //  Log canceling
                    TryLog(() => new Log
                    {
                        Type = LogType.Verify,
                        Gateway = paymentData.Gateway,
                        OrderNumber = paymentData.OrderNumber,
                        Amount = paymentData.Amount,
                        Message = paymentVerifyContext.CancellationReason,
                        CreatedOn = DateTime.Now,
                        ReferenceId = paymentData.ReferenceId,
                        TransactionId = string.Empty,
                        Status = VerifyResultStatus.CanceledProgrammatically.ToString()
                    });

                    paymentData.Status = PaymentDataStatus.Failed;

                    //  Update PaymentData in the Storage
                    UpdatePaymentData(paymentData);

                    return new VerifyResult(paymentData.Gateway, paymentData.ReferenceId, paymentData.TransactionId, VerifyResultStatus.CanceledProgrammatically, paymentVerifyContext.CancellationReason);
                }
            }

            //  Making ready data to verify

            //  Incoming request parameters
            IRequestParameters requestParameters = new RequestParameters(httpContext.Request);

            //  Create VerifyContext
            var gatewayVerifyPaymentContext = CreateGatewayVerifyPaymentContext(paymentData, requestParameters);

            //  4) Create gateway from PaymentData's Gateway
            var gateway = GatewayFactory.CreateGateway(paymentData.Gateway);

            //  5) Verify
            var verifyResult = gateway.Verify(gatewayVerifyPaymentContext);

            //  Log Verify
            TryLog(() => new Log
            {
                Type = LogType.Verify,
                Gateway = paymentData.Gateway,
                OrderNumber = paymentData.OrderNumber,
                Amount = paymentData.Amount,
                Message = verifyResult.Message,
                CreatedOn = DateTime.Now,
                ReferenceId = paymentData.ReferenceId,
                TransactionId = verifyResult.TransactionId,
                Status = verifyResult.Status.ToString()
            });

            //  Update PaymentData
            paymentData.TransactionId = verifyResult.TransactionId;
            paymentData.Status = verifyResult.IsSuccess() ? PaymentDataStatus.Verified : PaymentDataStatus.Failed;

            //  Save PaymentData to the storage
            UpdatePaymentData(paymentData);

            return verifyResult;
        }

        /// <summary>
        /// Verifies request that comes from a gateway.
        /// </summary>
        /// <param name="httpContext">HttpContext object of current request.</param>
        /// <param name="paymentVerifyingHandler">Describes the invoice which sent by the gateway. You can compare its data with your database and also cancel the payment operation if you need.</param>
        public static Task<VerifyResult> VerifyAsync(HttpContext httpContext, Action<IPaymentVerifyingContext> paymentVerifyingHandler = null)
        {
            return VerifyAsync(new HttpContextWrapper(httpContext), paymentVerifyingHandler);
        }

        /// <summary>
        /// Verifies request that comes from a gateway.
        /// </summary>
        /// <param name="httpContext">HttpContext object of current request.</param>
        /// <param name="paymentVerifyingHandler">Describes the invoice which sent by the gateway. You can compare its data with your database and also cancel the payment operation if you need.</param>
        public static async Task<VerifyResult> VerifyAsync(HttpContextBase httpContext, Action<IPaymentVerifyingContext> paymentVerifyingHandler = null)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            //  1) Get PaymentData's ID from HttpRequest
            if (!httpContext.Request.TryGetPaymentDataId(out var paymentId))
            {
                return new VerifyResult(0, string.Empty, string.Empty, VerifyResultStatus.NotValid, "Payment's ID is not valid.");
            }

            //  2) Load PaymentData by ID from storage
            var paymentData = await SelectPaymentDataByIdAsync(paymentId);

            if (paymentData == null)
            {
                return new VerifyResult(0, string.Empty, string.Empty, VerifyResultStatus.NotValid, $"No payment found with ID: {paymentId:N}.");
            }

            ThrowExceptionIfGatewayIsNotConfigured(paymentData.Gateway);

            //  Some paymentData's checks...

            if (paymentData.Status != PaymentDataStatus.Requested)
            {
                if (paymentData.Status == PaymentDataStatus.Verified)
                {
                    return new VerifyResult(paymentData.Gateway, paymentData.ReferenceId, paymentData.TransactionId, VerifyResultStatus.AlreadyVerified, "The payment is already verified.");
                }

                return new VerifyResult(paymentData.Gateway, paymentData.ReferenceId, paymentData.TransactionId, VerifyResultStatus.NotValid, "Payment is not valid.");
            }

            if (paymentData.IsExpired())
            {
                return new VerifyResult(paymentData.Gateway, paymentData.ReferenceId, paymentData.TransactionId, VerifyResultStatus.PaymentTimeExpired, "The time of payment is expired.");
            }

            //  Let developer decides to continue or stop the payment
            if (paymentVerifyingHandler != null)
            {
                var paymentVerifyContext = new PaymentVerifyingContext(paymentData.Gateway, paymentData.OrderNumber, paymentData.ReferenceId);

                paymentVerifyingHandler.Invoke(paymentVerifyContext);

                if (paymentVerifyContext.IsCanceled)
                {
                    //  Log canceling
                    TryLog(() => new Log
                    {
                        Type = LogType.Verify,
                        Gateway = paymentData.Gateway,
                        OrderNumber = paymentData.OrderNumber,
                        Amount = paymentData.Amount,
                        Message = paymentVerifyContext.CancellationReason,
                        CreatedOn = DateTime.Now,
                        ReferenceId = paymentData.ReferenceId,
                        TransactionId = string.Empty,
                        Status = VerifyResultStatus.CanceledProgrammatically.ToString()
                    });

                    paymentData.Status = PaymentDataStatus.Failed;

                    //  Update PaymentData in the Storage
                    await UpdatePaymentDataAsync(paymentData);

                    return new VerifyResult(paymentData.Gateway, paymentData.ReferenceId, paymentData.TransactionId, VerifyResultStatus.CanceledProgrammatically, paymentVerifyContext.CancellationReason);
                }
            }

            //  Making ready data to verify

            //  Incoming request parameters
            IRequestParameters requestParameters = new RequestParameters(httpContext.Request);

            //  Create VerifyContext
            var gatewayVerifyPaymentContext = CreateGatewayVerifyPaymentContext(paymentData, requestParameters);

            //  4) Create gateway from PaymentData's Gateway
            var gateway = GatewayFactory.CreateGateway(paymentData.Gateway);

            //  5) Verify
            var verifyResult = await gateway.VerifyAsync(gatewayVerifyPaymentContext);

            //  Log Verify
            TryLog(() => new Log
            {
                Type = LogType.Verify,
                Gateway = paymentData.Gateway,
                OrderNumber = paymentData.OrderNumber,
                Amount = paymentData.Amount,
                Message = verifyResult.Message,
                CreatedOn = DateTime.Now,
                ReferenceId = paymentData.ReferenceId,
                TransactionId = verifyResult.TransactionId,
                Status = verifyResult.Status.ToString()
            });

            //  Update PaymentData
            paymentData.TransactionId = verifyResult.TransactionId;
            paymentData.Status = verifyResult.IsSuccess() ? PaymentDataStatus.Verified : PaymentDataStatus.Failed;

            //  Save PaymentData to the storage
            await UpdatePaymentDataAsync(paymentData);

            return verifyResult;
        }

        /// <summary>
        /// Refunds a specific payment by its Order Number.
        /// </summary>
        /// <param name="refundInvoice">RefundInvoice object</param>
        public static RefundResult Refund(RefundInvoice refundInvoice)
        {
            if (refundInvoice == null)
            {
                throw new ArgumentNullException(nameof(refundInvoice));
            }

            var paymentData = SelectPaymentDataByOrderNumber(refundInvoice.OrderNumber);

            if (paymentData == null)
            {
                return new RefundResult(0, 0, RefundResultStatus.Failed, $"No payment found with order's number: {refundInvoice.OrderNumber}");
            }

            if (refundInvoice.AmountToRefund > paymentData.Amount)
            {
                return new RefundResult(0, 0, RefundResultStatus.Failed, $"Amount To Refund cannot be greater than original amount. Original Amount: {paymentData.Amount:N0}. Amount To Refund: {refundInvoice.AmountToRefund:N0}");
            }

            ThrowExceptionIfGatewayIsNotConfigured(paymentData.Gateway);

            var gateway = GatewayFactory.CreateGateway(paymentData.Gateway);

            var amountToRefund = refundInvoice.AmountToRefund > 0 ? refundInvoice.AmountToRefund : paymentData.Amount;

            var gatewayRefundPaymentContext = new GatewayRefundPaymentContext(paymentData.OrderNumber, amountToRefund, paymentData.ReferenceId, paymentData.TransactionId, paymentData.AdditionalData);

            try
            {
                var refundResult = gateway.Refund(gatewayRefundPaymentContext);

                //  Log
                TryLog(() => new Log
                {
                    Type = LogType.Refund,
                    Gateway = paymentData.Gateway,
                    OrderNumber = paymentData.OrderNumber,
                    Amount = paymentData.Amount,
                    Message = refundResult.Message,
                    CreatedOn = DateTime.Now,
                    ReferenceId = paymentData.ReferenceId,
                    TransactionId = paymentData.TransactionId,
                    Status = refundResult.Status.ToString()
                });

                if (refundResult.IsSuccess())
                {
                    paymentData.Status = PaymentDataStatus.Refunded;

                    UpdatePaymentData(paymentData);
                }

                return refundResult;
            }
            catch (Exception exception)
            {
                //  Log
                TryLog(() => new Log
                {
                    Type = LogType.Error,
                    Gateway = paymentData.Gateway,
                    OrderNumber = paymentData.OrderNumber,
                    Amount = amountToRefund,
                    Message = exception.Message,
                    CreatedOn = DateTime.Now,
                    ReferenceId = paymentData.ReferenceId,
                    TransactionId = paymentData.TransactionId,
                    Status = string.Empty
                });

                return new RefundResult(paymentData.Gateway, 0, RefundResultStatus.Failed, exception.Message);
            }
        }

        /// <summary>
        /// Refunds a specific payment by its Order Number.
        /// </summary>
        /// <param name="refundInvoice">RefundInvoice object</param>
        public static async Task<RefundResult> RefundAsync(RefundInvoice refundInvoice)
        {
            if (refundInvoice == null)
            {
                throw new ArgumentNullException(nameof(refundInvoice));
            }

            var paymentData = await SelectPaymentDataByOrderNumberAsync(refundInvoice.OrderNumber);

            if (paymentData == null)
            {
                return new RefundResult(0, 0, RefundResultStatus.Failed, $"No payment found with order's number: {refundInvoice.OrderNumber}");
            }

            if (refundInvoice.AmountToRefund > paymentData.Amount)
            {
                return new RefundResult(0, 0, RefundResultStatus.Failed, $"Amount To Refund cannot be greater than original amount. Original Amount: {paymentData.Amount:N0}. Amount To Refund: {refundInvoice.AmountToRefund:N0}");
            }

            ThrowExceptionIfGatewayIsNotConfigured(paymentData.Gateway);

            var gateway = GatewayFactory.CreateGateway(paymentData.Gateway);

            var amountToRefund = refundInvoice.AmountToRefund > 0 ? refundInvoice.AmountToRefund : paymentData.Amount;

            var gatewayRefundPaymentContext = new GatewayRefundPaymentContext(paymentData.OrderNumber, amountToRefund, paymentData.ReferenceId, paymentData.TransactionId, paymentData.AdditionalData);

            try
            {
                var refundResult = await gateway.RefundAsync(gatewayRefundPaymentContext);

                //  Log
                TryLog(() => new Log
                {
                    Type = LogType.Refund,
                    Gateway = paymentData.Gateway,
                    OrderNumber = paymentData.OrderNumber,
                    Amount = paymentData.Amount,
                    Message = refundResult.Message,
                    CreatedOn = DateTime.Now,
                    ReferenceId = paymentData.ReferenceId,
                    TransactionId = paymentData.TransactionId,
                    Status = refundResult.Status.ToString()
                });

                if (refundResult.IsSuccess())
                {
                    paymentData.Status = PaymentDataStatus.Refunded;

                    await UpdatePaymentDataAsync(paymentData);
                }

                return refundResult;
            }
            catch (Exception exception)
            {
                //  Log
                TryLog(() => new Log
                {
                    Type = LogType.Error,
                    Gateway = paymentData.Gateway,
                    OrderNumber = paymentData.OrderNumber,
                    Amount = amountToRefund,
                    Message = exception.Message,
                    CreatedOn = DateTime.Now,
                    ReferenceId = paymentData.ReferenceId,
                    TransactionId = paymentData.TransactionId,
                    Status = string.Empty
                });

                return new RefundResult(paymentData.Gateway, 0, RefundResultStatus.Failed, exception.Message);
            }
        }

        private static GatewayVerifyPaymentContext CreateGatewayVerifyPaymentContext(PaymentData paymentData, IRequestParameters requestParameters)
        {
            return new GatewayVerifyPaymentContext(
                paymentData.OrderNumber,
                paymentData.Amount,
                paymentData.ReferenceId,
                paymentData.CreatedOn,
                paymentData.AdditionalData,
                requestParameters);
        }

        private static string CreateCallbackUrl(string callbackUrl, Guid paymentDataId)
        {
            var uri = new Uri(callbackUrl);

            return uri.AppendQueryString("paymentID", paymentDataId.ToString("N")).ToString();
        }

        private static void ThrowExceptionIfGatewayIsNotConfigured(Gateway gateway)
        {
            if (!ParbadConfiguration.Gateways.IsGatewayConfigured(gateway))
            {
                throw new GatewayConfigurationException(gateway);
            }
        }

        /// <summary>
        /// Log without throwing any exceptions
        /// </summary>
        private static void TryLog(Func<Log> func)
        {
            try
            {
                ParbadConfiguration.Logger.Provider?.Write(func());
            }
            catch
            {
                // ignored
            }
        }

        #region Storage methods

        private static PaymentData SelectPaymentDataById(Guid paymentId)
        {
            if (ParbadConfiguration.Storage == null)
            {
                throw new StorageNotImplementedException();
            }

            return ParbadConfiguration.Storage.SelectById(paymentId);
        }
        private static Task<PaymentData> SelectPaymentDataByIdAsync(Guid paymentId)
        {
            if (ParbadConfiguration.Storage == null)
            {
                throw new StorageNotImplementedException();
            }

            return ParbadConfiguration.Storage.SelectByIdAsync(paymentId);
        }

        private static PaymentData SelectPaymentDataByOrderNumber(long orderNumber)
        {
            if (ParbadConfiguration.Storage == null)
            {
                throw new StorageNotImplementedException();
            }

            return ParbadConfiguration.Storage.SelectByOrderNumber(orderNumber);
        }
        private static Task<PaymentData> SelectPaymentDataByOrderNumberAsync(long orderNumber)
        {
            if (ParbadConfiguration.Storage == null)
            {
                throw new StorageNotImplementedException();
            }

            return ParbadConfiguration.Storage.SelectByOrderNumberAsync(orderNumber);
        }

        private static void InsertPaymentData(PaymentData paymentData)
        {
            if (ParbadConfiguration.Storage == null)
            {
                throw new StorageNotImplementedException();
            }

            ParbadConfiguration.Storage.Insert(paymentData);
        }
        private static Task InsertPaymentDataAsync(PaymentData paymentData)
        {
            if (ParbadConfiguration.Storage == null)
            {
                throw new StorageNotImplementedException();
            }

            return ParbadConfiguration.Storage.InsertAsync(paymentData);
        }

        private static void UpdatePaymentData(PaymentData paymentData)
        {
            if (ParbadConfiguration.Storage == null)
            {
                throw new StorageNotImplementedException();
            }

            ParbadConfiguration.Storage.Update(paymentData);
        }
        private static Task UpdatePaymentDataAsync(PaymentData paymentData)
        {
            if (ParbadConfiguration.Storage == null)
            {
                throw new StorageNotImplementedException();
            }

            return ParbadConfiguration.Storage.UpdateAsync(paymentData);
        }

        #endregion
    }
}