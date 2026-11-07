---
sidebar_position: 10
title: Migrating from 3.x
---

# Migrating from 3.x to 4.0

Parbad 4.0 is a major version: it removes the APIs that 3.x marked as obsolete, and changes some APIs to make payments
safer. For most applications, upgrading means:

1. Update all Parbad packages to 4.0.0 (all packages now share one version).
2. Build, and fix the compile errors with the tables below.
3. With the EF Core storage: add a migration.

Applications that implement their own gateway or storage have more to do; see the last sections.

## Platforms

* The packages target .NET Standard 2.0 and 2.1, .NET 8, .NET 9 and .NET 10.
* The EF Core storage needs .NET 8 and EF Core 8 or later; it works with the EF Core version of your application.
* `Parbad.Mvc5` and `Parbad.Owin` target .NET Framework 4.7.2. `Parbad.Mvc5` needs ASP.NET MVC 5.3.

## Removed APIs

Everything that 3.x marked as `[Obsolete]` is removed:

| 3.x | 4.0 |
|---|---|
| `StaticOnlinePayment.Instance` | `StaticOnlinePayment.CreateScope()`, one scope per web request ([Configuration](./configuration#without-dependency-injection)) |
| `IOnlinePaymentAccessor.OnlinePayment` | `accessor.CreateScope()` |
| `IStorageManager`, `StorageManager`, `UseDefaultStorageManager()` | `IStorage` |
| `IStorage.Payments`, `IStorage.Transactions` | The methods of `IStorage`, such as `GetPaymentByTrackingNumberAsync` |
| `PaymentVerifyResult.Succeed(...)` | `PaymentVerifyResult.Paid(...)`; see [Developing a gateway](./developing-a-gateway) |
| Asan Pardakht: the account's `Key` and `IV`, and the options `ApiUrl`, `EncryptUrl`, `DecryptUrl` | Asan Pardakht uses its REST API (since 3.12): `MerchantConfigurationId`, `UserName`, `Password` |
| ZarinPal: the options `ApiRefundUrl` and `SandboxApiRefundUrl` | Refunds use ZarinPal's GraphQL API with the account's `AuthorizationToken` |

## Payments

The stored payment has a status, instead of the two flags of 3.x:

* `Payment.Status`: `Requested`, `Paid`, `Failed`, `Cancelled`, `PartiallyRefunded` or `Refunded`.
* `Payment.RefundedAmount`: the total refunded so far; a payment can be refunded in several parts.
* `Payment.BankReference`: the reference the bank verified.
* `Payment.IsCompleted` and `Payment.IsPaid` still exist, but are read-only: they follow from `Status`.

## EF Core storage

* Parbad's tables can now live in your own `DbContext`, with `modelBuilder.UseParbad()` and `UseEfCore<TContext>()`;
  see [Storage](./storage). `UseEfCore(options => ...)` with Parbad's own `ParbadDataContext` still works.
* The payment table has three new columns: `status`, `refunded_amount` and `bank_reference`. **Add a migration**
  after the upgrade. Existing rows need no data migration: their status follows from their existing columns, and their
  refunds from their transactions.
* `ParbadDataContext` takes `DbContextOptions<ParbadDataContext>`, and the storage class is
  `EntityFrameworkCoreStorage<TContext>`. This matters only for code that creates them itself.

## Money

* The conversion from `Money` to `long` is **explicit**, and throws when the amount has a fraction: write
  `(long)amount`. The conversion to `decimal` stays implicit.
* Equality is exact: `new Money(10.5m)` no longer equals `10`.
* `Money.Parse` uses the invariant culture, so `"1000.5"` means the same on every machine.

## Results

The results of `RequestAsync`, `FetchAsync`, `VerifyAsync` and the others are immutable: their properties are set when
the result is created. Code that changed a result after the fact no longer compiles.

## Nullable annotations

All packages are annotated for nullable reference types. In a project with nullable enabled, the compiler may now warn
where a value can be null, for example `result.Message`. These are warnings only.

## Your own storage

`IStorage` has three new methods, which were optional in 3.12 (`IPaymentCompletion`, `IPaidPaymentLookup`) and are now
required:

* `TryCompletePaymentAsync`: completes a payment in one atomic step, only if the stored payment allows it.
* `FindPaidPaymentByBankReferenceAsync`: finds a paid payment by the bank's reference.
* `GetLargestTrackingNumberAsync`: the largest tracking number, for `UseAutoIncrementTrackingNumber()`.

Store the new `Status`, `RefundedAmount` and `BankReference` of each payment. The XML documentation of `IStorage`
describes each member. A storage derived from the cache storage: `FindPaidPaymentByTransactionCodeAsync` is now
`FindPaidPaymentByBankReferenceAsync`.

## Your own gateway

A gateway now reports what the bank said, and Parbad decides the result:

* A successful verification is `PaymentVerifyResult.Paid(transactionCode, bankReference, message)`. Parbad accepts a
  bank reference for one invoice only.
* An "already verified" answer that does not prove which payment it belongs to is
  `PaymentVerifyResult.AlreadyVerifiedAtBank(...)`.
* When the bank reports the amount or the order ID, set `BankReportedAmount` and `BankReportedOrderId` with a `with`
  expression; Parbad compares them with the invoice.

[Developing a gateway](./developing-a-gateway) shows a complete gateway.

## Behaviour changes

* ZarinPal uses its `payment.zarinpal.com` addresses, sends the customer's email and mobile number as `metadata`, and
  supports refunds.
