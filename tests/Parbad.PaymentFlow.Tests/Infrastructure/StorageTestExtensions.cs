using System.Threading;
using System.Threading.Tasks;
using Parbad.Storage.Abstractions;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.PaymentFlow.Tests.Infrastructure;

public static class StorageTestExtensions
{
    /// <summary>Completes a payment as paid (with the transaction code as its bank reference) or as failed.</summary>
    public static Task<bool> TryCompletePaymentAsync(this IStorage storage,
                                                     long paymentId,
                                                     bool isPaid,
                                                     string? transactionCode,
                                                     CancellationToken cancellationToken = default) =>
        storage.TryCompletePaymentAsync(paymentId,
                                        isPaid ? PaymentStatus.Paid : PaymentStatus.Failed,
                                        transactionCode,
                                        isPaid ? transactionCode : null,
                                        cancellationToken);
}
