---
sidebar_position: 9
title: توسعه یک درگاه
source_hash: 2d4b94d05ac4
---

# توسعه یک درگاه

این صفحه برای یک ارائه‌دهنده پرداخت فرضی به نام **Hero** یک درگاه می‌سازد، به صورت یک پکیج جداگانه. پکیج‌های درگاه
پرباد، مانند [Parbad.Gateway.Zibal](https://github.com/Sina-Soltani/Parbad/tree/master/src/Parbad.Gateway/Zibal/src)،
نمونه‌های کاملی هستند.

## یک درگاه چه می‌کند {#what-a-gateway-does}

یک درگاه چهار متد را پیاده‌سازی می‌کند. پرباد آن‌ها را صدا می‌زند و همه کارهای پیرامون آن‌ها را انجام می‌دهد:
پرداخت‌ها را ذخیره می‌کند، شماره پیگیری می‌دهد، از تأیید دوباره جلوگیری می‌کند و درباره نتیجه تصمیم می‌گیرد.

| متد | چه زمانی | درگاه چه می‌کند |
|---|---|---|
| `RequestAsync` | برنامه یک پرداخت درخواست می‌کند. | پرداخت را در بانک ثبت می‌کند و می‌گوید مشتری چگونه به آنجا فرستاده شود. |
| `FetchAsync` | مشتری به آدرس بازگشت برمی‌گردد. | آنچه را بانک همراه مشتری فرستاده می‌خواند: آیا مشتری پرداخت کرده است؟ |
| `VerifyAsync` | برنامه پرداخت را تأیید می‌کند. | از بانک می‌خواهد پرداخت را تأیید کند و پاسخ بانک را گزارش می‌کند. |
| `RefundAsync` | برنامه وجه یک پرداخت را مسترد می‌کند. | اگر بانک پشتیبانی کند، از بانک استرداد وجه را می‌خواهد. |

درگاه **آنچه را بانک گفته گزارش می‌کند**؛ پرباد تصمیم می‌گیرد که این برای پرداخت چه معنایی دارد. برای مثال، پرباد
هر شماره مرجع بانک را فقط برای یک فاکتور می‌پذیرد و مبلغی را که بانک گزارش می‌کند با فاکتور مقایسه می‌کند.

## پروژه {#the-project}

یک class library برای ‎.NET Standard 2.0 یا بالاتر بسازید، برای مثال با نام `Hero.Parbad.Gateway`، و پرباد را به
آن اضافه کنید:

```bash
dotnet add package Parbad
```

## حساب {#the-account}

تنظیماتی که هر برنامه برای هر یک از حساب‌هایش نزد ارائه‌دهنده می‌دهد:

```csharp
public class HeroGatewayAccount : GatewayAccount
{
    public string MerchantId { get; set; } = null!;
}
```

## درگاه {#the-gateway}

```csharp
using Microsoft.AspNetCore.Http;
using Parbad;
using Parbad.Abstraction;
using Parbad.GatewayBuilders;
using Parbad.Http;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using Parbad.Net;
using Parbad.Storage.Abstractions.Models;

[Gateway(Name)]
public class HeroGateway : GatewayBase<HeroGatewayAccount>
{
    public const string Name = "Hero";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _httpClient;

    public HeroGateway(IGatewayAccountProvider<HeroGatewayAccount> accountProvider,
                       IHttpContextAccessor httpContextAccessor,
                       IHttpClientFactory httpClientFactory) : base(accountProvider)
    {
        _httpContextAccessor = httpContextAccessor;

        // The client of this gateway, with the timeout and proxy the application configured for it.
        _httpClient = httpClientFactory.CreateClient(this);
    }
}
```

نام داخل `[Gateway(...)]` همان نامی است که برنامه‌ها با `SetGateway("Hero")` به کار می‌برند.

### درخواست {#request}

```csharp
public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice, CancellationToken cancellationToken = default)
{
    var account = await GetAccountAsync(invoice);

    // Register the payment with the bank, e.g. with _httpClient, and get its token.
    var token = await RegisterPaymentAsync(account, invoice.TrackingNumber, invoice.Amount, invoice.CallbackUrl, cancellationToken);

    if (token == null)
    {
        return PaymentRequestResult.Failed("The bank rejected the payment.", account.Name);
    }

    // Send the customer to the bank's page, with a redirect...
    var result = PaymentRequestResult.SucceedWithRedirect(account.Name,
                                                          _httpContextAccessor.GetRequiredHttpContext(),
                                                          $"https://pay.hero.example/{token}");

    // ...or by posting a form:
    // PaymentRequestResult.SucceedWithPost(account.Name, httpContext, url, new Dictionary<string, string> { ... });

    // Data for the later steps. Parbad stores it with the request transaction.
    result.DatabaseAdditionalData.Add("HeroToken", token);

    return result;
}
```

### دریافت {#fetch}

```csharp
public override Task<IPaymentFetchResult> FetchAsync(InvoiceContext context, CancellationToken cancellationToken = default)
{
    var request = _httpContextAccessor.GetRequiredHttpContext().Request;

    // What the bank sent with the customer (in the query string or the form, depending on the bank).
    var status = request.Query["status"];

    var result = status == "OK"
                     ? PaymentFetchResult.ReadyForVerifying()
                     : PaymentFetchResult.Failed("The customer cancelled the payment.");

    return Task.FromResult<IPaymentFetchResult>(result);
}
```

### تأیید {#verify}

```csharp
public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
{
    var account = await GetAccountAsync(context.Payment);

    // The data stored by RequestAsync.
    var token = context.Transactions
                       .Single(transaction => transaction.Type == TransactionType.Request)
                       .ToDictionary()["HeroToken"];

    var answer = await VerifyAtBankAsync(account, token, cancellationToken);

    if (!answer.IsPaid)
    {
        return PaymentVerifyResult.Failed(answer.Message, answer.Code);
    }

    // transactionCode: the code the customer sees (e.g. the RRN). bankReference: what the bank verified (here the
    // token). The amount and the order ID that the bank reports let Parbad check them against the invoice.
    return PaymentVerifyResult.Paid(answer.Rrn, token, "The payment was successful.") with
           {
               BankReportedAmount = answer.Amount,
               BankReportedOrderId = answer.OrderId
           };
}
```

اگر بانک پاسخ «قبلاً تأیید شده» بدهد بدون اینکه نشان دهد منظورش کدام پرداخت است،
`PaymentVerifyResult.AlreadyVerifiedAtBank(...)` را برگردانید: در این صورت پرباد پرداخت را فقط وقتی پرداخت‌شده ثبت
می‌کند که هیچ پرداخت دیگری همان شماره مرجع بانک را نداشته باشد.

اگر دسترسی به بانک ممکن نبود، اجازه دهید exception بالا برود: پرباد پرداخت را باز نگه می‌دارد تا بتوان دوباره آن را
تأیید کرد.

### استرداد وجه {#refund}

```csharp
public override Task<IPaymentRefundResult> RefundAsync(InvoiceContext context, Money amount, CancellationToken cancellationToken = default)
{
    // A bank without refunds:
    return Task.FromResult<IPaymentRefundResult>(PaymentRefundResult.Failed("Hero does not support refunds."));
}
```

## متدهای الحاقی {#extension-methods}

تا برنامه‌ها بتوانند `AddHero()`، `WithAccounts(...)` و `UseHero()` را بنویسند:

```csharp
public static class HeroGatewayExtensions
{
    public static IGatewayConfigurationBuilder<HeroGateway> AddHero(this IGatewayBuilder builder) =>
        builder.AddGateway<HeroGateway>()
               .WithHttpClient(client => { });

    public static IGatewayConfigurationBuilder<HeroGateway> WithAccounts(
        this IGatewayConfigurationBuilder<HeroGateway> builder,
        Action<IGatewayAccountBuilder<HeroGatewayAccount>> configureAccounts) =>
        builder.WithAccounts(configureAccounts);

    public static IInvoiceBuilder UseHero(this IInvoiceBuilder builder) =>
        builder.SetGateway(HeroGateway.Name);
}
```

## انتشار {#publishing}

مشخصات پکیج را به پروژه بدهید (شناسه، توضیح، نویسندگان، مجوز، یک README) و سپس:

```bash
dotnet pack -c Release
```

و پکیج را در nuget.org منتشر کنید.
