---
title: Saman
---

# Saman

The Saman gateway is included in the [Parbad](https://www.nuget.org/packages/Parbad/) package.

## Configuration

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddSaman()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.TerminalId = "<Your terminal ID>";
                       account.Password = "<Your password>";
                   });
               });
       })
       // ...
```

## Payment request

```csharp
var result = await onlinePayment.RequestAsync(invoice =>
{
    invoice
        .SetAmount(amount)
        .SetCallbackUrl(callbackUrl)
        .UseAutoIncrementTrackingNumber()
        .SetSamanData("09121234567") // Optional: the customer's mobile number
        .UseSaman();
});
```

## Fetch and verification results

`GetSamanAdditionalData()` on a fetch or verification result returns the data that Saman sent: RRN, reference number,
trace number, masked card number, and so on.
