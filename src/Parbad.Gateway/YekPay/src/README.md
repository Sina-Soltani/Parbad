# Parbad.Gateway.YekPay

The YekPay gateway for [Parbad](https://www.nuget.org/packages/Parbad/), the online payment library for .NET.

## Configuration

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddYekPay()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.MerchantId = "<Your merchant ID>";
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
        .SetYekPayData(yekPay =>
        {
            // Required: the currencies and the customer's details
            yekPay.FromCurrencyCode = ...;
            yekPay.ToCurrencyCode = ...;
            yekPay.FirstName = "...";
            // ...
        })
        .UseYekPay();
});
```

## More information

* [GitHub](https://github.com/Sina-Soltani/Parbad)
* [Documentation](https://sina-soltani.github.io/Parbad/docs/gateways/yekpay)
