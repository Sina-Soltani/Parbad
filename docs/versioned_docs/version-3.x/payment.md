---
title: "Payment"
slug: /payment
---

# Payment

* [Requesting a payment](#requesting-a-payment)
  * [ASP.NET CORE](#aspnet-core)
    * [Sample Project](https://github.com/Sina-Soltani/Parbad.Samples/tree/master/Web/Parbad.Sample.AspNetCore)
  * [ASP.NET MVC](#aspnet-mvc)
    * [Sample Project](https://github.com/Sina-Soltani/Parbad.Samples/tree/master/Web/Parbad.Sample.Mvc)
  * [ASP.NET WebForms](#aspnet-webforms)
    * [Sample Project](https://github.com/Sina-Soltani/Parbad.Samples/tree/master/Web/Parbad.Sample.WebForm)
  * [APIs and Client-Side Applications](#apis-and-client-side-applications)
    * [Sample Project](https://github.com/Sina-Soltani/Parbad.Samples/tree/master/Web/Parbad.Sample.Angular)
* [Verifying a payment](#verifying-a-payment)
* [Refunding a payment](#refunding-a-payment)
* [Samples](https://github.com/Sina-Soltani/Parbad.Samples)

Before starting, depends on your application choose either IOnlinePayment interface or StaticOnlinePayment class.
[Learn more](./configuration.md#general-configuration)

### Requesting a payment

```csharp
var result = _onlinePayment.Request("<GATEWAY NAME>", <TRACKING NUMBER>, <AMOUNT>, "<CALLBACK URL>");
```

You can also use the [Invoice Builder](./invoice-builder.md) like so:
```csharp
var result = _onlinePayment.Request(invoice =>
{
    invoice.SetTrackingNumber(<TRACKING NUMBER>)
           .SetAmount(<AMOUNT>)
           .SetCallbackUrl("<CALLBACK URL>")
           .SetGateway("<GATEWAY NAME>");

        // Or use a gateway directly

           .UseMellat();
});
```
* Tracking Number: A unique number for each request.
* Amount: The amount that must be paid.
* Callback URL: Is an absolute URL in your website. Gateways redirect the clients to this URL for verifying the payments.
* Gateway: The invoice will be sent to this gateway. 

The gateways names are:
* Mellat
* Melli
* Saman
* Parsian
* Pasargad
* IranKish
* Sepehr
* ZarinPal
* PayIr
* IdPay
* YekPay
* PayPing
* ParbadVirtual

**Note:** Gateway name is **NOT** case sensitive. "Mellat" = "mellat" = "MELLAT"

See the [Invoice Builder](./invoice-builder.md) for more functions.

#### Samples:

##### ASP.NET CORE
```csharp
using Parbad.AspNetCore;

private readonly IOnlinePayment _onlinePayment;

public PaymentController(IOnlinePayment onlinePayment)
{
    _onlinePayment = onlinePayment;
}

public async Task<IActionResult> Pay()
{
    var result = await _onlinePayment.RequestAsync("Mellat", 123, 25000, "http://www.mywebsite.com/foo/bar/");

    if (result.IsSucceed)
    {
        // Save the TrackingNumber inside your database.

        // It will redirect the client to the gateway.
        return result.GatewayTransporter.TransportToGateway();
    }
    else
    {
        // The request was not successful. You can see the Message property for more information.
    }
}
```
[ASP.NET Core Sample Project](https://github.com/Sina-Soltani/Parbad.Samples/tree/master/Web/Parbad.Sample.AspNetCore)

##### ASP.NET MVC
```csharp
using Parbad.Mvc5;

private readonly IOnlinePayment _onlinePayment;

public PaymentController(IOnlinePayment onlinePayment)
{
    _onlinePayment = onlinePayment;
}

public async Task<ActionResult> Pay()
{
    var result = await _onlinePayment.RequestAsync("Mellat", 123, 25000, "http://www.mywebsite.com/foo/bar/");

    if (result.IsSucceed)
    {
        // Save the TrackingNumber inside your database.

        // It will redirect the client to the gateway.
        return result.GatewayTransporter.TransportToGateway();
    }
    else
    {
        // The request was not successful. You can see the Message property for more information.
    }
}
```
[ASP.NET MVC Sample Project](https://github.com/Sina-Soltani/Parbad.Samples/tree/master/Web/Parbad.Sample.Mvc)

##### ASP.NET WebForms
```csharp
using Parbad;

protected async void BtnPay_Click(object sender, EventArgs e)
{
    // Static Usage
    var result = await StaticOnlinePayment.Instance.RequestAsync("Mellat", 123, 25000, "http://www.mywebsite.com/foo/bar/");

    if (result.IsSucceed)
    {
        // Save the TrackingNumber inside your database.

        // It will redirect the client to the gateway.
        await result.GatewayTransporter.TransportAsync();
    }
    else
    {
        // The request was not successful. You can see the Message property for more information.
    }
}
```

[ASP.NET WebForms Sample Project](https://github.com/Sina-Soltani/Parbad.Samples/tree/master/Web/Parbad.Sample.WebForm)

##### APIs and Client-Side Applications
For APIs and Client-Side applications such as Angular, React, Ajax and so on you can send the `Descriptor` to the client.
The `Descriptor` contains all information which you need for transporting the client to the gateways.

```csharp
using Parbad.AspNetCore;

private readonly IOnlinePayment _onlinePayment;

public PaymentController(IOnlinePayment onlinePayment)
{
    _onlinePayment = onlinePayment;
}

public async Task<IActionResult> Pay()
{
    var result = await _onlinePayment.RequestAsync("Mellat", 123, 25000, "http://www.mywebsite.com/foo/bar/");

    if (result.IsSucceed)
    {
        // Save the TrackingNumber inside your database.

        // Descriptor contains all information about the transportation to the specified gateway.
        return Ok(result.GatewayTransporter.Descriptor);
    }
    else
    {
        // The request was not successful. You can see the Message property for more information.
    }
}
```
[Angular Sample Project](https://github.com/Sina-Soltani/Parbad.Samples/tree/master/Web/Parbad.Sample.Angular)

### Verifying a payment
When clients return from gateways, you need to check whether they have paid or not.

```csharp
[HttpGet, HttpPost] // Gateways contact you with different HTTP methods, so make sure that you support both GET and Post.
public async Task<IActionResult> Verify()
{
   var invoice = await _onlinePayment.FetchAsync();

   if (invoice.Status != PaymentFetchResultStatus.ReadyForVerifying)
   {
     // Check if the invoice is new or it's already processed before.
      var isAlreadyProcessed = invoice.Status == PaymentFetchResultStatus.AlreadyProcessed;
      var isAlreadyVerified = invoice.IsAlreadyVerified;
      return Content("The payment was not successful.");
   }

   // An example of checking the invoice in your website.
   if (!Is_There_Still_Product_In_Shop(invoice.TrackingNumber))
   {
      var cancelResult = await _onlinePayment.CancelAsync(invoice, cancellationReason: "Sorry, We have no more products to sell.");

      return Content("Order failed.");
   }

   var verifyResult = await _onlinePayment.VerifyAsync(invoice);

   // checking the status of the verification method
   if (verifyResult.Status != PaymentVerifyResultStatus.Succeed)
   {
      // checking if the payment is already verfied
      var isAlreadyVerified = verifyResult.Status == PaymentVerifyResultStatus.AlreadyVerified;
      return Content("The payment is already verified before.");
   }

   return Content("The payment was successful");
}
```

### Refunding a payment

```csharp
var trackingNumber = 123;

var result = _onlinePayment.RefundCompletely(trackingNumber);
```