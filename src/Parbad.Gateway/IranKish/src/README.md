# Parbad.Gateway.IranKish

The Iran Kish gateway for [Parbad](https://www.nuget.org/packages/Parbad/), the online payment library for .NET.

## Configuration

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

## Payment request

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

## More information

* [GitHub](https://github.com/Sina-Soltani/Parbad)
* [Documentation](https://github.com/Sina-Soltani/Parbad/wiki)
