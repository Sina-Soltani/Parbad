---
title: آسان پرداخت
source_hash: 1b5c03b8db07
---

# آسان پرداخت

درگاه آسان پرداخت در پکیج [Parbad](https://www.nuget.org/packages/Parbad/) قرار دارد.

## پیکربندی

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

## درخواست پرداخت

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
