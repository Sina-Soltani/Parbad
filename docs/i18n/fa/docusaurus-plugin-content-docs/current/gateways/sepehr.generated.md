---
title: سپهر
slug: /gateways/sepehr
source_hash: cb20aac44abd
---

# سپهر

```bash
dotnet add package Parbad.Gateway.Sepehr
```

درگاه سپهر برای [پرباد](https://www.nuget.org/packages/Parbad/)، کتابخانه پرداخت آنلاین ‎.NET.

## پیکربندی

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddSepehr()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.TerminalId = 12345678;
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
        .UseSepehr();
});
```

## نتیجه تأیید

`TransactionCode` یک تأیید موفق همان RRN است؛ شماره مرجعی که سپهر به مشتری نشان می‌دهد. `verifyResult.GetSepehrAdditionalData()` سایر اطلاعات تأیید سپهر را برمی‌گرداند.
