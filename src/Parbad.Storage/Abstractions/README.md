# Parbad.Storage.Abstractions

The storage contract of [Parbad](https://www.nuget.org/packages/Parbad/), the online payment library for .NET: the
`Payment` and `Transaction` models and the `IStorage` interface, which saves and loads them.

You need this package only to write your own storage. Parbad provides
[Parbad.Storage.EntityFrameworkCore](https://www.nuget.org/packages/Parbad.Storage.EntityFrameworkCore/) and
[Parbad.Storage.Cache](https://www.nuget.org/packages/Parbad.Storage.Cache/).

## Your own storage

Implement `IStorage` and register it:

```csharp
builder.Services.AddParbad()
       .ConfigureStorage(storage => storage.AddStorage<MyStorage>(ServiceLifetime.Transient));
```

Two members protect payment verification:

* `TryCompletePaymentAsync` completes a payment in one atomic step, only if the stored payment allows it (a paid
  payment is never changed), and reports whether it did. When two verifications of the same payment run at the same
  time, for example on two servers, the slower one must not overwrite the result of the faster one.
* `FindPaidPaymentByBankReferenceAsync` finds a paid payment by the bank's reference, so that one bank payment cannot
  settle two invoices.

The XML documentation of `IStorage` describes every member.

## More information

* [GitHub](https://github.com/Sina-Soltani/Parbad)
* [Documentation](https://sina-soltani.github.io/Parbad/docs/storage#your-own-storage)
