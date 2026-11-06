# Parbad

Parbad is a free and open-source online payment library for .NET. It connects web applications to Iranian online
payment gateways: it requests a payment, sends the customer to the bank, verifies the payment when the customer comes
back, and can refund it. The Virtual Gateway simulates a bank, so an application can be developed and tested without a
real gateway account.

## Gateways

This package contains these gateways:

* Mellat
* Melli
* Saman
* Pasargad
* Parsian
* Asan Pardakht
* Parbad Virtual Gateway (for development and testing)

Other gateways have their own packages:
[IranKish](https://www.nuget.org/packages/Parbad.Gateway.IranKish/),
[Sepehr](https://www.nuget.org/packages/Parbad.Gateway.Sepehr/),
[ZarinPal](https://www.nuget.org/packages/Parbad.Gateway.ZarinPal/),
[Pay.ir](https://www.nuget.org/packages/Parbad.Gateway.PayIr/),
[IDPay](https://www.nuget.org/packages/Parbad.Gateway.IdPay/),
[YekPay](https://www.nuget.org/packages/Parbad.Gateway.YekPay/),
[PayPing](https://www.nuget.org/packages/Parbad.Gateway.PayPing/),
[Zibal](https://www.nuget.org/packages/Parbad.Gateway.Zibal/) and
[FanAva](https://www.nuget.org/packages/Parbad.Gateway.FanAva/).

## Other packages

* Web frameworks: [Parbad.AspNetCore](https://www.nuget.org/packages/Parbad.AspNetCore/) for ASP.NET Core;
  [Parbad.Mvc5](https://www.nuget.org/packages/Parbad.Mvc5/) and [Parbad.Owin](https://www.nuget.org/packages/Parbad.Owin/)
  for ASP.NET MVC 5 and WebForms.
* Storage of payments: [Parbad.Storage.EntityFrameworkCore](https://www.nuget.org/packages/Parbad.Storage.EntityFrameworkCore/)
  and [Parbad.Storage.Cache](https://www.nuget.org/packages/Parbad.Storage.Cache/), or your own implementation of
  [Parbad.Storage.Abstractions](https://www.nuget.org/packages/Parbad.Storage.Abstractions/).

## Configuration (ASP.NET Core)

With [Parbad.AspNetCore](https://www.nuget.org/packages/Parbad.AspNetCore/) and
[Parbad.Storage.EntityFrameworkCore](https://www.nuget.org/packages/Parbad.Storage.EntityFrameworkCore/):

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
                       account.TerminalId = 123;
                       account.UserName = "<Your username>";
                       account.UserPassword = "<Your password>";
                   });
               });

           gateways
               .AddParbadVirtual()
               .WithOptions(options => options.GatewayPath = "/MyVirtualGateway");
       })
       .ConfigureHttpContext(httpContext => httpContext.UseDefaultAspNetCore())
       .ConfigureStorage(storage => storage.UseEfCore<ShopDbContext>());
```

## Payment

Inject `IOnlinePayment`. Request a payment and send the customer to the bank:

```csharp
var result = await onlinePayment.RequestAsync(invoice =>
{
    invoice
        .SetAmount(250_000)
        .SetCallbackUrl("https://shop.example/payment/verify")
        .UseAutoIncrementTrackingNumber()
        .UseMellat();
});

if (result.IsSucceed)
{
    // Store result.TrackingNumber with the order.
    return result.GatewayTransporter.TransportToGateway(); // using Parbad.AspNetCore;
}
```

Parbad never converts amounts: set the amount in the unit (Rial or Toman) that the gateway expects.

When the bank sends the customer back to the callback URL, fetch the invoice, check the order, and verify the payment:

```csharp
var invoice = await onlinePayment.FetchAsync();

if (invoice.Status == PaymentFetchResultStatus.ReadyForVerifying)
{
    // Check the order (stock, price, ...) before the money is taken, then:
    var verifyResult = await onlinePayment.VerifyAsync(invoice);

    if (verifyResult.IsSucceed)
    {
        // The payment is complete. verifyResult.TransactionCode is the bank's code of the transaction.
    }
}
```

## More information

* [GitHub](https://github.com/Sina-Soltani/Parbad)
* [Documentation](https://github.com/Sina-Soltani/Parbad/wiki)
* [Samples](https://github.com/Sina-Soltani/Parbad.Samples)
