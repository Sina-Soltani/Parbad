# Parbad.PaymentFlow.Tests

Behaviour tests for the payment flow. They run the real `IOnlinePayment` pipeline; only the bank is simulated.

```
dotnet test tests/Parbad.PaymentFlow.Tests
```

## What is covered

| Folder | Contents |
|---|---|
| `Core/` | `DefaultOnlinePayment` (request, fetch, verify, cancel, refund, concurrent verification) against a fake gateway, and `Money` comparisons. |
| `Gateways/` | The same 14 scenarios for each of the 16 gateways, plus a few gateway-specific tests. Each bank is simulated by `FakeBank` and the gateway's subclass. |
| `Storage/` | `MemoryCacheStorage` and `DistributedCacheStorage`: updates from detached copies, no shared references, unique tracking numbers and tokens, parallel creates, instances sharing one `IMemoryCache`. |
| `Infrastructure/` | The test host (DI container, thread-safe in-memory storage, replaceable HTTP context) and the `KnownIssue` helper. |

The gateway scenarios cover:
- request accepted, rejected, or answered with an HTML error page;
- paid, cancelled and bank-rejected payments;
- an HTML error page during verification;
- a lost verification response followed by a retry;
- servers running with `fa-IR` and `de-DE` cultures;
- a cheaper or same-amount payment of another invoice used to settle this one;
- full and partial refunds.

## Reading the results

Every test asserts the **correct** behaviour. The outcome tells you where the code stands:

| Outcome | Meaning |
|---|---|
| Passed | The behaviour is correct. |
| Skipped: `KNOWN ISSUE <issue> reproduces: …` | A known bug still exists. `<issue>` describes it. The build stays green. |
| Skipped: `Not applicable …` | The scenario does not apply to this gateway (e.g. no refund support). |
| Failed: `Known issue <issue> no longer reproduces` | The bug has been fixed. Remove its entry from `KnownIssues` so the test guards the fix from now on. |
| Failed: anything else | A regression, or a problem in a simulator (simulator errors and unexpected HTTP calls are reported by the test cleanup). |

All scenarios run under the invariant culture, so the results do not depend on the machine's settings; the
culture scenarios switch culture explicitly.

## Adding a gateway

Subclass `GatewayConformanceTests` and implement:

- `GatewayName` and `SupportsRefund`;
- `AddGateway`: register the gateway with an account and options that point to `BankUrl`;
- `UseGateway`: select the gateway on the invoice;
- `SetupBank`: answer the gateway's HTTP calls from `Bank`, through `Bank.Endpoint(...)` so faults are applied;
- `CreateCallback`: the callback the bank sends for a transaction.

List failing scenarios in `KnownIssues` with a description of a known bug, never to silence an unexplained failure.

## Limits of the simulation

- **The simulators follow the bank APIs as Parbad implements them.** Where a bank's real behaviour is unknown
  (for example, how it answers a repeated verification), the simulator says so in a comment starting with
  `Assumption (unverified)`, and assumes the answer most favourable to Parbad. Search for that text before
  relying on a passing result.
- **Some problems cannot be tested here:**
  - AsanPardakht's 12-hour timestamp only partly: the gateway reads `DateTime.Now` directly, so the
    simulator (which checks the timestamp against the clock) catches a 12-hour clock only between 12:00 and
    00:59.
- **MockHttp 6 swallows exceptions from asynchronous response handlers,** so the request then hangs. Simulators
  must not throw: `FakeBank.Endpoint` records simulator errors instead, and a lost response is simulated by a
  body that fails while it is read.
