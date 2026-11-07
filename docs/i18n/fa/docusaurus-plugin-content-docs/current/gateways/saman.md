---
title: سامان
source_hash: ec08144e3858
---

# سامان

درگاه سامان در پکیج [Parbad](https://www.nuget.org/packages/Parbad/) قرار دارد.

## پیکربندی

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

## درخواست پرداخت

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

## نتیجه‌های دریافت و تأیید

`GetSamanAdditionalData()` روی نتیجه دریافت یا تأیید، اطلاعاتی را که سامان فرستاده است برمی‌گرداند: RRN، شماره مرجع، شماره ردیابی، شماره کارت پوشانده‌شده و غیره.
