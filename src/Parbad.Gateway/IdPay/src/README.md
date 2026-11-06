# Parbad.Gateway.IdPay

The IDPay gateway for [Parbad](https://www.nuget.org/packages/Parbad/), the online payment library for .NET.

## Configuration

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddIdPay()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.Api = "<Your API key>";
                       account.IsTestAccount = false; // true: IDPay's sandbox
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
        .SetIdPayData(new IdPayRequestAdditionalData
        {
            // Optional: the payer's name, phone, email and a description
        })
        .UseIdPay();
});
```

## More information

* [GitHub](https://github.com/Sina-Soltani/Parbad)
* [Documentation](https://github.com/Sina-Soltani/Parbad/wiki)
