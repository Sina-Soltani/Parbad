---
sidebar_position: 10
title: Migrating from 3.x
---

# Migrating from 3.x to 4.0

:::caution Draft
This page is an outline for the preview of the documentation site.
:::

## Platforms

* Parbad targets .NET Standard 2.0 and 2.1, .NET 8, .NET 9 and .NET 10. `Parbad.Mvc5` and `Parbad.Owin` target
  .NET Framework 4.7.2 and need ASP.NET MVC 5.3.
* All packages share one version: 4.0.0.

## Removed APIs

Everything that was marked `[Obsolete]` in 3.x is removed, for example:

| 3.x | 4.0 |
|---|---|
| `StaticOnlinePayment.Instance` | `StaticOnlinePayment.CreateScope()`, one scope per request |
| `IStorageManager`, `StorageManager` | `IStorage` |
| `IStorage.Payments`, `IStorage.Transactions` | The query methods of `IStorage` |

## EF Core storage

* The storage needs EF Core 8 or later.
* Parbad's tables can now live in your own `DbContext`: see [Storage](./storage.md).
* The payment table has three new columns: `status`, `refunded_amount` and `bank_reference`. Add a migration. Existing
  rows need no data migration.

## Money

The conversion from `Money` to `long` is explicit and throws when the amount has a fraction.

## Custom gateways

A gateway reports what the bank said, and Parbad decides the result. Verification results are created with
`PaymentVerifyResult.Paid(...)`, `PaymentVerifyResult.AlreadyVerifiedAtBank(...)` and `PaymentVerifyResult.Failed(...)`.
