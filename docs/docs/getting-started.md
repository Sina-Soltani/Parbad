---
sidebar_position: 2
title: Getting started
---

# Getting started

This page takes an ASP.NET Core application from installation to its first verified payment.

## 1. Install the packages

```bash
dotnet add package Parbad
dotnet add package Parbad.AspNetCore
dotnet add package Parbad.Storage.EntityFrameworkCore
```

Add a gateway package too if your gateway is not in the main package; see [Gateways](./intro.md#gateways).

## 2. Configure Parbad

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddMellat()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.TerminalId = 123;
                       account.UserName = "<Your username>";
                       account.UserPassword = "<Your password>";
                   });
               });

           gateways
               .AddParbadVirtual()
               .WithOptions(options => options.GatewayPath = "/MyVirtualGateway");
       })
       .ConfigureHttpContext(httpContext => httpContext.UseDefaultAspNetCore())
       .ConfigureStorage(storage => storage.UseEfCore<ShopDbContext>());
```

`UseEfCore<ShopDbContext>()` keeps Parbad's tables in your own database; see [Storage](./storage.md) for the
`DbContext` setup.

## 3. Request a payment

Inject `IOnlinePayment`, request a payment and send the customer to the bank:

```csharp
using Parbad.AspNetCore;

public async Task<IActionResult> Pay()
{
    var result = await _onlinePayment.RequestAsync(invoice =>
    {
        invoice
            .SetAmount(250_000)
            .SetCallbackUrl(Url.Action("Verify", "Payment", null, Request.Scheme)!)
            .UseAutoIncrementTrackingNumber()
            .UseMellat();
    });

    if (!result.IsSucceed)
    {
        return View("PaymentFailed", result.Message);
    }

    // Store result.TrackingNumber with the order.
    return result.GatewayTransporter.TransportToGateway();
}
```

:::note
Parbad never converts amounts. Set the amount in the unit (Rial or Toman) that the gateway expects.
:::

## 4. Verify the payment

The bank sends the customer back to the callback URL, with either GET or POST:

```csharp
[HttpGet, HttpPost]
public async Task<IActionResult> Verify()
{
    var invoice = await _onlinePayment.FetchAsync();

    if (invoice.Status != PaymentFetchResultStatus.ReadyForVerifying)
    {
        return View("PaymentFailed", invoice.Message);
    }

    if (!IsOrderStillValid(invoice.TrackingNumber))
    {
        await _onlinePayment.CancelAsync(invoice, cancellationReason: "The product is sold out.");

        return View("PaymentFailed", "The product is sold out.");
    }

    var verifyResult = await _onlinePayment.VerifyAsync(invoice);

    if (!verifyResult.IsSucceed)
    {
        return View("PaymentFailed", verifyResult.Message);
    }

    // The payment is complete: deliver the order.
    return View("PaymentSucceeded", verifyResult.TransactionCode);
}
```

Check the order **before** `VerifyAsync`: once a payment is verified, the money is taken, and giving it back needs a
refund.
