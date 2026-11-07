---
sidebar_position: 10
title: مهاجرت از نسخه 3.x
source_hash: 0b5b6a0b05d7
---

# مهاجرت از نسخه 3.x به 4.0

:::caution پیش‌نویس
این صفحه طرح اولیه‌ای برای پیش‌نمایش سایت مستندات است.
:::

## پلتفرم‌ها

* پرباد برای ‎.NET Standard 2.0 و 2.1، ‎.NET 8، ‎.NET 9 و ‎.NET 10 ساخته می‌شود. `Parbad.Mvc5` و `Parbad.Owin` برای
  ‎.NET Framework 4.7.2 ساخته می‌شوند و به ASP.NET MVC 5.3 نیاز دارند.
* همه پکیج‌ها یک نسخه مشترک دارند: 4.0.0.

## APIهای حذف‌شده

هر چیزی که در 3.x با `[Obsolete]` علامت خورده بود حذف شده است؛ برای مثال:

| 3.x | 4.0 |
|---|---|
| `StaticOnlinePayment.Instance` | `StaticOnlinePayment.CreateScope()`، یک scope برای هر درخواست |
| `IStorageManager`، `StorageManager` | `IStorage` |
| `IStorage.Payments`، `IStorage.Transactions` | متدهای جستجوی `IStorage` |

## ذخیره‌سازی EF Core

* ذخیره‌سازی به EF Core 8 یا بالاتر نیاز دارد.
* جدول‌های پرباد حالا می‌توانند در `DbContext` خود شما باشند: صفحه [ذخیره‌سازی](./storage.md) را ببینید.
* جدول پرداخت سه ستون جدید دارد: `status`، `refunded_amount` و `bank_reference`. یک migration اضافه کنید. ردیف‌های
  موجود به مهاجرت داده نیاز ندارند.

## Money

تبدیل `Money` به `long` صریح (explicit) است و اگر مبلغ بخش اعشاری داشته باشد خطا می‌دهد.

## درگاه‌های اختصاصی

درگاه آنچه را بانک گفته گزارش می‌کند و پرباد درباره نتیجه تصمیم می‌گیرد. نتیجه‌های تأیید با
`PaymentVerifyResult.Paid(...)`، `PaymentVerifyResult.AlreadyVerifiedAtBank(...)` و `PaymentVerifyResult.Failed(...)`
ساخته می‌شوند.
