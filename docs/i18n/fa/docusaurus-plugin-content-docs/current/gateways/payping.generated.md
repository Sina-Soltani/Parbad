---
title: پی‌پینگ
slug: /gateways/payping
source_hash: 8659b81fa0e5
---

# پی‌پینگ

```bash
dotnet add package Parbad.Gateway.PayPing
```

درگاه پی‌پینگ برای [پرباد](https://www.nuget.org/packages/Parbad/)، کتابخانه پرداخت آنلاین ‎.NET.

پی‌پینگ مبلغ را به تومان می‌گیرد. پرباد هیچ‌وقت مبلغ را تبدیل نمی‌کند؛ پس مبلغ فاکتور را به تومان وارد کنید.

## پیکربندی

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddPayPing()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.AccessToken = "<Your access token>";
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
        .SetPayPingData(payPing =>
        {
            // Optional: the payer's name, mobile and a description
        })
        .UsePayPing();
});
```
