---
sidebar_position: 5
title: Invoice Builder
---

# Invoice Builder

`RequestAsync(invoice => ...)` describes the invoice with the Invoice Builder:

```csharp
var result = await _onlinePayment.RequestAsync(invoice =>
{
    invoice
        .SetTrackingNumber(1001)
        .SetAmount(250_000)
        .SetCallbackUrl("https://shop.example/payment/verify")
        .UseMellat();
});
```

## Tracking numbers

Each invoice needs a unique tracking number. Give your own with `SetTrackingNumber(...)`, or let Parbad choose one:

```csharp
// One more than the largest tracking number in the storage.
invoice.UseAutoIncrementTrackingNumber();

// A random number that is not in the storage yet.
invoice.UseAutoRandomTrackingNumber();
```

Their minimum value (and the increment) are set with `ConfigureAutoIncrementTrackingNumber` and
`ConfigureAutoRandomTrackingNumber`; see [Configuration](./configuration#tracking-numbers).

## Choosing the gateway

```csharp
invoice.UseMellat();

// or by name, e.g. from your settings (not case-sensitive)
invoice.SetGateway("Mellat");
```

Some gateways take more data, such as the customer's mobile number or a description; see [Gateways](/docs/gateways).

## Choosing the account

When a gateway has several accounts, choose one by its name:

```csharp
invoice.UseAccount("Donations");
```

See [several accounts for one gateway](./configuration#several-accounts-for-one-gateway).

## Properties

An invoice can carry your own values, which a gateway or a formatter can read:

```csharp
invoice.AddOrUpdateProperty("OrderId", orderId);
```

## Formatters

A formatter changes the invoice before it is sent to the gateway, e.g. to apply a rule to every invoice:

```csharp
invoice.AddFormatter(invoice => invoice.CallbackUrl = new CallbackUrl("https://shop.example/payment/verify"));
```
