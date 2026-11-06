---
title: "Configuration"
slug: /configuration
---

# Configuration

1. [General Configuration](#general-configuration)
   * [Using Microsoft Dependency Injection (such as in ASP.NET CORE)](#using-microsoft-dependency-injection-such-as-in-aspnet-core)
   * [Using a Dependency Injection library (such as Autofac)](#using-a-dependency-injection-library-such-as-autofac)
   * [Using no Dependency Injection](#using-no-dependency-injection)
2. [Gateways](#gateways)
   * [Adding an account](#adding-an-account)
   * [Adding the accounts dynamically using a custom source (such as Database)](#adding-the-accounts-dynamically-using-a-custom-source)
   * [Adding multiple accounts for a Gateway](#adding-multiple-accounts-for-a-gateway)
3. [Web Application Integration](#web-application-integration)
   * [ASP.NET CORE](#aspnet-core)
   * [ASP.NET MVC & ASP.NET WebForms](#aspnet-mvc--aspnet-webforms)
4. [Storage](#storage)
   * [Why Storage is needed?](#why-storage-is-needed)
   * [Using Cached Storages (Memory Cache, Distributed Cache)](#using-cached-storage)
   * [Using EntityFramework Core](#using-entityframework-core)
     * [Sample](https://github.com/Sina-Soltani/Parbad.Samples/tree/master/Storage/Parbad.Sample.EntityFrameworkCore)
   * [Using Custom Storage](#using-a-custom-storage)
     * [Sample](https://github.com/Sina-Soltani/Parbad.Samples/tree/master/Storage/CustomStorage)
5. [Options](#options)
   * [Messages](#messages)
   * [Enabling/Disabling the Logger](#enablingdisabling-the-logger)
   * [Nonce - Content-Security-Policy](#nonce---content-security-policy)
   * [Auto Tracking Number](#auto-tracking-number)

## General Configuration

#### Using Microsoft Dependency Injection (such as in ASP.NET CORE)
**Startup.cs** file

```csharp
public void ConfigureServices(IServiceCollection services)
{
   services.AddControllers();

   services.AddParbad()
         //.configuration...
         //.configuration...
         //.configuration...
}
```
So you can inject the **IOnlinePayment** interface wherever you need.

#### Using a Dependency Injection library (such as Autofac)
**Startup.cs** file

```csharp
public void Configuration(IAppBuilder app)
{
   var parbadBuilder = ParbadBuilder.CreateDefaultBuilder()
                                  //.configuration...
                                  //.configuration...
                                  //.configuration...

   // Register Parbad using Autofac or any other DI libraries.
   containerBuilder.Populate(parbadBuilder.Services);
}
```
So you can inject the **IOnlinePayment** interface wherever you need.

#### Using no Dependency Injection
**Startup.cs** file

```csharp
public void Configuration(IAppBuilder app)
{
   var onlinePaymentAccessor = ParbadBuilder.CreateDefaultBuilder()
                                  //.configuration...
                                  //.configuration...
                                  //.configuration...
                                    .Build();
}
```
The `onlinePaymentAccessor` object is an instance of `IOnlinePaymentAccessor` which gives you the ability to work with the `IOnlinePayment` interface. You can now use the StaticOnlinePayment class wherever you need. For example in projects that don't have any Dependency Injection.

Note: It's important to call the **Build** method for using the static **StaticOnlinePayment** class.

## Gateways

#### Adding an account
Add the gateway you want with its accounts as follows:

Example: Adding Mellat gateway with an account
```csharp
.ConfigureGateways(gateways =>
{
   gateways
        .AddMellat()
        .WithAccounts(source =>
        {
           source.AddInMemory(account =>
           {
              account.TerminalId = 123;
              account.UserName = "MyId";
              account.UserPassword = "MyPassword";
           });
        });
})
```
The `AddInMemory` method saves the gateway's data in your server's RAM.

#### Adding the accounts dynamically using a custom source
For adding the accounts using a source such as database or an external service, you need to define a custom source and then add the accounts inside the source.

**Note:** After reading/adding the gateway accounts, they will not be saved anywhere and every time a payment is being requested or verified, Parbad will ask your `Custom Source` to read/add the accounts again and again. This gives you the ability to use specific accounts each time a payment must be processed.

This is an example of adding multiple gateways accounts. You can define a source by implementing the `IGatewayAccountSource` interface like so:

```csharp
public class GatewaysAccounts : 
   IGatewayAccountSource<MellatGatewayAccount>,
   IGatewayAccountSource<ParsianGatewayAccount>,
   IGatewayAccountSource<SamanGatewayAccount>
{
    private readonly IYourSettingsService _settingsService;

    public GatewaysAccounts(IYourSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task AddAccountsAsync(IGatewayAccountCollection<MellatGatewayAccount> accounts)
    {
        var settings = await _settingsService.GetSettingsAsync();

        accounts.Add(new MellatGatewayAccount
        {
           TerminalId = settings.TerminalId,
           UserName = settings.UserName,
           UserPassword = settings.UserPassword
        });
    }

    public async Task AddAccountsAsync(IGatewayAccountCollection<ParsianGatewayAccount> accounts)
    {
        var settings = await _settingsService.GetSettingsAsync();

        accounts.Add(new ParsianGatewayAccount
        {
           LoginAccount = settings.LoginAccount
        });
    }

    public async Task AddAccountsAsync(IGatewayAccountCollection<SamanGatewayAccount> accounts)
    {
        var settings = await _settingsService.GetSettingsAsync();

        accounts.Add(new SamanGatewayAccount
        {
           MerchantId = settings.TerminalId,
           Password = settings.Password
        });
    }
}
```
Note: As you see, you can also inject the services that you need inside your source. The injected services (IYourSettingsService in the above example) must be registered using your DI library before. Otherwise Parbad cannot create the source.

After that, all you need to do is telling Parbad about your custom source as follows:
```csharp
.ConfigureGateways(gateways =>
{
   gateways
        .AddMellat()
        .WithAccounts(source => source.Add<GatewaysAccounts>(ServiceLifetime.Transient));

   // other gateways...
})
```

#### Adding multiple accounts for a Gateway
```csharp
.ConfigureGateways(gateways =>
{
   gateways
        .AddMellat()
        .WithAccounts(source =>
        {
           source.AddInMemory(account =>
           {
              account.Name = "Account 1";
              // other settings
           });

           source.AddInMemory(account =>
           {
              account.Name = "Account 2";
              // other settings
           });
        });
})
```
See also: [Requesting a payment with a specific account](./invoice-builder.md#requesting-a-payment-with-a-specific-account)

## Web Application Integration
Depend on your web application choose one of the integration packages to make your app compatible with Parbad.

#### [ASP.NET CORE](./installation.md#aspnet-core)
```csharp
.ConfigureHttpContext(builder => builder.UseDefaultAspNetCore())
```

#### [ASP.NET MVC & ASP.NET WebForms](./installation.md#aspnet-mvc)
```csharp
.ConfigureHttpContext(builder => builder.UseOwinFromCurrentHttpContext())
```
Note: The method _builder.UseOwin_ also has other overloads that you can use.

See also the [Installation](./installation.md) page.

## Storage

Storage is a place where Parbad saves and loads the payment information to process them internally, so you don't need to care about the internal payment processes.

#### Why Storage is needed?
* Auto-Prevent from double or multiple processing of a payment
* Auto State Handling (States are: Requested, Ready For Payment, Ready For Verifying, Paid, Refunded, Failed)
* Auto Tracking Number Management
* No need to prepare and give any data when `Fetching` or `Verifying`.

**Note:**
Parbad database is **ONLY FOR INTERNAL OPERATIONS** like saving and loading the payment information.
You will get the important information such as Tracking Number, Amount and Transaction Code when you request a payment or verify a requested payment using the **Result** objects.

You can configure the storage by:
* Using the pre-defined storages.
    * [Memory Cache and Distributed Cache](./configuration.md#using-cached-storage)
    * [EntityFramework Core](./configuration.md#using-entityframework-core)
* [Using a custom storage](./configuration.md#using-a-custom-storage)

#### Using Cached Storage

Nuget Package Manager:
```
Install-Package Parbad.Storage.Cache
```
.NET CLI:
```
dotnet add package Parbad.Storage.Cache
```

Memory Cache usage:
```csharp
.ConfigureStorage(builder => builder.UseMemoryCache());
```
With this setting, Parbad will use the `IMemoryCache` interface to save/load the payment data.

**Note**: `MemoryCache` is a suitable Storage for testing and developing your applications. But it's not stable and you shouldn't use it for production.

Distributed Cache usage:
```csharp
.ConfigureStorage(builder => builder.UseDistributedCache());
```
With this setting, Parbad will use the `IDistributedCache` interface to save/load the payment data.

Note: Make sure that you already added a distributed cache provider such as [Redis](https://www.nuget.org/packages/Microsoft.Extensions.Caching.StackExchangeRedis/6.0.0-rc.1.21452.15) or [SQL Server](https://www.nuget.org/packages/Microsoft.Extensions.Caching.SqlServer/6.0.0-rc.1.21452.15).

#### Using EntityFramework Core
[Sample](https://github.com/Sina-Soltani/Parbad.Samples/tree/master/Storage/Parbad.Sample.EntityFrameworkCore)

By using the EntityFrameworkCore Storage you have the ability to save Parbad information inside your own database.
You can also use many database providers such as SQL Server, Sqlite, MySql and so on.

**Step 1) Installation**

Nuget Package Manager:
```
Install-Package Parbad.Storage.EntityFrameworkCore
```
.NET CLI:
```
dotnet add package Parbad.Storage.EntityFrameworkCore
```

**Step 2) Configuration:**
```csharp
.ConfigureStorage(builder =>
{
   builder.UseEfCore(options => 
   {
      // Example 1: Using SQL Server
      var assemblyName = typeof(Startup).Assembly.GetName().Name;
      options.ConfigureDbContext = db => db.UseSqlServer("Your Connection String", sql => sql.MigrationsAssembly(assemblyName));

      // Example 2: If you prefer to have a separate MigrationHistory table for Parbad, you can change the above line to this:
      options.ConfigureDbContext = db => db.UseSqlServer("Your Connection String", sql =>
      {
         sql.MigrationsAssembly(assemblyName);
         sql.MigrationsHistoryTable("TABLE NAME");
      });

      options.DefaultSchema = "SCHEMA NAME"; // optional

      options.PaymentTableOptions.Name = "TABLE NAME"; // optional
      options.PaymentTableOptions.Schema = "SCHEMA NAME"; // optional

      options.TransactionTableOptions.Name = "TABLE NAME"; // optional
      options.TransactionTableOptions.Schema = "SCHEMA NAME"; // optional
   })
})
```

**Step 3) Adding the Migrations:**

Open the Command Prompt (CMD) in your project's folder and run the following command:
```
dotnet ef migrations add <MigrationName> -c ParbadDataContext
```

It will automatically add the Parbad Migrations in your project.

If you prefer to have a separate folder for Parbad Migrations you can add an output parameter to the above command like so:
```
dotnet ef migrations add <MigrationName> -c ParbadDataContext -o Parbad/Migrations
```

**Step 4) Applying the Migrations on the database:**

```
dotnet ef database update -c ParbadDataContext
```

#### Using a custom Storage

[Sample](https://github.com/Sina-Soltani/Parbad.Samples/tree/master/Storage/CustomStorage)

If for any reason you want to use your specific storage then you can implement the _IStorage_ interface as follows:

Note: If you want your Custom Storage to be in a class library, you just need to install the [Parbad.Storage.Abstractions](https://www.nuget.org/packages/Parbad.Storage.Abstractions/) package in your class library project.

```csharp
public class MyStorage : IStorage
{
   // implementations here...
}
```
Note: You can also inject any service inside your storage if you need. The injected services must be registered with your dependency injection.

Now just add it like so:
```csharp
.ConfigureStorage(builder => builder.AddStorage(new MyStorage()));

// or

.ConfigureStorage(builder => builder.AddStorage<MyStorage>([SERVICE LIFETIME]));

// or

.ConfigureStorage(builder => builder.AddStorage<MyStorage>(provider => new MyStorage(), [SERVICE LIFETIME]));
```

### Data Model

#### Payment
Property name | .Net type | SQL type
------------ | ------------- | -------------
Id (Primary Key) | long | bigint
TrackingNumber | long | bigint
Amount | decimal | decimal(18,2)
Token | string | nvarchar(max)
TransactionCode | string | nvarchar(max)
GatewayName | string | nvarchar(max)
GatewayAccountName | string | nvarchar(max)
IsCompleted | bool | bit
IsPaid | bool | bit

#### Transaction
Property name | .Net type | SQL type
------------ | ------------- | -------------
Id (Primary Key) | long | bigint
Amount | decimal | decimal(18,2)
Type | TransactionType | tinyint
IsSucceed | bool | bit
Message | string | nvarchar(max)
AdditionalData | string | nvarchar(max)
PaymentId (Foreign Key) | long | bigint

## Options

#### Messages

```c#
.ConfigureOptions(options =>
{
   options.Messages.PaymentSucceed = "YOUR MESSAGE";
   // other messages...
});
```

#### Enabling/Disabling the Logger
Parbad uses the [ILogger](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/) interface to log the operations.
```c#
.ConfigureOptions(options =>
{
   options.EnableLogging = true | false;
});
```

#### Nonce - Content Security Policy
In case you have enabled Nonce as Content-Security-Policy in your web application, you must then also tell Parbad how to get Nonce, so that Parbad can use it when redirecting clients to gateways, and also when showing the Virtual Gateway.

To do this, you can use `NonceFactory` in `ParbadOptions` class like so:
```c#
.ConfigureOptions(options =>
{
   options.NonceFactory = httpContext => <your implementation goes here>
})
```

#### Auto Tracking Number
Auto Increment Tracking Number Options
```
.ConfigureAutoIncrementTrackingNumber(options => 
{
  options.MinimumValue = 1000;
  options.Increment = 1;
});
```

Auto Random Tracking Number Options
```
.ConfigureAutoRandomTrackingNumber(options => 
{
  options.MinimumValue = 1000;
});
```