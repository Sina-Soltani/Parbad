---
title: Parsian
---

# Parsian

The Parsian gateway is included in the [Parbad](https://www.nuget.org/packages/Parbad/) package.

## Configuration

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddParsian()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.LoginAccount = "<Your login account>";
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
        .SetParsianAdditionalData(new ParsianGatewayAdditionalDataRequest
        {
            // Optional: additional data, originator
        })
        .UseParsian();
});
```
