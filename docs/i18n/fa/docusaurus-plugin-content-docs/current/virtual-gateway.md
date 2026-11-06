---
sidebar_position: 5
title: درگاه مجازی
---

# درگاه مجازی

درگاه مجازی یک بانک را شبیه‌سازی می‌کند تا بتوانید پرداخت برنامه‌تان را بدون حساب واقعی درگاه توسعه دهید و تست کنید.

![صفحه پرداخت درگاه مجازی](https://github.com/user-attachments/assets/c028b0ba-a316-4b94-ba7d-2fd919ee41d2)

## پیکربندی

درگاه مجازی را با مسیری که در آن نمایش داده می‌شود به درگاه‌ها اضافه کنید:

```csharp
.ConfigureGateways(gateways =>
{
    gateways
        .AddParbadVirtual()
        .WithOptions(options => options.GatewayPath = "/MyVirtualGateway");
})
```

مسیر باید با `/` شروع شود.

## فعال‌کردن

### ASP.NET Core

```csharp
app.UseParbadVirtualGatewayWhenDeveloping();
```

این متد درگاه مجازی را فقط در محیط Development اضافه می‌کند. `UseParbadVirtualGateway()` آن را همیشه و
`UseParbadVirtualGatewayWhen(...)` با شرط دلخواه شما اضافه می‌کند.

### ASP.NET MVC 5 و WebForms (OWIN)

```csharp
var parbad = ParbadBuilder.CreateDefaultBuilder()
                          // configuration
                          .Build();

app.UseParbadVirtualGateway(parbad.Services);
```

## استفاده

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
