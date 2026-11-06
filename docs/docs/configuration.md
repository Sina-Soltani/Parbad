---
sidebar_position: 3
title: Configuration
---

# Configuration

## Registering Parbad

### With Microsoft dependency injection (ASP.NET Core)

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways => { /* ... */ })
       .ConfigureHttpContext(httpContext => httpContext.UseDefaultAspNetCore())
       .ConfigureStorage(storage => { /* ... */ });
```

Then inject `IOnlinePayment` wherever you need it.

### With another container (such as Autofac)

```csharp
var parbad = ParbadBuilder.CreateDefaultBuilder()
                          .ConfigureGateways(gateways => { /* ... */ })
                          .ConfigureHttpContext(httpContext => httpContext.UseOwinFromCurrentHttpContext())
                          .ConfigureStorage(storage => { /* ... */ });

containerBuilder.Populate(parbad.Services);
```

### Without dependency injection

Build Parbad once when the application starts:

```csharp
ParbadBuilder.CreateDefaultBuilder()
             .ConfigureGateways(gateways => { /* ... */ })
             .ConfigureHttpContext(httpContext => httpContext.UseOwinFromCurrentHttpContext())
             .ConfigureStorage(storage => { /* ... */ })
             .Build();
```

Then create one scope per web request and dispose it at the end of the request:

```csharp
using (var scope = StaticOnlinePayment.CreateScope())
{
    var result = await scope.OnlinePayment.RequestAsync(invoice => { /* ... */ });
}
```

## Gateway accounts

### One account

```csharp
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
})
```

### Several accounts for one gateway

Give each account a name, and choose it per invoice with `UseAccount("...")`:

```csharp
gateways
    .AddMellat()
    .WithAccounts(accounts =>
    {
        accounts.AddInMemory(account =>
        {
            account.Name = "Shop";
            // ...
        });

        accounts.AddInMemory(account =>
        {
            account.Name = "Donations";
            // ...
        });
    });
```

### Accounts from a database or a service

Implement `IGatewayAccountSource<TAccount>`. Parbad asks the source for the accounts each time it processes a payment,
so changes take effect immediately. The source can inject your own services:

```csharp
public class MellatAccounts : IGatewayAccountSource<MellatGatewayAccount>
{
    private readonly ISettingsService _settings;

    public MellatAccounts(ISettingsService settings) => _settings = settings;

    public async Task AddAccountsAsync(IGatewayAccountCollection<MellatGatewayAccount> accounts)
    {
        var settings = await _settings.GetMellatSettingsAsync();

        accounts.Add(new MellatGatewayAccount
        {
            TerminalId = settings.TerminalId,
            UserName = settings.UserName,
            UserPassword = settings.UserPassword
        });
    }
}
```

```csharp
gateways
    .AddMellat()
    .WithAccounts(accounts => accounts.Add<MellatAccounts>(ServiceLifetime.Transient));
```

## Web application integration

| Application | Package | Configuration |
|---|---|---|
| ASP.NET Core | `Parbad.AspNetCore` | `.ConfigureHttpContext(builder => builder.UseDefaultAspNetCore())` |
| ASP.NET MVC 5 | `Parbad.Mvc5` | `.ConfigureHttpContext(builder => builder.UseOwinFromCurrentHttpContext())` |
| ASP.NET WebForms | `Parbad.Owin` | `.ConfigureHttpContext(builder => builder.UseOwinFromCurrentHttpContext())` |

## Options

```csharp
.ConfigureOptions(options =>
{
    // The messages of the results.
    options.Messages.PaymentSucceed = "Thank you for your payment.";

    // Parbad logs with ILogger; this turns it off.
    options.EnableLogging = false;

    // The timeout of every call to a gateway (default: 30 seconds).
    options.GatewayTimeout = TimeSpan.FromSeconds(20);

    // With a Content-Security-Policy nonce: the nonce of the current request, used by the pages that send the
    // customer to the bank and by the Virtual Gateway.
    options.NonceFactory = httpContext => (string)httpContext.Items["csp-nonce"]!;
})
```

### Tracking numbers

`UseAutoIncrementTrackingNumber()` and `UseAutoRandomTrackingNumber()` give each invoice a tracking number. Their
options:

```csharp
.ConfigureAutoIncrementTrackingNumber(options =>
{
    options.MinimumValue = 1000;
    options.Increment = 1;
})
.ConfigureAutoRandomTrackingNumber(options => options.MinimumValue = 1000)
```
