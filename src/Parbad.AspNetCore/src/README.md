# Parbad.AspNetCore

ASP.NET Core integration for [Parbad](https://www.nuget.org/packages/Parbad/), the online payment library for .NET.

## Configuration

Parbad reads the bank's callback from the current request:

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways => { /* your gateways */ })
       .ConfigureHttpContext(httpContext => httpContext.UseDefaultAspNetCore())
       .ConfigureStorage(storage => { /* your storage */ });
```

## Sending the customer to the bank

`TransportToGateway()` (namespace `Parbad.AspNetCore`) turns the result of a payment request into an action result: a
redirect, or a form that is posted to the bank.

```csharp
using Parbad.AspNetCore;

var result = await onlinePayment.RequestAsync(invoice => { /* the invoice */ });

if (result.IsSucceed)
{
    return result.GatewayTransporter.TransportToGateway();
}
```

## Virtual Gateway

The Parbad Virtual Gateway simulates a bank while you develop. Add it to the gateways with a path
(`gateways.AddParbadVirtual().WithOptions(options => options.GatewayPath = "/MyVirtualGateway")`) and add its middleware:

```csharp
app.UseParbadVirtualGatewayWhenDeveloping();
```

`UseParbadVirtualGatewayWhenDeveloping` adds it only in the Development environment; `UseParbadVirtualGateway` and
`UseParbadVirtualGatewayWhen` add it always or under your own condition.

## More information

* [Configuration](https://sina-soltani.github.io/Parbad/docs/configuration)
* [GitHub](https://github.com/Sina-Soltani/Parbad)
* [Documentation](https://sina-soltani.github.io/Parbad/)
