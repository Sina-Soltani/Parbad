---
title: پارسیان
source_hash: 938170cc9538
---

# پارسیان

درگاه پارسیان در پکیج [Parbad](https://www.nuget.org/packages/Parbad/) قرار دارد.

## پیکربندی

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddParsian()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.LoginAccount = "<Your login account>";
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
        .SetParsianAdditionalData(new ParsianGatewayAdditionalDataRequest
        {
            // Optional: additional data, originator
        })
        .UseParsian();
});
```
