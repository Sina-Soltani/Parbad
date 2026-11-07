---
title: Asan Pardakht
---

# Asan Pardakht

The Asan Pardakht gateway is included in the [Parbad](https://www.nuget.org/packages/Parbad/) package.

## Configuration

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddAsanPardakht()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.MerchantConfigurationId = "<Your merchant configuration ID>";
                       account.UserName = "<Your username>";
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
        .UseAsanPardakht();
});
```
