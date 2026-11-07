---
title: ملت
source_hash: 655844a26597
---

# ملت

درگاه ملت در پکیج [Parbad](https://www.nuget.org/packages/Parbad/) قرار دارد.

## پیکربندی

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

## درخواست پرداخت

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

## پرداخت تجمیعی

پرداخت تجمیعی مبلغ را بین زیرسرویس‌های حساب ملت شما تقسیم می‌کند:

```csharp
invoice
    .AddMellatCumulativeAccount(subServiceId: 1, amount: 150_000)
    .AddMellatCumulativeAccount(subServiceId: 2, amount: 100_000)
    .UseMellat();
```

`AddMellatCumulativeAccount` یک overload با شناسه پرداخت‌کننده هم دارد و `AddMellatCumulativeAccounts` چند حساب را با هم اضافه می‌کند.
