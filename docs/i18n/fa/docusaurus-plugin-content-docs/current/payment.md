---
sidebar_position: 4
title: پرداخت
source_hash: 5c1e51d2579d
---

# پرداخت

هر پرداخت این مراحل را طی می‌کند: **درخواست** ← مشتری در بانک پرداخت می‌کند ← **دریافت** ← **تأیید** (یا **لغو**) ← بعدها و در صورت نیاز، **استرداد وجه**.

`IOnlinePayment` را هر جا که لازم دارید تزریق کنید. بدون تزریق وابستگی از `StaticOnlinePayment.CreateScope()`
استفاده کنید؛ [پیکربندی](./configuration#without-dependency-injection) را ببینید.

## درخواست پرداخت {#requesting-a-payment}

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

* **مبلغ:** مبلغ قابل پرداخت، با واحدی که درگاه انتظار دارد. پرباد هیچ‌وقت مبلغ را تبدیل نمی‌کند.
* **آدرس بازگشت (Callback URL):** یک آدرس کامل از وب‌سایت شما. بانک مشتری را به آن برمی‌گرداند.
* **شماره پیگیری:** یک شماره یکتا برای هر فاکتور. شماره خودتان را با `SetTrackingNumber(...)` بدهید یا انتخاب آن
  را به پرباد بسپارید؛ [فاکتورساز](./invoice-builder) را ببینید.
* **درگاه:** `UseMellat()`، `UseZarinPal()`، ... یا `SetGateway("Mellat")`. اطلاعاتی را که هر درگاه لازم دارد در
  [درگاه‌ها](/docs/gateways) ببینید.

یک شکل کوتاه برای فاکتورهای ساده:

```csharp
var result = await _onlinePayment.RequestAsync("Mellat", trackingNumber: 1001, amount: 250_000,
                                               callbackUrl: "https://shop.example/payment/verify");
```

نام درگاه‌ها: `AsanPardakht`، `FanAva`، `IdPay`، `IranKish`، `Mellat`، `Melli`، `ParbadVirtual`، `Parsian`،
`Pasargad`، `PayIr`، `PayPing`، `Saman`، `Sepehr`، `YekPay`، `ZarinPal`، `Zibal`. بزرگی و کوچکی حروف مهم نیست.

### فرستادن مشتری به بانک {#sending-the-customer-to-the-bank}

اگر درخواست موفق بود، `result.TrackingNumber` را همراه سفارش ذخیره کنید و مشتری را به بانک بفرستید.

در ASP.NET Core (`using Parbad.AspNetCore;`) و ASP.NET MVC 5 (`using Parbad.Mvc;`):

```csharp
if (result.IsSucceed)
{
    return result.GatewayTransporter.TransportToGateway();
}

// The request failed: result.Message says why.
```

در WebForms:

```csharp
await result.GatewayTransporter.TransportAsync();
```

### APIها و برنامه‌های سمت کاربر {#apis-and-client-side-applications}

یک API، برای مثال در پشت Angular یا React، به جای آن مشخصات انتقال را برای کلاینت می‌فرستد:

```csharp
if (result.IsSucceed)
{
    return Ok(result.GatewayTransporter.Descriptor);
}
```

Descriptor نوع انتقال (`Redirect` یا `Post`)، آدرس صفحه بانک و فیلدهای فرمی را که باید ارسال شوند در بر دارد. سپس
کلاینت مرورگر را منتقل می‌کند یا فرم را ارسال می‌کند.

## تأیید پرداخت {#verifying-a-payment}

بانک مشتری را با GET یا POST به آدرس بازگشت برمی‌گرداند. فاکتور را دریافت کنید، سفارش را بررسی کنید و سپس پرداخت
را تأیید کنید:

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

سفارش را **پیش از** `VerifyAsync` بررسی کنید: پس از تأیید پرداخت، پول برداشت شده است و بازگرداندن آن به استرداد
وجه نیاز دارد.

پرباد هر پرداخت را فقط یک بار تأیید می‌کند، حتی اگر صفحه دوباره بارگذاری شود یا دو سرور هم‌زمان همان پرداخت را
تأیید کنند. اگر بانک مبلغ یا شماره سفارش پرداخت را گزارش کند، پرباد آن‌ها را با فاکتور مقایسه می‌کند و هر شماره
مرجع بانک را فقط برای یک فاکتور می‌پذیرد.

## استرداد وجه {#refunding-a-payment}

```csharp
// The whole amount
var result = await _onlinePayment.RefundCompletelyAsync(trackingNumber);

// A part of it
var result = await _onlinePayment.RefundSpecificAmountAsync(trackingNumber, amount: 100_000);
```

چند استرداد جزئی تا سقف مبلغ پرداخت‌شده ممکن است. `Status` پرداخت ابتدا `PartiallyRefunded` و سپس `Refunded` می‌شود
و `RefundedAmount` مجموع مبالغ مسترد شده را نگه می‌دارد. همه درگاه‌ها از استرداد وجه پشتیبانی نمی‌کنند؛ برای
چنین درگاهی، نتیجه یک خطاست که همین را می‌گوید.
