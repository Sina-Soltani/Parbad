using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.GatewayBuilders;
using Parbad.Internal;

namespace Parbad.PaymentFlow.Tests.Core;

public class FakeGatewayAccount : GatewayAccount
{
}

/// <summary>
/// Controls and records what <see cref="FakeGateway"/> does. Registered as a singleton per test.
/// </summary>
public sealed class FakeGatewayBehaviour
{
    private int _verifyCalls;

    public Exception? RequestException { get; set; }

    public Exception? VerifyException { get; set; }

    /// <summary>Awaited inside every verify call; used to make concurrent verifications overlap.</summary>
    public Func<Task> BeforeVerify { get; set; } = () => Task.CompletedTask;

    public int VerifyCalls => _verifyCalls;

    public ConcurrentQueue<decimal> RefundedAmounts { get; } = new();

    internal int RecordVerifyCall() => Interlocked.Increment(ref _verifyCalls);
}

/// <summary>
/// A minimal gateway for testing <see cref="IOnlinePayment"/> itself, without any bank protocol.
/// </summary>
[Gateway(Name)]
public class FakeGateway : GatewayBase<FakeGatewayAccount>
{
    public const string Name = "Fake";
    public const string PaymentPageUrl = "https://bank.test/pay";
    public const string TransactionCode = "RRN-0001";

    private readonly FakeGatewayBehaviour _behaviour;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FakeGateway(
        IGatewayAccountProvider<FakeGatewayAccount> accountProvider,
        FakeGatewayBehaviour behaviour,
        IHttpContextAccessor httpContextAccessor)
        : base(accountProvider)
    {
        _behaviour = behaviour;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        if (_behaviour.RequestException != null) throw _behaviour.RequestException;

        var account = await GetAccountAsync(invoice);

        return PaymentRequestResult.SucceedWithRedirect(account.Name, _httpContextAccessor.HttpContext!, PaymentPageUrl);
    }

    public override Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default) =>
        Task.FromResult<IPaymentFetchResult>(PaymentFetchResult.ReadyForVerifying());

    public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
    {
        _behaviour.RecordVerifyCall();

        await _behaviour.BeforeVerify();

        if (_behaviour.VerifyException != null) throw _behaviour.VerifyException;

        return PaymentVerifyResult.Succeed(TransactionCode, "Paid.");
    }

    public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
    {
        _behaviour.RefundedAmounts.Enqueue(amount);

        return Task.FromResult<IPaymentRefundResult>(PaymentRefundResult.Succeed("Refunded."));
    }
}
