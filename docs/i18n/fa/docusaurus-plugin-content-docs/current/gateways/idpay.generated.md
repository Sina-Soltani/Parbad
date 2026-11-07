---
title: آیدی‌پی
slug: /gateways/idpay
source_hash: 6a81a70b5ef1
---

# آیدی‌پی

```bash
dotnet add package Parbad.Gateway.IdPay
```

درگاه آیدی‌پی برای [پرباد](https://www.nuget.org/packages/Parbad/)، کتابخانه پرداخت آنلاین ‎.NET.

## پیکربندی

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddIdPay()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.Api = "<Your API key>";
                       account.IsTestAccount = false; // true: IDPay's sandbox
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
        .SetIdPayData(new IdPayRequestAdditionalData
        {
            // Optional: the payer's name, phone, email and a description
        })
        .UseIdPay();
});
```
