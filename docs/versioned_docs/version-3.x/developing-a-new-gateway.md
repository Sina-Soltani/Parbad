---
title: "Developing a new Gateway"
slug: /developing-a-new-gateway
---

# Developing a new Gateway

### How to create a new Gateway and publish it in Nuget.org

Do you like to develop a new gateway for Parbad and publish it in package managers like nuget.org? Then this is the right article for you.

In this article you will learn:
* [Start from the beginning](./developing-a-new-gateway.md#start-from-the-beginning)
  * [What do Request, Verify and Refund do?](#what-do-request-verify-and-refund-do)
  * [Creating the project](./developing-a-new-gateway.md#creating-the-project)
  * [Installing the Parbad package](./developing-a-new-gateway.md#installing-the-parbad-package)
  * [Creating the Gateway Account](./developing-a-new-gateway.md#creating-the-gateway-account)
  * [Creating the Gateway](./developing-a-new-gateway.md#creating-the-gateway)
  * [Creating the extension methods](./developing-a-new-gateway.md#creating-the-extension-methods)
* [Implementing the methods and using the Parbad features](./developing-a-new-gateway.md#implementing-the-methods-and-using-the-parbad-features)
  * [Getting the accounts](./developing-a-new-gateway.md#getting-the-accounts)
  * [Communicating with the banks by using HttpClient](./developing-a-new-gateway.md#communicating-with-the-banks-by-using-httpclient)
  * [Reading the requests by using HttpContext](./developing-a-new-gateway.md#reading-the-requests-by-using-httpcontext)
  * [Transporting the user to banks](./developing-a-new-gateway.md#transporting-the-user-to-banks)
  * [Implementing the Request method](./developing-a-new-gateway.md#implementing-the-request-method)
  * [Implementing the Verify method](./developing-a-new-gateway.md#implementing-the-verify-method)
  * [Implementing the Refund method](./developing-a-new-gateway.md#implementing-the-refund-method)
* [Saving data inside the Storage](./developing-a-new-gateway.md#saving-data-inside-the-storage)
* [Creating and publishing a Nuget](./developing-a-new-gateway.md#creating-and-publishing-a-nuget)
* [Samples](https://github.com/Sina-Soltani/Parbad/tree/master/src/Parbad.Gateway)

## Start from the beginning

#### What do Request, Verify and Refund do?
As their name suggests, these are 3 main methods that a gateway must have.

* **Request:** In this step, you usually send the invoice and user to the payment provider (read it bank). After that the user pays the invoice inside the payment provider website and will be transferred again to your website by payment provider. You need to know how does your payment provider accept the requests.
* **Verify:** This methods will be called when a user comes back from the payment provider website and want to proceed the payment operation. In this step, you must verify the invoice and make sure if the invoice is paid successfully or not.
* **Refund:** Not every gateway has this feature enabled. But it can be used to refund an already paid invoice.

#### Creating the project

We want to create a new gateway which called **Hero**.

First of all, let's create the project.
* Open the Visual Studio
* File -> New -> Project -> Class Library (.NET Standard 2 or later)
* Enter the project name. Suggested name pattern: **Hero.Parbad.Gateway** - This is the name of your nuget package that users will see.

#### Installing the Parbad package
Enter the bellow command inside the Package Manager Console or install it using the Visual Package Manager.
```
Install-Package Parbad
```

#### Creating the Gateway Account

We start with defining the Account class like so:
```csharp
public class HeroGatewayAccount : GatewayAccount
{
   public int MyPropertyA { get; set; }

   public int MyPropertyB { get; set; }
}
```
#### Creating the Gateway

Then define your gateway class and implement the methods:
```csharp
public class HeroGateway : GatewayBase<HeroGatewayAccount>
{
   public HeroGateway(IGatewayAccountProvider<HeroGatewayAccount> accountProvider) : base(accountProvider)
   {
   }
   public Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
   {
      // implementation...
   }

   public Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
   {
      // implementation...
   }

   public Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
   {
      // implementation...
   }
}
```
Users can use your gateway by giving the name "Hero" (without "Gateway") when they create an invoice.
Another way to set the name of your gateway is using the [Gateway] attribute like so:
```csharp
[Gateway("Hero")]
public class HeroGateway : GatewayBase<HeroGatewayAccount>
{
}
```
In this case, Parbad will find your gateway by looking the name that you specified in this attribute.

Until here is everything done with the definitions.

#### Creating the extension methods

Let's create some extension methods to make it easier for users to use our gateway.
```csharp
public static class HeroGatewayBuilderExtensions
{
   public static IGatewayConfigurationBuilder<HeroGateway> AddHero(this IGatewayBuilder builder)
   {
      return builder.AddGateway<HeroGateway>();
   }

   public static IGatewayConfigurationBuilder<HeroGateway> WithAccounts(this IGatewayConfigurationBuilder<HeroGateway> builder, Action<IGatewayAccountBuilder<HeroGatewayAccount>> configureAccounts)
   {
      return builder.WithAccounts(configureAccounts);
   }

   public static IInvoiceBuilder UseHero(this IInvoiceBuilder builder)
   {
      return builder.SetGateway("Hero");
   }
}
```

## Implementing the methods and using the Parbad features

#### Getting the accounts
Users can add their bank account information in startup of their websites. You can get the accounts like so:
```csharp
public class HeroGateway : GatewayBase<HeroGatewayAccount>
{
   public HeroGateway(IGatewayAccountProvider<HeroGatewayAccount> accountProvider) : base(accountProvider)
   {
   }

   public async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
   {
      var account = await GetAccountAsync(invoice);
   }

   public Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
   {
      var account = await GetAccountAsync(context.Payment);
   }

   public Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
   {
      var account = await GetAccountAsync(context.Payment);
   }
}
```
Parbad will give you an account for the specified invoice.


#### Communicating with the banks by using HttpClient
Parbad can give you a specific HttpClient for communicating with the bank. You only need to inject it inside the constructor of your gateway class like so:
```csharp
public class HeroGateway : GatewayBase<HeroGatewayAccount>
{
   private readonly HttpClient _httpClient;

   public HeroGateway(IGatewayAccountProvider<HeroGatewayAccount> accountProvider,
                    IHttpClientFactory httpClientFactory) : base(accountProvider)
   {
      _httpClient = _httpClientFactory.CreateClient(this); // The complete name of your gateway
   }
}
```
The method `_httpClientFactory.CreateClient(this)` gives you the unique HttpClient with pre defined base address that you need.

Now you can communicate with the bank that you want like so:

```csharp
public async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
{
   var responseMessage = await _httpClient.PostAsync([URL], [DATA], cancellationToken);
}
```

#### Reading the requests by using HttpContext
When the clients pay the money inside the bank website, they will return to your website and this is the moment that you must read the information that the bank has sent to you. You can do this like so:
```csharp
public class HeroGateway : GatewayBase<HeroGatewayAccount>
{
   private readonly IHttpContextAccessor _httpContextAccessor;

   public HeroGateway(IGatewayAccountProvider<HeroGatewayAccount> accountProvider,
                    IHttpContextAccessor httpContextAccessor) : base(accountProvider)
   {
      _httpContextAccessor = httpContextAccessor;
   }

   public Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
   {
      var httpContext = _httpContextAccessor.HttpContext;

      var key1 = httpContext.Request.Form["key1"];
      var key2 = httpContext.Request.Form["key2"];
   }
}
```

#### Transporting the user to banks


Transporting by posting a form:
```csharp
public class HeroGateway : GatewayBase<HeroGatewayAccount>
{
   private readonly IHttpContextAccessor _httpContextAccessor;

   public HeroGateway(IGatewayAccountProvider<HeroGatewayAccount> accountProvider,
                    IHttpContextAccessor httpContextAccessor) : base(accountProvider)
   {
      _httpContextAccessor = httpContextAccessor;
   }

   public async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
   {
      var transporter = new GatewayPost(
                httpContextAccessor: httpContextAccessor,
                url: "BANK PAYMENT PAGE URL",
                formData: new Dictionary<string, string>
                {
                    {"FORM FIELD NAME", FORM FIELD VALUE},
                    {"FORM FIELD NAME", FORM FIELD VALUE},
                    // other form fields...
                });

      return new PaymentRequestResult
      {
         IsSucceed = True | False,
         GatewayAccountName = "Gateway Account Name",
         GatewayTransporter = transporter,
         Message = "" // optional
      };
   }
}
```

Transporting by redirecting the user:
```csharp
public class HeroGateway : GatewayBase<HeroGatewayAccount>
{
   private readonly IHttpContextAccessor _httpContextAccessor;

   public HeroGateway(IGatewayAccountProvider<HeroGatewayAccount> accountProvider,
                    IHttpContextAccessor httpContextAccessor) : base(accountProvider)
   {
      _httpContextAccessor = httpContextAccessor;
   }

   public async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
   {
      var transporter = new GatewayRedirect(
                httpContextAccessor: httpContextAccessor,
                url: "BANK PAYMENT PAGE URL");

      return new PaymentRequestResult
      {
         IsSucceed = True | False,
         GatewayAccountName = "Gateway Account Name",
         GatewayTransporter = transporter,
         Message = "" // optional
      };
   }
}
```

#### Implementing the Request method
```csharp
public class HeroGateway : GatewayBase<HeroGatewayAccount>
{
   private readonly IHttpContextAccessor _httpContextAccessor;
   private readonly HttpClient _httpClient;

   public HeroGateway(IGatewayAccountProvider<HeroGatewayAccount> accountProvider,
                      IHttpContextAccessor httpContextAccessor,
                      IHttpClientFactory httpClientFactory) : base(accountProvider)
   {
      _httpContextAccessor = httpContextAccessor;
      _httpClient = httpClientFactory.CreateClient(this);
   }

   public async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
   {
      var account = await GetAccountAsync(invoice);

      // If necessary, use the HttpClient to send and receive data with banks

      var transporter = // Create a transporter

      return new PaymentRequestResult
      {
         IsSucceed = True | False,
         GatewayAccountName = "Gateway Account Name",
         GatewayTransporter = transporter,
         Message = "" // optional
      };
   }
}
```

#### Implementing the Verify method
```csharp
public class HeroGateway : GatewayBase<HeroGatewayAccount>
{
   private readonly IHttpContextAccessor _httpContextAccessor;
   private readonly HttpClient _httpClient;

   public HeroGateway(IGatewayAccountProvider<HeroGatewayAccount> accountProvider,
                      IHttpContextAccessor httpContextAccessor,
                      IHttpClientFactory httpClientFactory) : base(accountProvider)
   {
      _httpContextAccessor = httpContextAccessor;
      _httpClient = httpClientFactory.CreateClient(this);
   }

   public Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
   {
      var account = await GetAccountAsync(context.Payment);

      // Use the HttpContext and HttpClient to communicate with banks

      return new PaymentVerifyResult
      {
         IsSucceed = True | False,
         GatewayAccountName = "GATEWAY ACCOUNT NAME",
         TransactionCode = "THE IMPORTANT CODE THAT YOU RECEIVED FROM THE BANK",
         Message = "" // optional
      }
   }
}
```

#### Implementing the Refund method
```csharp
public class HeroGateway : GatewayBase<HeroGatewayAccount>
{
   private readonly IHttpContextAccessor _httpContextAccessor;
   private readonly HttpClient _httpClient;

   public HeroGateway(IGatewayAccountProvider<HeroGatewayAccount> accountProvider,
                      IHttpContextAccessor httpContextAccessor,
                      IHttpClientFactory httpClientFactory) : base(accountProvider)
   {
      _httpContextAccessor = httpContextAccessor;
      _httpClient = httpClientFactory.CreateClient(this);
   }

   public Task<IPaymentVerifyResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
   {
      var account = await GetAccountAsync(context.Payment);

      // Use the HttpContext and HttpClient to communicate with banks

      return new PaymentRefundResult
      {
         IsSucceed = True | False,
         GatewayAccountName = "GATEWAY ACCOUNT NAME",
         Message = "" // optional
      }
   }
}
```

## Saving data inside the Storage

Sometimes you need to save some data inside the Storage and load them sometime later for example in verifying an invoice.

All you need to do is to add your data inside the `DatabaseAdditionalData` property of your current method.

Parbad saves the DatabaseAdditionalData inside the Storage as string JSON and you can get them later in other methods.

Example:
```csharp
public class HeroGateway : GatewayBase<HeroGatewayAccount>
{
   public HeroGateway(IGatewayAccountProvider<HeroGatewayAccount> accountProvider) : base(accountProvider)
   {
   }

   public async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
   {
      var result = new PaymentRequestResult
      {
      };

      // Saving some data inside the Storage.
      result.DatabaseAdditionalData.Add("KEY", "VALUE");

      return result;
   }

   public Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
   {
      var transaction = context.Transactions.FirstOrDefault(); // geting a specific Transaction

      var data = JsonConvert.DeserializeObject<IDictionary<string, string>>(transaction.AdditionalData);

      var value = data["KEY"];
   }
}
```

## Creating and publishing a Nuget

* Inside the Visual Studio, click on your project and then press `Alt + Enter` to open the project settings.
* Open the `Package` tab and fill the information of your project there.
* Make sure again that the package name is the correct one (For example: Hero.Parbad.Gateway).
* Change the Solution Configuration to `Release` and finally right click on your project and select the `Pack` menu.

You can now publish your created Nuget package in package managers like nuget.org.
