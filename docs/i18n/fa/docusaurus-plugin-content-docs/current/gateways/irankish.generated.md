---
title: ایران کیش
slug: /gateways/irankish
source_hash: bd6a442a35c0
---

# ایران کیش

```bash
dotnet add package Parbad.Gateway.IranKish
```

درگاه ایران کیش برای [پرباد](https://www.nuget.org/packages/Parbad/)، کتابخانه پرداخت آنلاین ‎.NET.

## پیکربندی

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddIranKish()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.TerminalId = "<Your terminal ID>";
                       account.AcceptorId = "<Your acceptor ID>";
                       account.PassPhrase = "<Your pass phrase>";
                       account.PublicKey = "<The RSA public key that Iran Kish gave you, in PEM format>";
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
        .SetIranKishAdditionalData(new IranKishGatewayAdditionalData
        {
            MobileNumberOrEmail = "09121234567" // Optional
        })
        .UseIranKish();
});
```
