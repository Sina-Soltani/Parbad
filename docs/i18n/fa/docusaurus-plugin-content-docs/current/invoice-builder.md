---
sidebar_position: 5
title: فاکتورساز (Invoice Builder)
source_hash: b29e1912cd38
---

# فاکتورساز (Invoice Builder)

`RequestAsync(invoice => ...)` فاکتور را با فاکتورساز (Invoice Builder) توصیف می‌کند:

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

## شماره پیگیری {#tracking-numbers}

هر فاکتور یک شماره پیگیری یکتا لازم دارد. شماره خودتان را با `SetTrackingNumber(...)` بدهید یا انتخاب آن را به
پرباد بسپارید:

```csharp
// One more than the largest tracking number in the storage.
invoice.UseAutoIncrementTrackingNumber();

// A random number that is not in the storage yet.
invoice.UseAutoRandomTrackingNumber();
```

حداقل مقدار آن‌ها (و میزان افزایش) با `ConfigureAutoIncrementTrackingNumber` و `ConfigureAutoRandomTrackingNumber`
تنظیم می‌شود؛ [پیکربندی](./configuration#tracking-numbers) را ببینید.

## انتخاب درگاه {#choosing-the-gateway}

```csharp
invoice.UseMellat();

// or by name, e.g. from your settings (not case-sensitive)
invoice.SetGateway("Mellat");
```

بعضی درگاه‌ها اطلاعات بیشتری می‌گیرند، مانند شماره موبایل مشتری یا یک توضیح؛ [درگاه‌ها](/docs/gateways) را ببینید.

## انتخاب حساب {#choosing-the-account}

اگر یک درگاه چند حساب دارد، یکی را با نامش انتخاب کنید:

```csharp
invoice.UseAccount("Donations");
```

[چند حساب برای یک درگاه](./configuration#several-accounts-for-one-gateway) را ببینید.

## ویژگی‌ها {#properties}

یک فاکتور می‌تواند مقدارهای دلخواه شما را همراه داشته باشد که درگاه یا یک formatter می‌تواند آن‌ها را بخواند:

```csharp
invoice.AddOrUpdateProperty("OrderId", orderId);
```

## Formatterها {#formatters}

یک formatter فاکتور را پیش از ارسال به درگاه تغییر می‌دهد، برای مثال تا یک قاعده را روی همه فاکتورها اعمال کند:

```csharp
invoice.AddFormatter(invoice => invoice.CallbackUrl = new CallbackUrl("https://shop.example/payment/verify"));
```
