# Parbad.Owin

OWIN and classic ASP.NET (System.Web) integration for [Parbad](https://www.nuget.org/packages/Parbad/), the online
payment library for .NET. For ASP.NET MVC 5, use [Parbad.Mvc5](https://www.nuget.org/packages/Parbad.Mvc5/), which
includes this package.

## Configuration

Build Parbad once when the application starts, for example in `Application_Start` or the OWIN `Startup` class:

```csharp
var parbad = ParbadBuilder.CreateDefaultBuilder()
                          .ConfigureGateways(gateways => { /* your gateways */ })
                          .ConfigureHttpContext(httpContext => httpContext.UseOwinFromCurrentHttpContext())
                          .ConfigureStorage(storage => { /* your storage */ })
                          .Build();
```

Without dependency injection, create one scope per web request and dispose it at the end of the request:

```csharp
using (var scope = StaticOnlinePayment.CreateScope())
{
    var result = await scope.OnlinePayment.RequestAsync(invoice => { /* the invoice */ });
}
```

## Virtual Gateway

The Parbad Virtual Gateway simulates a bank while you develop. Add it to the gateways with a path
(`gateways.AddParbadVirtual().WithOptions(options => options.GatewayPath = "/MyVirtualGateway")`) and add its OWIN
middleware:

```csharp
app.UseParbadVirtualGateway(parbad.Services);
```

## More information

* [Configuration](https://github.com/Sina-Soltani/Parbad/wiki/Configuration#aspnet-mvc--aspnet-webforms)
* [GitHub](https://github.com/Sina-Soltani/Parbad)
* [Documentation](https://github.com/Sina-Soltani/Parbad/wiki)
