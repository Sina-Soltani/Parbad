---
title: ZarinPal
---

# ZarinPal

```bash
dotnet add package Parbad.Gateway.ZarinPal
```

## Configuration

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

## Payment request

ZarinPal needs a description for every payment:

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

## Verification result

`verifyResult.GetZarinPalOriginalVerificationResult()` returns ZarinPal's own verification result.

## Refunds

Refunds use ZarinPal's GraphQL API and need the `AuthorizationToken` of the account. ZarinPal has no sandbox for
refunds.

```csharp
var refundResult = await _onlinePayment.RefundCompletelyAsync(trackingNumber);
```
