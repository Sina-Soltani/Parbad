# Parbad.Gateway.FanAva

The FanAva gateway for [Parbad](https://www.nuget.org/packages/Parbad/), the online payment library for .NET.

## Configuration

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddFanAva()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.UserId = "<Your user ID>";
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
        .SetFanAvaAdditionalData(new FanAvaGatewayAdditionalDataRequest
        {
            // Optional: mobile number, email, apportionment accounts, ...
        })
        .UseFanAva();
});
```

## More information

* [GitHub](https://github.com/Sina-Soltani/Parbad)
* [Documentation](https://github.com/Sina-Soltani/Parbad/wiki)
