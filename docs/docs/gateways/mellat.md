---
title: Mellat
---

# Mellat

The Mellat gateway is included in the [Parbad](https://www.nuget.org/packages/Parbad/) package.

## Configuration

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddMellat()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.TerminalId = 1234567;
                       account.UserName = "<Your username>";
                       account.UserPassword = "<Your password>";
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
        .SetMellatAdditionalData(new MellatGatewayAdditionalDataRequest
        {
            // Optional: additional data, payer ID, mobile number
        })
        .UseMellat();
});
```

## Cumulative payment

A cumulative payment splits the amount between the sub-services of your Mellat account:

```csharp
invoice
    .AddMellatCumulativeAccount(subServiceId: 1, amount: 150_000)
    .AddMellatCumulativeAccount(subServiceId: 2, amount: 100_000)
    .UseMellat();
```

`AddMellatCumulativeAccount` also has an overload with a payer ID, and `AddMellatCumulativeAccounts` adds several
accounts at once.
