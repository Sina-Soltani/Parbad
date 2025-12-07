Tara Gateway for Parbad
=======================

This package provides integration with Tara payment gateway (https://pay.tara360.ir) for the Parbad payment library.

## How to use

### Installation
Install the package via NuGet:
```
dotnet add package Parbad.Gateway.Tara
```

### Configuration
Add Tara gateway to your services:

```csharp
services.AddParbad()
    .ConfigureGateways(gateways =>
    {
        gateways.AddTara()
            .WithAccounts(accounts =>
            {
                accounts.AddInMemory(account =>
                {
                    account.Username = "your-username";
                    account.Password = "your-password";
                    account.Ip = "your-ip-address";
                    account.IsTest = false; // Set to true for staging environment
                });
            });
    });
```

### Test/Staging Environment

Tara provides a staging environment for testing. To use it:

```csharp
accounts.AddInMemory(account =>
{
    account.Username = "test-username";
    account.Password = "test-password";
    account.Ip = "your-ip";
    account.IsTest = true;  // Automatically uses stage-pay.tara360.ir
});

// OR use the extension method:
accounts.AddInMemory(account =>
{
    account.Username = "test-username";
    account.Password = "test-password";
    account.Ip = "your-ip";
})
.UseTestMode();  // Automatically switches to staging
```

When `IsTest = true`, all API calls automatically use:
- **Production**: `https://pay.tara360.ir/pay`
- **Staging**: `https://stage-pay.tara360.ir/pay`

### Basic Usage
Use Tara gateway in your payment requests:

```csharp
var result = await _onlinePayment.RequestAsync(invoice => 
{
    invoice
        .SetAmount(10000)
        .SetCallbackUrl("https://your-site.com/callback")
        .UseTara();
});
```

### Advanced Usage - Tara-Specific Features

Tara gateway supports additional features like service amounts, invoice items, mobile number, VAT, and additional data.

#### Option 1: Using SetTaraData with configuration action
```csharp
var result = await _onlinePayment.RequestAsync(invoice => 
{
    invoice
        .SetAmount(10000)
        .SetCallbackUrl("https://your-site.com/callback")
        .SetTaraData(data => 
        {
            data.ServiceAmountList.Add(new TaraServiceAmount 
            { 
                serviceId = 1, 
                amount = 10000 
            });
            
            data.InvoiceItemList.Add(new TaraInvoiceItem 
            { 
                name = "Product 1",
                code = "P001",
                count = 1,
                unit = 1,
                fee = 10000,
                group = "Electronics",
                groupTitle = "Electronic Items"
            });
            
            data.Mobile = "09123456789";
            data.Vat = 900;
            data.AdditionalData = "Order #12345";
        })
        .UseTara();
});
```

#### Option 2: Using individual extension methods
```csharp
var result = await _onlinePayment.RequestAsync(invoice => 
{
    invoice
        .SetAmount(10000)
        .SetCallbackUrl("https://your-site.com/callback")
        .AddTaraServiceAmount(serviceId: 1, amount: 5000)
        .AddTaraServiceAmount(serviceId: 2, amount: 5000)
        .AddTaraInvoiceItem(new TaraInvoiceItem 
        { 
            name = "Product 1",
            code = "P001",
            count = 2,
            fee = 5000
        })
        .SetTaraMobile("09123456789")
        .SetTaraVat(900)
        .SetTaraAdditionalData("Order #12345")
        .UseTara();
});
```

#### Option 3: Adding multiple items at once
```csharp
var result = await _onlinePayment.RequestAsync(invoice => 
{
    var serviceAmounts = new List<TaraServiceAmount>
    {
        new TaraServiceAmount { serviceId = 1, amount = 7000 },
        new TaraServiceAmount { serviceId = 2, amount = 3000 }
    };

    var items = new List<TaraInvoiceItem>
    {
        new TaraInvoiceItem 
        { 
            name = "Product 1", 
            code = "P001", 
            count = 1, 
            fee = 7000 
        },
        new TaraInvoiceItem 
        { 
            name = "Product 2", 
            code = "P002", 
            count = 1, 
            fee = 3000 
        }
    };

    invoice
        .SetAmount(10000)
        .SetCallbackUrl("https://your-site.com/callback")
        .AddTaraServiceAmounts(serviceAmounts)
        .AddTaraInvoiceItems(items)
        .UseTara();
});
```

### Available Extension Methods

- **SetTaraData(TaraRequestAdditionalData)** - Set all Tara data at once
- **SetTaraData(Action<TaraRequestAdditionalData>)** - Configure with action
- **AddTaraServiceAmount(long serviceId, long amount)** - Add single service amount
- **AddTaraServiceAmounts(IEnumerable<TaraServiceAmount>)** - Add multiple service amounts
- **AddTaraInvoiceItem(TaraInvoiceItem)** - Add single invoice item
- **AddTaraInvoiceItems(IEnumerable<TaraInvoiceItem>)** - Add multiple invoice items
- **SetTaraMobile(string)** - Set customer mobile number
- **SetTaraAdditionalData(string)** - Set additional data string
- **SetTaraVat(long)** - Set VAT amount

### Note
If you don't provide service amounts, the gateway will automatically create a default service amount with serviceId = 1 and the invoice amount.

For more information about Parbad, visit: https://github.com/Sina-Soltani/Parbad

