# Parbad.Gateway.PayIr

The Pay.ir gateway for [Parbad](https://www.nuget.org/packages/Parbad/), the online payment library for .NET.

## Configuration

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddPayIr()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.Api = "<Your API key>";
                       account.IsTestAccount = false; // true: Pay.ir's test API
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
        .SetPayIrAdditionalData(new PayIrRequestAdditionalData
        {
            // Optional: mobile, factor number, description, valid card number, national code
        })
        .UsePayIr();
});
```

## Verification result

`verifyResult.GetPayIrAdditionalData()` returns the additional data of Pay.ir's verification.

## More information

* [GitHub](https://github.com/Sina-Soltani/Parbad)
* [Documentation](https://github.com/Sina-Soltani/Parbad/wiki)
