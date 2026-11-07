---
sidebar_position: 5
title: Virtual Gateway
---

# Virtual Gateway

The Virtual Gateway simulates a bank, so you can develop and test the payment of your application without a real
gateway account.

![Virtual Gateway payment page](https://github.com/user-attachments/assets/c028b0ba-a316-4b94-ba7d-2fd919ee41d2)

## Configuration

Add it to the gateways with the path where it is shown:

```csharp
.ConfigureGateways(gateways =>
{
    gateways
        .AddParbadVirtual()
        .WithOptions(options => options.GatewayPath = "/MyVirtualGateway");
})
```

The path must start with `/`.

## Enabling it

### ASP.NET Core

```csharp
app.UseParbadVirtualGatewayWhenDeveloping();
```

It is added only in the Development environment. `UseParbadVirtualGateway()` adds it always, and
`UseParbadVirtualGatewayWhen(...)` under your own condition.

### ASP.NET MVC 5 and WebForms (OWIN)

```csharp
var parbad = ParbadBuilder.CreateDefaultBuilder()
                          // configuration
                          .Build();

app.UseParbadVirtualGateway(parbad.Services);
```

## Usage

```csharp
var result = await _onlinePayment.RequestAsync(invoice =>
{
    invoice
        .SetAmount(2000)
        .SetCallbackUrl(callbackUrl)
        .UseAutoIncrementTrackingNumber()
        .UseParbadVirtual();
});

if (result.IsSucceed)
{
    return result.GatewayTransporter.TransportToGateway();
}
```
