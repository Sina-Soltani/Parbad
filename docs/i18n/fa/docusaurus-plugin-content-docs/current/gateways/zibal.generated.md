---
title: زیبال
slug: /gateways/zibal
source_hash: 504846075044
---

# زیبال

```bash
dotnet add package Parbad.Gateway.Zibal
```

درگاه زیبال برای [پرباد](https://www.nuget.org/packages/Parbad/)، کتابخانه پرداخت آنلاین ‎.NET.

## پیکربندی

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddZibal()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.Merchant = "<Your merchant ID>";
                       account.IsSandbox = false; // true: Zibal's sandbox
                   });
               });
       })
       // ...
```

## درخواست پرداخت

```csharp
var result = await onlinePayment.RequestAsync(invoice =>
{
    invoice
        .SetAmount(amount)
        .SetCallbackUrl(callbackUrl)
        .UseAutoIncrementTrackingNumber()
        .SetZibalData(new ZibalRequestAdditionalData
        {
            // Optional: description, mobile number, allowed cards, ...
        })
        .UseZibal();
});
```
