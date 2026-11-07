---
title: "Gateways specific functions"
slug: /gateways-specific-functions
---

# Gateways specific functions

Here you can see the specific functions of each gateway.

Gateways:
* [Mellat](#mellat)
* [Parsian](#parsian)
* [ZarinPal](#zarinpal)
* [Pay.ir](#payir)
* [Sepehr](#sepehr)
* [YekPay](#yekpay)
* [PayPing](#payping)
* [Zibal](#zibal)
* [IdPay](#id-pay)

## Mellat

#### Set additional data
Sample:
```csharp
var result = _onlinePayment.Request(invoice =>
{
   invoice
       //other invoice info...
       .SetMellatAdditionalData(...)

       .SetGateway("Mellat")
       // OR
       .UseMellat();
});
```

#### Cumulative Dynamic Payment Request

Use the [Invoice Builder](./invoice-builder.md) to create an invoice and then use the _AddMellatCumulativeAccount_ method for using Cumulative Dynamic Payment Request.

Sample:
```csharp
var result = _onlinePayment.Request(invoice =>
{
   invoice
       //other info...
       .SetAmount(50000)
       .AddMellatCumulativeAccount(<SUB SERVICE ID>, <AMOUNT>);

       .SetGateway("Mellat")
       // OR
       .UseMellat();
});
```

**Note**: Maximum 10 accounts can be added.

**Note**: By using the `AddMellatCumulativeAccount` method, you don't need to specify the Mellat Gateway by using the _UseGateway_ method. It will be done for you automatically.


## Parsian

#### Set AdditionalData and Originator
Sample:
```csharp
var result = _onlinePayment.Request(invoice =>
{
   invoice
       //other invoice info...
       .SetParsianAdditionalData(new ParsianGatewayAdditionalDataRequest
       {
          AdditionalData = "",
          Originator = ""
       })

       .SetGateway("Parsian")
       // OR
       .UseParsian();
});
```

## ZarinPal

#### Configuration

```c#
.ConfigureGateways(gateways =>
{
    gateways
        .AddZarinPal()
        .WithAccounts(accounts =>
        {
            accounts.AddInMemory(account =>
            {
                account.MerchantId = "<Your ID>";
                account.IsSandbox = true | false;
            });
        });
})
```
Note: Set a random value for the `MerchantId` such as `test` if you want to work with the Sandbox option enabled.


#### Making a request

```c#
var result = _onlinePayment.RequestAsync(invoice => 
{
   invoice
      // other invoice info
      .SetZarinPalData("<Description>");

      .SetGateway("ZarinPal")
      // OR
      .UseZarinPal();
})
```

## Pay.ir

#### Set Request Additional Data
```c#
var result = _onlinePayment.RequestAsync(invoice => 
{
   invoice
      .SetPayIrAdditionalData(...);
})
```

#### Getting the additional data
To get the FactorNumber, Mobile, Description and CardNumber do like so:

```c#
var verifyResult = await _onlinePayment.VerifyAsync(invoice);

var additionalData = verifyResult.GetPayIrAdditionalData();
```

## Sepehr
#### Getting the additional data
To get the TraceNumber, DigitalReceipt, RRN and CardNumber do like so:

```c#
var verifyResult = await _onlinePayment.VerifyAsync(invoice);

var additionalData = verifyResult.GetSepehrAdditionalData();
```

## YekPay
#### Using the YekPay Gateway
```csharp
var result = _onlinePayment.Request(invoice =>
{
    invoice
          .SetYekPayData(yekPay => 
          {
             // payment details go here...
          })

          .SetGateway("YekPay")
          // OR
          .UseYekPay();
});
```

## PayPing
#### Using the PayPing Gateway
```csharp
var result = _onlinePayment.Request(invoice =>
{
    invoice
          .SetPayPingData(payPing => 
          {
             // optional
          });

          .SetGateway("PayPing")
          // OR
          .UsePayPing();
});
```

## Zibal
#### Using the Zibal Gateway
```csharp
var result = _onlinePayment.Request(invoice =>
{
    invoice
          .SetZibalData(new ZibalRequest
          {
             // optional Zibal data
          })

          .SetGateway("Zibal")
          // OR
          .UseZibal();
});
```

## ID Pay
#### Using the ID Pay Gateway
```csharp
var result = _onlinePayment.Request(invoice =>
{
    invoice
          .SetIdPayData(new ...
          {
             // optional data
          })

          .SetGateway("IdPay")
          // OR
          .UseIdPay();
});
```
