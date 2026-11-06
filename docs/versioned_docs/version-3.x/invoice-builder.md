---
title: "Invoice Builder"
slug: /invoice-builder
---

# Invoice Builder

The Invoice Builder helps you to create and customize an invoice.

* [Requesting a payment](./invoice-builder.md#requesting-a-payment)
* [Using Auto Increment Tracking Number](./invoice-builder.md#using-auto-increment-tracking-number)
* [Using Auto Random Tracking Number](./invoice-builder.md#using-auto-random-tracking-number)
* [Requesting a payment with a specific account](./invoice-builder.md#requesting-a-payment-with-a-specific-account)
* [Gateways Specific Functions](./gateways-specific-functions.md)

#### Requesting a payment

A simple payment request is something like:
```csharp
var result = _onlinePayment.Request("Mellat", 123, 25000, "http://www.mywebsite.com/foo/bar/");
```
You can make the same request by using the Invoice Builder like so:
```csharp
var result = _onlinePayment.Request(invoice =>
{
    invoice
          .SetTrackingNumber(123)
          .SetAmount(25000)
          .SetCallbackUrl("http://www.mywebsite.com/foo/bar/")
          .SetGateway("Mellat"); // Gateway name is NOT case sensitive. "Mellat" = "mellat" = "MELLAT"

        // OR

          .UseMellat();
});
```

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
* ParbadVirtual
* PayPing

#### Using Auto Increment Tracking Number

```csharp
var result = _onlinePayment.Request(invoice =>
{
    invoice.UseAutoIncrementTrackingNumber();
});
```
It generates automatically a new tracking number which is one unit greater than the latest number.

#### Using Auto Random Tracking Number
```csharp
var result = _onlinePayment.Request(invoice =>
{
    invoice.UseAutoRandomTrackingNumber();
});
```
It generates randomly a tracking number which doesn't exist in Parbad Storage.

#### Requesting a payment with a specific account
```csharp
var result = _onlinePayment.Request(invoice =>
{
    invoice.UseAccount("Account 1");
});
```
See also [Adding multiple accounts](./configuration.md#adding-multiple-accounts-for-a-gateway)