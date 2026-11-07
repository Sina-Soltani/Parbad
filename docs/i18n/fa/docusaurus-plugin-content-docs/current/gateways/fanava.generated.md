---
title: فن‌آوا
slug: /gateways/fanava
source_hash: b6c22a819af9
---

# فن‌آوا

```bash
dotnet add package Parbad.Gateway.FanAva
```

درگاه فن‌آوا برای [پرباد](https://www.nuget.org/packages/Parbad/)، کتابخانه پرداخت آنلاین ‎.NET.

## پیکربندی

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddFanAva()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.UserId = "<Your user ID>";
                       account.Password = "<Your password>";
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
        .SetFanAvaAdditionalData(new FanAvaGatewayAdditionalDataRequest
        {
            // Optional: mobile number, email, apportionment accounts, ...
        })
        .UseFanAva();
});
```
