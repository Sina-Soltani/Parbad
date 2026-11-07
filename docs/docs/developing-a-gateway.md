---
sidebar_position: 9
title: Developing a gateway
---

# Developing a gateway

This page builds a gateway for a made-up payment provider called **Hero**, as a package of its own. The gateway
packages of Parbad, such as [Parbad.Gateway.Zibal](https://github.com/Sina-Soltani/Parbad/tree/master/src/Parbad.Gateway/Zibal/src),
are complete examples.

## What a gateway does

A gateway implements four methods. Parbad calls them and does everything around them: it stores the payments, gives
tracking numbers, protects against double verification, and decides the result.

| Method | When | What the gateway does |
|---|---|---|
| `RequestAsync` | The application requests a payment. | Registers the payment with the bank and says how to send the customer there. |
| `FetchAsync` | The customer comes back to the callback URL. | Reads what the bank sent with the customer: did the customer pay? |
| `VerifyAsync` | The application verifies the payment. | Asks the bank to confirm the payment and reports what the bank answered. |
| `RefundAsync` | The application refunds a payment. | Asks the bank to refund, if the bank supports it. |

A gateway **reports what the bank said**; Parbad decides what it means for the payment. For example, Parbad accepts a
bank's reference for one invoice only, and compares the amount the bank reports with the invoice.

## The project

Create a class library for .NET Standard 2.0 or later, named for example `Hero.Parbad.Gateway`, and add Parbad:

```bash
dotnet add package Parbad
```

## The account

The settings an application gives for each of its accounts with the provider:

```csharp
public class HeroGatewayAccount : GatewayAccount
{
    public string MerchantId { get; set; } = null!;
}
```

## The gateway

```csharp
using Microsoft.AspNetCore.Http;
using Parbad;
using Parbad.Abstraction;
using Parbad.GatewayBuilders;
using Parbad.Http;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using Parbad.Net;
using Parbad.Storage.Abstractions.Models;

[Gateway(Name)]
public class HeroGateway : GatewayBase<HeroGatewayAccount>
{
    public const string Name = "Hero";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _httpClient;

    public HeroGateway(IGatewayAccountProvider<HeroGatewayAccount> accountProvider,
                       IHttpContextAccessor httpContextAccessor,
                       IHttpClientFactory httpClientFactory) : base(accountProvider)
    {
        _httpContextAccessor = httpContextAccessor;

        // The client of this gateway, with the timeout and proxy the application configured for it.
        _httpClient = httpClientFactory.CreateClient(this);
    }
}
```

The name in `[Gateway(...)]` is the name applications use with `SetGateway("Hero")`.

### Request

```csharp
public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
{
    var account = await GetAccountAsync(invoice);

    // Register the payment with the bank, e.g. with _httpClient, and get its token.
    var token = await RegisterPaymentAsync(account, invoice.TrackingNumber, invoice.Amount, invoice.CallbackUrl, cancellationToken);

    if (token == null)
    {
        return PaymentRequestResult.Failed("The bank rejected the payment.", account.Name);
    }

    // Send the customer to the bank's page, with a redirect...
    var result = PaymentRequestResult.SucceedWithRedirect(account.Name,
                                                          _httpContextAccessor.GetRequiredHttpContext(),
                                                          $"https://pay.hero.example/{token}");

    // ...or by posting a form:
    // PaymentRequestResult.SucceedWithPost(account.Name, httpContext, url, new Dictionary<string, string> { ... });

    // Data for the later steps. Parbad stores it with the request transaction.
    result.DatabaseAdditionalData.Add("HeroToken", token);

    return result;
}
```

### Fetch

```csharp
public override Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
{
    var request = _httpContextAccessor.GetRequiredHttpContext().Request;

    // What the bank sent with the customer (in the query string or the form, depending on the bank).
    var status = request.Query["status"];

    var result = status == "OK"
                     ? PaymentFetchResult.ReadyForVerifying()
                     : PaymentFetchResult.Failed("The customer cancelled the payment.");

    return Task.FromResult<IPaymentFetchResult>(result);
}
```

### Verify

```csharp
public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
{
    var account = await GetAccountAsync(context.Payment);

    // The data stored by RequestAsync.
    var token = context.Transactions
                       .Single(transaction => transaction.Type == TransactionType.Request)
                       .ToDictionary()["HeroToken"];

    var answer = await VerifyAtBankAsync(account, token, cancellationToken);

    if (!answer.IsPaid)
    {
        return PaymentVerifyResult.Failed(answer.Message, answer.Code);
    }

    // transactionCode: the code the customer sees (e.g. the RRN). bankReference: what the bank verified (here the
    // token). The amount and the order ID that the bank reports let Parbad check them against the invoice.
    return PaymentVerifyResult.Paid(answer.Rrn, token, "The payment was successful.") with
           {
               BankReportedAmount = answer.Amount,
               BankReportedOrderId = answer.OrderId
           };
}
```

When the bank answers "already verified" without proving which payment it means, return
`PaymentVerifyResult.AlreadyVerifiedAtBank(...)`: Parbad then records the payment as paid only if no other payment has
the same bank reference.

If the bank cannot be reached, let the exception propagate: Parbad leaves the payment open, so it can be verified again.

### Refund

```csharp
public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
{
    // A bank without refunds:
    return Task.FromResult<IPaymentRefundResult>(PaymentRefundResult.Failed("Hero does not support refunds."));
}
```

## Extension methods

So that applications can write `AddHero()`, `WithAccounts(...)` and `UseHero()`:

```csharp
public static class HeroGatewayExtensions
{
    public static IGatewayConfigurationBuilder<HeroGateway> AddHero(this IGatewayBuilder builder) =>
        builder.AddGateway<HeroGateway>()
               .WithHttpClient(client => { });

    public static IGatewayConfigurationBuilder<HeroGateway> WithAccounts(
        this IGatewayConfigurationBuilder<HeroGateway> builder,
        Action<IGatewayAccountBuilder<HeroGatewayAccount>> configureAccounts) =>
        builder.WithAccounts(configureAccounts);

    public static IInvoiceBuilder UseHero(this IInvoiceBuilder builder) =>
        builder.SetGateway(HeroGateway.Name);
}
```

## Publishing

Give the project its package metadata (id, description, authors, license, a README), then:

```bash
dotnet pack -c Release
```

and publish the package to nuget.org.
