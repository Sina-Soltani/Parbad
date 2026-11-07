---
title: Pay.ir
slug: /gateways/payir
source_hash: a5674f4bfd70
---

# Pay.ir

```bash
dotnet add package Parbad.Gateway.PayIr
```

درگاه Pay.ir برای [پرباد](https://www.nuget.org/packages/Parbad/)، کتابخانه پرداخت آنلاین ‎.NET.

## پیکربندی

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddPayIr()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.Api = "<Your API key>";
                       account.IsTestAccount = false; // true: Pay.ir's test API
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
        .SetPayIrAdditionalData(new PayIrRequestAdditionalData
        {
            // Optional: mobile, factor number, description, valid card number, national code
        })
        .UsePayIr();
});
```

## نتیجه تأیید

`verifyResult.GetPayIrAdditionalData()` اطلاعات تکمیلی تأیید Pay.ir را برمی‌گرداند.
