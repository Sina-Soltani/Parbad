# Parbad.Mvc5

ASP.NET MVC 5 integration for [Parbad](https://www.nuget.org/packages/Parbad/), the online payment library for .NET.
It includes [Parbad.Owin](https://www.nuget.org/packages/Parbad.Owin/), which connects Parbad to the current request
and hosts the Virtual Gateway; see its page for the configuration.

## Sending the customer to the bank

`TransportToGateway()` (namespace `Parbad.Mvc`) turns the result of a payment request into an `ActionResult`: a
redirect, or a form that is posted to the bank.

```csharp
using Parbad.Mvc;

using (var scope = StaticOnlinePayment.CreateScope())
{
    var result = await scope.OnlinePayment.RequestAsync(invoice => { /* the invoice */ });

    if (result.IsSucceed)
    {
        return result.GatewayTransporter.TransportToGateway();
    }
}
```

## More information

* [Configuration](https://github.com/Sina-Soltani/Parbad/wiki/Configuration#aspnet-mvc--aspnet-webforms)
* [GitHub](https://github.com/Sina-Soltani/Parbad)
* [Documentation](https://github.com/Sina-Soltani/Parbad/wiki)
