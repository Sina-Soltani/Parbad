---
sidebar_position: 1
title: Introduction
slug: /intro
---

# Parbad

Parbad is a free and open-source online payment library for .NET. It connects web applications to Iranian online
payment gateways: it requests a payment, sends the customer to the bank, verifies the payment when the customer comes
back, and can refund it.

## How it works

![How Parbad works](/img/how-it-works.png)

1. Your application asks Parbad for a payment (an *invoice*: amount, callback URL, gateway).
2. Parbad registers the payment with the bank and sends the customer to the bank's payment page.
3. The bank sends the customer back to your callback URL.
4. Parbad fetches the payment; you check your order (stock, price, ...).
5. Parbad verifies the payment with the bank. Only then is the money really taken.

Parbad stores the state of every payment, so a payment is verified only once, even when the callback page is refreshed
or the same payment is verified on two servers at the same time.

## Features

* Free and open-source
* One API for every gateway
* Virtual Gateway: a simulated bank for development and tests
* ASP.NET Core, ASP.NET MVC 5 and WebForms
* Storage in your own database with EF Core, in a cache, or in your own implementation

## Gateways

| Gateway | Package |
|---|---|
| Mellat, Melli, Saman, Pasargad, Parsian, Asan Pardakht | [Parbad](https://www.nuget.org/packages/Parbad/) |
| Iran Kish | [Parbad.Gateway.IranKish](https://www.nuget.org/packages/Parbad.Gateway.IranKish/) |
| Sepehr | [Parbad.Gateway.Sepehr](https://www.nuget.org/packages/Parbad.Gateway.Sepehr/) |
| ZarinPal | [Parbad.Gateway.ZarinPal](https://www.nuget.org/packages/Parbad.Gateway.ZarinPal/) |
| Pay.ir | [Parbad.Gateway.PayIr](https://www.nuget.org/packages/Parbad.Gateway.PayIr/) |
| IDPay | [Parbad.Gateway.IdPay](https://www.nuget.org/packages/Parbad.Gateway.IdPay/) |
| YekPay | [Parbad.Gateway.YekPay](https://www.nuget.org/packages/Parbad.Gateway.YekPay/) |
| PayPing | [Parbad.Gateway.PayPing](https://www.nuget.org/packages/Parbad.Gateway.PayPing/) |
| Zibal | [Parbad.Gateway.Zibal](https://www.nuget.org/packages/Parbad.Gateway.Zibal/) |
| FanAva | [Parbad.Gateway.FanAva](https://www.nuget.org/packages/Parbad.Gateway.FanAva/) |

Next: [Getting started](./getting-started.md).
