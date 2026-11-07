---
title: پاسارگاد
source_hash: 1f08bcc22843
---

# پاسارگاد

درگاه پاسارگاد در پکیج [Parbad](https://www.nuget.org/packages/Parbad/) قرار دارد.

## پیکربندی

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddPasargad()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.TerminalNumber = "<Your terminal number>";
                       account.Username = "<Your username>";
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
        .SetPasargadData(new PasargadRequestAdditionalData
        {
            // Optional: description, payer's name, email, mobile number, national code, card numbers
        })
        .UsePasargad();
});
```
