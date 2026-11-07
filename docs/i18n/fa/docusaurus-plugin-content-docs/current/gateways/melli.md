---
title: ملی
source_hash: 8af2b284bd74
---

# ملی

درگاه ملی در پکیج [Parbad](https://www.nuget.org/packages/Parbad/) قرار دارد.

## پیکربندی

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddMelli()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.TerminalId = "<Your terminal ID>";
                       account.MerchantId = "<Your merchant ID>";
                       account.TerminalKey = "<Your terminal key>";
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
        .UseMelli();
});
```
