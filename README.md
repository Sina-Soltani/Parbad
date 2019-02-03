
### What is Parbad?

[مشاهده مقاله به فارسی](https://www.dotnettips.info/post/2880/%d9%be%d8%b1%d8%a8%d8%a7%d8%af-%d8%b1%d8%a7%d9%87%d9%86%d9%85%d8%a7%db%8c-%d8%a7%d8%aa%d8%b5%d8%a7%d9%84-%d9%88-%d9%be%db%8c%d8%a7%d8%af%d9%87%e2%80%8c%d8%b3%d8%a7%d8%b2%db%8c-%d8%af%d8%b1%da%af%d8%a7%d9%87%e2%80%8c%d9%87%d8%a7%db%8c-%d8%a7%db%8c%d9%86%d8%aa%d8%b1%d9%86%d8%aa%db%8c-%d8%b4%d8%a8%da%a9%d9%87-%d8%b4%d8%aa%d8%a7%d8%a8)

Parbad is an integrated online payment system. It helps developers and site owners to add Online Payment ability to their websites.
It can communicate with the most popular banks in Iran like: Mellat, Parsian, Tejarat, Saman and Pasargad.
These providers are also called , Shetab Network.

Unfortunately, these providers don't have a regular and clear instructions for developers to implement their codes and each of them has a different implementation.

But with Parbad you can with only three or four lines of codes, communicate with these providers.


| Gateway                  | Status     | Information |
| ------------------------ |:----------:| ------------------------------------------ |
| Mellat			       | Tested     | |
| Saman				       | Tested     | |
| Parsian				   | Tested     | |
| Pasargad				   | Tested     | |
| Iran Kish				   | Tested | |
| Parbad Virtual Gateway   | Tested     | |
| Melli					   | Tested | |


### Getting Started

* Usage
	* Install
	* Create an invoice and send it to gateway
	* Redirect client to the gateway
	* Verify payment
	* Use Parbad Virtual Gateway for development.
* Configuration
	* Gateways
	* Storage
	* Logger

### Install (via [Nuget](https://www.nuget.org/packages/Parbad/))
```
PM> Install-Package Parbad
```

For MVC (4+ , 5+) projects:
```
PM> Install-Package Parbad.Mvc5
```
- - - -
### Create invoice & redirect the client to gateway

First, you need to create an invoice and then send it to the specific gateway (Bank) that you want.
```csharp
var invoice = new Invoice( [Order Number], [Amount], "VERIFY URL");

var result = Payment.Request(Gateways.[Your Selected Gateway], invoice);
```
> A unique order number is needed for each requests.
> Amount is in official currency of Iran ( Rial )
> VERIFY URL is a URL of your website to verify the payment after client pays and come back from the gateway (You will learn how to verify a payment)

__Example:__
```csharp
var invoice = new Invoice(123, 25000, "http://www.mywebsite.com/payment/verify");

var result = Payment.Request(Gateways.Mellat, invoice);

if (result.Status == RequestResultStatus.Success)
{
    // Redirect the client to Gateway's website.
    result.RedirectToGateway(Context);
}
else
{
    Label1.Text = result.Message;
}
```
Here, the result object is the result of the request that you sent to the gateway:
* ReferenceId: A uniqe code for your payment's request. This code usually comes from the banks
* Status: Tells you about the result's status
* Message: A formatted message about the result (you can show it to the client if you like)

For MVC(4+, 5+) projects you can Install Parbad.Mvc5 and do like this:
```csharp
if (result.Status == RequestResultStatus.Success)
{
    // redirect the client to Gateway's website
    return result.ToActionResult();
}
else
{
	// you can show the message if you want
    var msg = result.Message;
}
```
- - - -
### Verify payment

When clients come back from the gateway then you must verify their payments.
But, where should you do verifying! Remember VERIFY URL in step 1?
So you should inside the Page_Load method of that URL do verifying. (if your project is an ASP.NET MVC project, then you should write the bellow codes in ActionMethod of that URL)

```c
var result = Payment.Verify(HttpContext);

if(result.Status == VerifyResultStatus.Success)
{
    // Client paid successfully and you can continue purchase steps.
}
else
{
    // Client didn't paid for some reasons. You can also see the reason in Status and Message properties.
    // So you can mark this payment as a rejected payment.
}
```
* The VerifyResult object in the above codes contains information about the verified payment.
* Gateway: The gateway that client paid the order with
* ReferenceId: The same code that you got in the first step
* TransactionId: As its name implies, it's a unique and important ID for payment that comes from the gateway (Bank)
* Status: Tells you about the result's status
* Message: A formatted message about the result. You can show it to the client if you like

Note that you must save this information in your database
All done.
- - - -
### Parbad Virtual Gateway

There is a virtual gateway for developing purposes. It's actually a simulator that simulates a gateway and its payment steps.
So, to test your application's functionality you don't need to have a real gateway with real data or a real bank account.
It's also a nice page that styled using bootstrap and shows you order's information like amount, order number, etc.
To enable it you must first configure it like so: (You can get complete Configuration's informaton afterwards)
```csharp
ParbadConfiguration.Gateways.ConfigureParbadVirtualGateway(new ParbadVirtualGatewayConfiguration("Parbad.axd"));
```

And also in `web.config` file of your web application project, add this configs:
```
<system.webServer>
  <handlers>
   <add name="ParbadGatewayPage" verb="*" path="Parbad.axd" type="Parbad.Web.Gateway.ParbadVirtualGatewayHandler" />
  </handlers>
</system.webServer>
```

All done. You successfully configured Parbad Virutal Gateway.
Now to use it just when you create an invoice and want to send it to a gateway, you should choose ParbadVirtualGateway (from enum items). Like so:
```csharp
var invoice = new Invoice(123, 25000, "http://www.mywebsite.com/payment/verify");

var result = Payment.Request(Gateways.ParbadVirtualGateway, invoice);
```
- - - -
### Configuration

A good place to put configurations is in `Global.asax.cs` file.

```c
public class Global : HttpApplication
{
    void Application_Start(object sender, EventArgs e)
    {
        // configurations
    }
}
```
Or if you use ASP.NET MVC and [Owin](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/owin), then you can put them in `Startup.cs` file.

```csharp
public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        // configurations
    }
}
```

__Gateways Configurations__

Before start using Parbad, you must configure the gateways that you want to work with.
Each gateways has its own configuration.

Example: Configure __Mellat__ Gateway:

```csharp
var mellatConfig = new MellatGatewayConfiguration(1111, "MyUserName", "MyPassword");

ParbadConfiguration.Gateways.ConfigureMellatGateway(mellatConfig);
```

__Storage Configurations__

Parbad needs a storage for saving/loading payments data.
It uses TemporaryMemoryStorage as default storage. It saves the data in internal web server memory (If for any reasons your web host restarts then you will loose saved payments data).
You can also use SqlServerStorage to save/load data in/from SQL Server.

```csharp
ParbadConfiguration.Storage = new SqlServerStorage("connection string");
```

Or if you want to save/load data by your specific way, then you can do like so:

```csharp
public class MyStorage : Storage
{
    // Implement methods here...
}
```

And then

```csharp
ParbadConfiguration.Storage = new MyStorage();
```

__Logger Configurations__

Parbad can log all payment operations. To enable it you only need to do this:

```csharp
ParbadConfiguration.Logger.Provider = new XmlFileLogger("path");
```

As its name implies, It logs data as XML files into the specified path. 
Or if you prefer to log data yourself then just do this:

```csharp
public class MyLogger : Logger
{
    protected internal override void Write(Log log)
    {
        // Your codes...
    }

    protected internal override LogReadContext Read(int pageNumber, int pageSize)
    {
        // Your codes...
    }
}
```

And then:

```csharp
ParbadConfiguration.Logger.Provider = new MyLogger();
```

__How to see logs in a styled page?__
I created a HttpHandler named ParbadLogViewerHandler. It shows you the logs in a styled page.
To use it, you must add these configurations in the web.config file of your web application.
```
<system.webServer>
  <handlers>
   <add name="ParbadLogViewer" verb="*" path="ParbadLog.axd" type="Parbad.Web.LogViewer.ParbadLogViewerHandler" />
  </handlers>
</system.webServer>
```

So you can access to this handler by typing your website URL + the __path__'s value in above configuration in your browser.
An example would be like this:
http://www.mywebsite.com/ParbadLog.axd

### License, etc.

Parbad is Copyright &copy; 2017 [Sina Soltani](https://www.linkedin.com/in/sinasoltani/) and other contributors under the [Apache license](LICENSE.txt).
Please let me know if you find any bugs or have any suggestions

با توجه به دارا بودن پَرباد به لایسنس، در صورت استفاده و یا کپی از کدهای این پروژه و بازنشر آن، مراتب به گیت هاب و سایر منابع گزارش خواهد شد
