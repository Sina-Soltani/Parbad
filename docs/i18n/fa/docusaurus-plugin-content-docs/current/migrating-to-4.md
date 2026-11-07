---
sidebar_position: 10
title: مهاجرت از نسخه 3.x
source_hash: 27ebdd02dc5a
---

# مهاجرت از نسخه 3.x به 4.0

پرباد 4.0 یک نسخه اصلی (major) است: APIهایی را که در 3.x منسوخ (obsolete) شده بودند حذف می‌کند و بعضی APIها را
تغییر می‌دهد تا پرداخت‌ها امن‌تر شوند. برای بیشتر برنامه‌ها، ارتقا یعنی:

1. همه پکیج‌های پرباد را به 4.0.0 به‌روز کنید (حالا همه پکیج‌ها یک نسخه مشترک دارند).
2. پروژه را build کنید و خطاهای کامپایل را با جدول‌های زیر برطرف کنید.
3. اگر از ذخیره‌سازی EF Core استفاده می‌کنید: یک migration اضافه کنید.

برنامه‌هایی که درگاه یا ذخیره‌سازی اختصاصی دارند کار بیشتری دارند؛ بخش‌های آخر را ببینید.

## پلتفرم‌ها {#platforms}

* پکیج‌ها برای ‎.NET Standard 2.0 و 2.1، ‎.NET 8، ‎.NET 9 و ‎.NET 10 ساخته می‌شوند.
* ذخیره‌سازی EF Core به ‎.NET 8 و EF Core 8 یا بالاتر نیاز دارد و با نسخه EF Core برنامه شما کار می‌کند.
* `Parbad.Mvc5` و `Parbad.Owin` برای ‎.NET Framework 4.7.2 ساخته می‌شوند. `Parbad.Mvc5` به ASP.NET MVC 5.3 نیاز دارد.

## APIهای حذف‌شده {#removed-apis}

هر چیزی که در 3.x با `[Obsolete]` علامت خورده بود حذف شده است:

| 3.x | 4.0 |
|---|---|
| `StaticOnlinePayment.Instance` | `StaticOnlinePayment.CreateScope()`، یک scope برای هر درخواست وب ([پیکربندی](./configuration#without-dependency-injection)) |
| `IOnlinePaymentAccessor.OnlinePayment` | `accessor.CreateScope()` |
| `IStorageManager`، `StorageManager`، `UseDefaultStorageManager()` | `IStorage` |
| `IStorage.Payments`، `IStorage.Transactions` | متدهای `IStorage`، مانند `GetPaymentByTrackingNumberAsync` |
| `PaymentVerifyResult.Succeed(...)` | `PaymentVerifyResult.Paid(...)`؛ [توسعه یک درگاه](./developing-a-gateway) را ببینید |
| آسان پرداخت: `Key` و `IV` حساب و تنظیمات `ApiUrl`، `EncryptUrl`، `DecryptUrl` | آسان پرداخت (از نسخه 3.12) از REST API خود استفاده می‌کند: `MerchantConfigurationId`، `UserName`، `Password` |
| زرین‌پال: تنظیمات `ApiRefundUrl` و `SandboxApiRefundUrl` | استرداد وجه از GraphQL API زرین‌پال با `AuthorizationToken` حساب انجام می‌شود |

## پرداخت‌ها {#payments}

پرداخت ذخیره‌شده به جای دو پرچم نسخه 3.x یک وضعیت دارد:

* `Payment.Status`: `Requested`، `Paid`، `Failed`، `Cancelled`، `PartiallyRefunded` یا `Refunded`.
* `Payment.RefundedAmount`: مجموع مبالغ مسترد شده تا کنون؛ وجه یک پرداخت می‌تواند در چند بخش مسترد شود.
* `Payment.BankReference`: شماره مرجعی که بانک تأیید کرده است.
* `Payment.IsCompleted` و `Payment.IsPaid` هنوز وجود دارند اما فقط‌خواندنی‌اند و از `Status` به دست می‌آیند.

## ذخیره‌سازی EF Core {#ef-core-storage}

* جدول‌های پرباد حالا می‌توانند با `modelBuilder.UseParbad()` و `UseEfCore<TContext>()` در `DbContext` خود شما باشند؛
  [ذخیره‌سازی](./storage) را ببینید. `UseEfCore(options => ...)` با `ParbadDataContext` خود پرباد هم همچنان کار می‌کند.
* جدول پرداخت سه ستون جدید دارد: `status`، `refunded_amount` و `bank_reference`. پس از ارتقا **یک migration اضافه
  کنید**. ردیف‌های موجود به مهاجرت داده نیاز ندارند: وضعیتشان از ستون‌های موجود و استرداد وجهشان از تراکنش‌هایشان
  به دست می‌آید.
* `ParbadDataContext` یک `DbContextOptions<ParbadDataContext>` می‌گیرد و کلاس ذخیره‌سازی
  `EntityFrameworkCoreStorage<TContext>` است. این فقط برای کدی مهم است که خودش آن‌ها را می‌سازد.

## Money {#money}

* تبدیل `Money` به `long` **صریح (explicit)** است و اگر مبلغ بخش اعشاری داشته باشد خطا می‌دهد: `(long)amount`
  بنویسید. تبدیل به `decimal` همچنان ضمنی (implicit) است.
* مقایسه دقیق است: `new Money(10.5m)` دیگر با `10` برابر نیست.
* `Money.Parse` از فرهنگ ثابت (invariant culture) استفاده می‌کند، بنابراین `"1000.5"` روی هر سیستمی یک معنا دارد.

## نتیجه‌ها {#results}

نتیجه‌های `RequestAsync`، `FetchAsync`، `VerifyAsync` و بقیه تغییرناپذیرند (immutable): ویژگی‌هایشان هنگام ساختن
نتیجه مقدار می‌گیرند. کدی که بعداً یک نتیجه را تغییر می‌داد دیگر کامپایل نمی‌شود.

## حاشیه‌نویسی‌های nullable {#nullable-annotations}

همه پکیج‌ها برای nullable reference types حاشیه‌نویسی شده‌اند. در پروژه‌ای که nullable در آن فعال است، کامپایلر
ممکن است حالا جایی که مقداری می‌تواند null باشد هشدار بدهد، برای مثال `result.Message`. این‌ها فقط هشدارند.

## ذخیره‌سازی اختصاصی {#your-own-storage}

`IStorage` سه متد جدید دارد که در 3.12 اختیاری بودند (`IPaymentCompletion`، `IPaidPaymentLookup`) و حالا الزامی‌اند:

* `TryCompletePaymentAsync`: پرداخت را در یک مرحله اتمی تکمیل می‌کند، فقط اگر پرداخت ذخیره‌شده اجازه دهد.
* `FindPaidPaymentByBankReferenceAsync`: یک پرداخت پرداخت‌شده را با شماره مرجع بانک پیدا می‌کند.
* `GetLargestTrackingNumberAsync`: بزرگ‌ترین شماره پیگیری، برای `UseAutoIncrementTrackingNumber()`.

`Status`، `RefundedAmount` و `BankReference` جدید هر پرداخت را ذخیره کنید. مستندات XML اینترفیس `IStorage` هر عضو را
توضیح می‌دهد. برای ذخیره‌سازی‌ای که از ذخیره‌سازی کش مشتق شده است: `FindPaidPaymentByTransactionCodeAsync` حالا
`FindPaidPaymentByBankReferenceAsync` است.

## درگاه اختصاصی {#your-own-gateway}

درگاه حالا آنچه را بانک گفته گزارش می‌کند و پرباد درباره نتیجه تصمیم می‌گیرد:

* تأیید موفق `PaymentVerifyResult.Paid(transactionCode, bankReference, message)` است. پرباد هر شماره مرجع بانک را
  فقط برای یک فاکتور می‌پذیرد.
* پاسخ «قبلاً تأیید شده» که نشان نمی‌دهد به کدام پرداخت تعلق دارد `PaymentVerifyResult.AlreadyVerifiedAtBank(...)` است.
* اگر بانک مبلغ یا شماره سفارش را گزارش کند، `BankReportedAmount` و `BankReportedOrderId` را با یک عبارت `with`
  مقدار دهید؛ پرباد آن‌ها را با فاکتور مقایسه می‌کند.

[توسعه یک درگاه](./developing-a-gateway) یک درگاه کامل را نشان می‌دهد.

## تغییرات رفتاری {#behaviour-changes}

* زرین‌پال از آدرس‌های `payment.zarinpal.com` استفاده می‌کند، ایمیل و شماره موبایل مشتری را به صورت `metadata`
  می‌فرستد و از استرداد وجه پشتیبانی می‌کند.
