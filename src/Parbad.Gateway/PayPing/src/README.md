# Parbad.Gateway.PayPing

The PayPing gateway for [Parbad](https://www.nuget.org/packages/Parbad/), the online payment library for .NET.

PayPing expects the amount in Toman. Parbad never converts amounts, so set the invoice amount in Toman.

## Configuration

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddPayPing()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.AccessToken = "<Your access token>";
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
        .SetPayPingData(payPing =>
        {
            // Optional: the payer's name, mobile and a description
        })
        .UsePayPing();
});
```

## More information

* [GitHub](https://github.com/Sina-Soltani/Parbad)
* [Documentation](https://sina-soltani.github.io/Parbad/docs/gateways/payping)
