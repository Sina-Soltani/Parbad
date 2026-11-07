---
sidebar_position: 4
title: Payment
---

# Payment

A payment goes through these steps: **request** → the customer pays at the bank → **fetch** → **verify** (or
**cancel**) → later, if needed, **refund**.

Inject `IOnlinePayment` wherever you need it. Without dependency injection, use `StaticOnlinePayment.CreateScope()`;
see [Configuration](./configuration#without-dependency-injection).

## Requesting a payment

```csharp
var result = await _onlinePayment.RequestAsync(invoice =>
{
    invoice
        .SetAmount(250_000)
        .SetCallbackUrl("https://shop.example/payment/verify")
        .UseAutoIncrementTrackingNumber()
        .UseMellat();
});
```

* **Amount:** the amount to pay, in the unit the gateway expects. Parbad never converts amounts.
* **Callback URL:** an absolute URL of your website. The bank sends the customer back to it.
* **Tracking number:** a unique number for each invoice. Give your own with `SetTrackingNumber(...)`, or let Parbad
  choose one; see [Invoice Builder](./invoice-builder).
* **Gateway:** `UseMellat()`, `UseZarinPal()`, ... or `SetGateway("Mellat")`. See [Gateways](/docs/gateways) for the
  data each gateway needs.

A short form for simple invoices:

```csharp
var result = await _onlinePayment.RequestAsync("Mellat", trackingNumber: 1001, amount: 250_000,
                                               callbackUrl: "https://shop.example/payment/verify");
```

The gateway names are: `AsanPardakht`, `FanAva`, `IdPay`, `IranKish`, `Mellat`, `Melli`, `ParbadVirtual`, `Parsian`,
`Pasargad`, `PayIr`, `PayPing`, `Saman`, `Sepehr`, `YekPay`, `ZarinPal`, `Zibal`. They are not case-sensitive.

### Sending the customer to the bank

When the request succeeds, store `result.TrackingNumber` with your order and send the customer to the bank.

In ASP.NET Core (`using Parbad.AspNetCore;`) and ASP.NET MVC 5 (`using Parbad.Mvc;`):

```csharp
if (result.IsSucceed)
{
    return result.GatewayTransporter.TransportToGateway();
}

// The request failed: result.Message says why.
```

In WebForms:

```csharp
await result.GatewayTransporter.TransportAsync();
```

### APIs and client-side applications

An API, for example behind Angular or React, sends the description of the transport to the client instead:

```csharp
if (result.IsSucceed)
{
    return Ok(result.GatewayTransporter.Descriptor);
}
```

The descriptor contains the type of the transport (`Redirect` or `Post`), the URL of the bank's page, and the form
fields to post. The client then redirects the browser, or posts the form.

## Verifying a payment

The bank sends the customer back to the callback URL, with GET or POST. Fetch the invoice, check your order, then verify:

```csharp
[HttpGet, HttpPost]
public async Task<IActionResult> Verify()
{
    var invoice = await _onlinePayment.FetchAsync();

    if (invoice.Status != PaymentFetchResultStatus.ReadyForVerifying)
    {
        // Failed: the customer cancelled or the bank rejected the payment.
        // AlreadyProcessed: the payment was already verified or cancelled, e.g. after a refresh of the page.
        return View("PaymentFailed", invoice.Message);
    }

    if (!IsOrderStillValid(invoice.TrackingNumber))
    {
        await _onlinePayment.CancelAsync(invoice, cancellationReason: "The product is sold out.");

        return View("PaymentFailed", "The product is sold out.");
    }

    var verifyResult = await _onlinePayment.VerifyAsync(invoice);

    if (verifyResult.Status == PaymentVerifyResultStatus.Succeed)
    {
        // Paid: deliver the order. verifyResult.TransactionCode is the bank's code of the transaction.
        return View("PaymentSucceeded", verifyResult.TransactionCode);
    }

    // Failed, or AlreadyVerified when the payment was already verified before.
    return View("PaymentFailed", verifyResult.Message);
}
```

Check the order **before** `VerifyAsync`: once a payment is verified, the money is taken, and giving it back needs a
refund.

Parbad verifies each payment only once, also when the page is refreshed or two servers verify the same payment at the
same time. When the bank reports the amount or the order of the payment, Parbad compares them with the invoice, and it
accepts a bank's reference for one invoice only.

## Refunding a payment

```csharp
// The whole amount
var result = await _onlinePayment.RefundCompletelyAsync(trackingNumber);

// A part of it
var result = await _onlinePayment.RefundSpecificAmountAsync(trackingNumber, amount: 100_000);
```

Several partial refunds are possible, up to the paid amount. The payment's `Status` becomes `PartiallyRefunded`, then
`Refunded`, and `RefundedAmount` holds the total. Not every gateway supports refunds; for a gateway that does not, the
result is a failure that says so.
