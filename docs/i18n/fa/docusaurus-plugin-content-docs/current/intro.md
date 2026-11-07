---
sidebar_position: 1
title: معرفی
slug: /intro
source_hash: 46daf78aa91d
---

# پَرباد

پرباد یک کتابخانه رایگان و متن‌باز پرداخت آنلاین برای ‎.NET است. پرباد برنامه‌های وب را به درگاه‌های پرداخت
آنلاین ایرانی وصل می‌کند: پرداخت را درخواست می‌کند، مشتری را به بانک می‌فرستد، پس از بازگشت مشتری پرداخت را تأیید
می‌کند و در صورت نیاز آن را بازمی‌گرداند.

## روند کار

![روند کار پرباد](/img/how-it-works.png)

1. برنامه شما از پرباد یک پرداخت درخواست می‌کند (یک *فاکتور*: مبلغ، آدرس بازگشت و درگاه).
2. پرباد پرداخت را در بانک ثبت می‌کند و مشتری را به صفحه پرداخت بانک می‌فرستد.
3. بانک مشتری را به آدرس بازگشت برنامه شما برمی‌گرداند.
4. پرباد اطلاعات پرداخت را دریافت می‌کند و شما سفارش را بررسی می‌کنید (موجودی، قیمت و ...).
5. پرباد پرداخت را در بانک تأیید می‌کند. فقط در این مرحله پول واقعاً برداشت می‌شود.

پرباد وضعیت هر پرداخت را نگه می‌دارد؛ بنابراین هر پرداخت فقط یک بار تأیید می‌شود، حتی اگر صفحه بازگشت دوباره
بارگذاری شود یا همان پرداخت هم‌زمان روی دو سرور تأیید شود.

## امکانات

* رایگان و متن‌باز
* یک API برای همه درگاه‌ها
* درگاه مجازی: یک بانک شبیه‌سازی‌شده برای توسعه و تست
* پشتیبانی از ASP.NET Core، ASP.NET MVC 5 و WebForms
* ذخیره اطلاعات در پایگاه داده خودتان با EF Core، در کش، یا با پیاده‌سازی خودتان

## درگاه‌ها {#gateways}

| درگاه | پکیج |
|---|---|
| ملت، ملی، سامان، پاسارگاد، پارسیان، آسان پرداخت | [Parbad](https://www.nuget.org/packages/Parbad/) |
| ایران کیش | [Parbad.Gateway.IranKish](https://www.nuget.org/packages/Parbad.Gateway.IranKish/) |
| سپهر | [Parbad.Gateway.Sepehr](https://www.nuget.org/packages/Parbad.Gateway.Sepehr/) |
| زرین‌پال | [Parbad.Gateway.ZarinPal](https://www.nuget.org/packages/Parbad.Gateway.ZarinPal/) |
| Pay.ir | [Parbad.Gateway.PayIr](https://www.nuget.org/packages/Parbad.Gateway.PayIr/) |
| آیدی‌پی | [Parbad.Gateway.IdPay](https://www.nuget.org/packages/Parbad.Gateway.IdPay/) |
| یک‌پی | [Parbad.Gateway.YekPay](https://www.nuget.org/packages/Parbad.Gateway.YekPay/) |
| پی‌پینگ | [Parbad.Gateway.PayPing](https://www.nuget.org/packages/Parbad.Gateway.PayPing/) |
| زیبال | [Parbad.Gateway.Zibal](https://www.nuget.org/packages/Parbad.Gateway.Zibal/) |
| فن‌آوا | [Parbad.Gateway.FanAva](https://www.nuget.org/packages/Parbad.Gateway.FanAva/) |

مرحله بعد: [شروع کار](./getting-started).
