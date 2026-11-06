---
title: Pasargad
---

# Pasargad

The Pasargad gateway is included in the [Parbad](https://www.nuget.org/packages/Parbad/) package.

## Configuration

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

## Payment request

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
