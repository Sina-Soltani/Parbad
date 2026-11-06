---
title: زرین‌پال
---

# زرین‌پال

```bash
dotnet add package Parbad.Gateway.ZarinPal
```

## پیکربندی

```csharp
.ConfigureGateways(gateways =>
{
    gateways
        .AddZarinPal()
        .WithAccounts(accounts =>
        {
            accounts.AddInMemory(account =>
            {
                account.MerchantId = "<Your merchant ID>";
                account.IsSandbox = false; // true: ZarinPal's sandbox

                // Only for refunds: the access token of your ZarinPal panel.
                account.AuthorizationToken = "<Your access token>";
            });
        });
})
```

* `IsSandbox`: با `true` از محیط آزمایشی (sandbox) زرین‌پال استفاده می‌شود.
* `AuthorizationToken`: توکن دسترسی پنل زرین‌پال؛ فقط برای استرداد وجه لازم است.

## درخواست پرداخت

زرین‌پال برای هر پرداخت یک توضیح لازم دارد:

```csharp
var result = await _onlinePayment.RequestAsync(invoice =>
{
    invoice
        .SetAmount(amount)
        .SetCallbackUrl(callbackUrl)
        .UseAutoIncrementTrackingNumber()
        .SetZarinPalData("Order 1234", email: "customer@example.com", mobile: "09121234567")
        .UseZarinPal();
});
```

## نتیجه تأیید

`verifyResult.GetZarinPalOriginalVerificationResult()` نتیجه تأیید خود زرین‌پال را برمی‌گرداند.

## استرداد وجه

استرداد وجه از GraphQL API زرین‌پال استفاده می‌کند و به `AuthorizationToken` حساب نیاز دارد. زرین‌پال برای استرداد وجه
محیط آزمایشی ندارد.

```csharp
var refundResult = await _onlinePayment.RefundCompletelyAsync(trackingNumber);
```
