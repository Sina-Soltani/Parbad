---
title: یک‌پی
slug: /gateways/yekpay
source_hash: 79109806273f
---

# یک‌پی

```bash
dotnet add package Parbad.Gateway.YekPay
```

درگاه یک‌پی برای [پرباد](https://www.nuget.org/packages/Parbad/)، کتابخانه پرداخت آنلاین ‎.NET.

## پیکربندی

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddYekPay()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.MerchantId = "<Your merchant ID>";
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
        .SetYekPayData(yekPay =>
        {
            // Required: the currencies and the customer's details
            yekPay.FromCurrencyCode = ...;
            yekPay.ToCurrencyCode = ...;
            yekPay.FirstName = "...";
            // ...
        })
        .UseYekPay();
});
```
