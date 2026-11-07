# Parbad.Gateway.ZarinPal

The ZarinPal gateway for [Parbad](https://www.nuget.org/packages/Parbad/), the online payment library for .NET.

## Configuration

```csharp
builder.Services.AddParbad()
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
       // ...
```

## Payment request

```csharp
var result = await onlinePayment.RequestAsync(invoice =>
{
    invoice
        .SetAmount(amount)
        .SetCallbackUrl(callbackUrl)
        .UseAutoIncrementTrackingNumber()
        .SetZarinPalData("Order 1234", email: "customer@example.com", mobile: "09121234567") // The description is required
        .UseZarinPal();
});
```

## Verification result

`verifyResult.GetZarinPalOriginalVerificationResult()` returns ZarinPal's own verification result.

## Refunds

Refunds use ZarinPal's GraphQL API and need the `AuthorizationToken` of the account. ZarinPal has no sandbox for
refunds.

```csharp
var refundResult = await onlinePayment.RefundCompletelyAsync(trackingNumber);
```

## More information

* [GitHub](https://github.com/Sina-Soltani/Parbad)
* [Documentation](https://sina-soltani.github.io/Parbad/docs/gateways/zarinpal)
