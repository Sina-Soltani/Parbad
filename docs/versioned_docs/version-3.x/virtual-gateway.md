---
title: "Virtual Gateway"
slug: /virtual-gateway
---

# Virtual Gateway

The Virtual Gateway is a simple internal gateway that simulates the online payment process, which helps the application development and testing.
In another word, you don't need to have a real bank account to test your application.

![Virtual Gateway Payment](https://github.com/user-attachments/assets/c028b0ba-a316-4b94-ba7d-2fd919ee41d2)

![Virtual Gateway Result](https://github.com/user-attachments/assets/9b2f9af4-8757-4b78-8f74-d63ec3d5bf2c)


* [Configuration](./virtual-gateway.md#configuration)
* [Enabling the Virtual Gateway](./virtual-gateway.md#enabling-the-virtual-gateway)
* * [ASP.NET CORE](./virtual-gateway.md#aspnet-core)
* * [ASP.NET MVC](./virtual-gateway.md#aspnet-mvc)
* * [ASP.NET WebForms](./virtual-gateway.md#aspnet-webforms)
* [Usage](./virtual-gateway.md#usage)

### Configuration
To use the Virtual Gateway, you need to add it to the list of gateways and configure it.

```csharp
   services.AddParbad()
           .ConfigureGateways(gateways =>
           {
              gateways
                   .AddParbadVirtual()
                   .WithOptions(options => options.GatewayPath = "/WriteAPathHere");
           })
```
**Note**: Make sure that the _GatewayPath_ starts with a "/". Example: `/MyVirtualGateway`

### Enabling the Virtual Gateway

#### ASP.NET CORE
Install the AspNetCore package:
```
Install-Package Parbad.AspNetCore
```
`Startup.cs` file:
```csharp
public void Configure(IApplicationBuilder app)
{
   app.UseMvc(routes =>
   {
      routes.MapRoute(
               name: "default",
               template: "{controller=Home}/{action=Index}/{id?}");
   });

   // Register the Virtual Gateway
   app.UseParbadVirtualGateway();
}
```

#### ASP.NET MVC
Install the MVC package:
```
Install-Package Parbad.Mvc5
```
`Startup.cs` file:

```csharp
public void Configuration(IAppBuilder app)
{
   var parbadBuilder = ParbadBuilder.CreateDefaultBuilder()
                                    // configurations

   // Register Parbad using a DI library such as Autofac.
   containerBuilder.Populate(parbadBuilder.Services);

   // Add the Parbad Virtual Gateway
   // Get IServiceProvider required by Parbad Virtual Gateway from the DI library.
   var serviceProvider = container.Resolve<IServiceProvider>();
   app.UseParbadVirtualGateway(serviceProvider);
}
```

#### ASP.NET WebForms
Install the Owin package:
```
Install-Package Parbad.Owin
```
`Startup.cs` file:

```csharp
public void Configuration(IAppBuilder app)
{
   var parbadBuilder = ParbadBuilder.CreateDefaultBuilder()
                                    // configurations
                                    .Build();

   // Add the Parbad Virtual Gateway
   app.UseParbadVirtualGateway(parbadBuilder.Services);
}
```

### Usage
```csharp
var result = await _onlinePayment.RequestAsync(invoice =>
{
   invoice
      .SetTrackingNumber(123)
      .SetAmount(2000)
      .SetCallbackUrl("http://www.mywebsite.com/foo/bar/")
      .SetGateway("ParbadVirtual")
      // OR
      .UseParbadVirtual()
});

if (result.IsSucceed)
{
   return result.GatewayTransporter.TransportToGateway();
}
```

Parbad will redirect the client to the Virtual Gateway.