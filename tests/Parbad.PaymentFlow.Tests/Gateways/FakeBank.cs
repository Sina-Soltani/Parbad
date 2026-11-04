using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Parbad.PaymentFlow.Tests.Gateways;

public enum BankOperation
{
    Request,
    Verify,
    Refund,

    /// <summary>Helper services (e.g. a remote encryption service); faults are never applied to them.</summary>
    Other
}

public enum BankFault
{
    None,

    /// <summary>The bank answers with its own error code (e.g. invalid terminal, transaction not valid).</summary>
    Reject,

    /// <summary>A proxy or the bank answers with an HTML error page (HTTP 502) instead of the API response.</summary>
    HtmlErrorPage,

    /// <summary>The bank answers HTTP 500 with a plain-text body instead of the API response.</summary>
    ServerError,

    /// <summary>The bank processes the first call, but the response never reaches the merchant.</summary>
    LoseResponseOnce
}

/// <summary>
/// One transaction as the bank sees it.
/// </summary>
public sealed class BankTransaction
{
    internal BankTransaction(int sequence, long orderId, decimal amount, string? callbackUrl)
    {
        Token = (7_000_000_000L + sequence).ToString(CultureInfo.InvariantCulture);
        Reference = (8_000_000_000L + sequence).ToString(CultureInfo.InvariantCulture);
        Rrn = (100_000_000_000L + sequence).ToString(CultureInfo.InvariantCulture);
        TraceNumber = (500_000 + sequence).ToString(CultureInfo.InvariantCulture);
        OrderId = orderId;
        Amount = amount;
        CallbackUrl = callbackUrl;
    }

    /// <summary>Identifier the bank issues at request time (token, authority, RefId, ...).</summary>
    public string Token { get; }

    /// <summary>Reference the bank issues after the payment (RefNum, SaleReferenceId, receipt, ...).</summary>
    public string Reference { get; }

    public string Rrn { get; }

    public string TraceNumber { get; }

    public string MaskedCardNumber => "603799******1234";

    /// <summary>Order id sent by the merchant at request time (0 when the bank API has none).</summary>
    public long OrderId { get; }

    /// <summary>Amount sent by the merchant at request time, in the bank's unit.</summary>
    public decimal Amount { get; }

    /// <summary>Callback URL sent by the merchant at request time.</summary>
    public string? CallbackUrl { get; }

    public bool Paid { get; set; }

    public bool Verified { get; set; }

    public decimal RefundedAmount { get; set; }

    /// <summary>Free-form data a simulator needs to remember between calls.</summary>
    public IDictionary<string, string> Data { get; } = new Dictionary<string, string>();
}

/// <summary>
/// What arrived at a simulated bank endpoint.
/// </summary>
public sealed class BankCall
{
    private JObject? _json;
    private XDocument? _xml;
    private Dictionary<string, string>? _form;

    internal BankCall(HttpRequestMessage request, string body, bool reject)
    {
        Request = request;
        Body = body;
        Reject = reject;
    }

    public HttpRequestMessage Request { get; }

    public string Body { get; }

    /// <summary>True when the test asked this endpoint to answer with the bank's error code.</summary>
    public bool Reject { get; }

    public JObject Json => _json ??= JObject.Parse(Body);

    public string? Header(string name) =>
        Request.Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;

    /// <summary>Value of the first XML element with the given local name (namespaces ignored).</summary>
    public string? Xml(string localName) =>
        (_xml ??= XDocument.Parse(Body)).Descendants().FirstOrDefault(element => element.Name.LocalName == localName)?.Value;

    public string? Form(string name)
    {
        _form ??= QueryHelpers.ParseQuery(Body).ToDictionary(pair => pair.Key, pair => pair.Value.ToString());

        return _form.TryGetValue(name, out var value) ? value : null;
    }

    public string? Query(string name) =>
        QueryHelpers.ParseQuery(Request.RequestUri!.Query).TryGetValue(name, out var value) ? value.ToString() : null;
}

/// <summary>
/// The state of a simulated bank. Gateway-specific simulators read and update it while they answer the
/// merchant's HTTP calls, the way the real bank would.
/// </summary>
public sealed class FakeBank
{
    private readonly object _sync = new();
    private readonly List<BankTransaction> _transactions = new();
    private readonly HashSet<BankOperation> _lostResponses = new();

    public BankFault RequestFault { get; set; }

    public BankFault VerifyFault { get; set; }

    public IReadOnlyList<BankTransaction> Transactions
    {
        get
        {
            lock (_sync) return _transactions.ToList();
        }
    }

    public BankTransaction Open(long orderId, decimal amount, string? callbackUrl)
    {
        lock (_sync)
        {
            var transaction = new BankTransaction(_transactions.Count + 1, orderId, amount, callbackUrl);
            _transactions.Add(transaction);

            return transaction;
        }
    }

    public BankTransaction? FindByToken(string? token) => Find(transaction => transaction.Token == token);

    public BankTransaction? FindByReference(string? reference) => Find(transaction => transaction.Reference == reference);

    public BankTransaction? FindByOrderId(long orderId) => Find(transaction => transaction.OrderId == orderId);

    public BankTransaction? Find(Func<BankTransaction, bool> predicate)
    {
        lock (_sync) return _transactions.LastOrDefault(predicate);
    }

    /// <summary>
    /// Exceptions thrown by a simulator itself. A non-empty list means the test setup is wrong, not Parbad.
    /// </summary>
    public IReadOnlyList<Exception> SimulatorErrors
    {
        get
        {
            lock (_sync) return _simulatorErrors.ToList();
        }
    }

    private readonly List<Exception> _simulatorErrors = new();

    /// <summary>
    /// Creates a MockHttp response handler for an endpoint, applying the configured fault for its operation.
    /// </summary>
    /// <remarks>
    /// The handler never throws: MockHttp 6 swallows exceptions from asynchronous handlers and the request then
    /// hangs until the HttpClient timeout. Simulator errors are recorded in <see cref="SimulatorErrors"/> instead.
    /// </remarks>
    public Func<HttpRequestMessage, Task<HttpResponseMessage>> Endpoint(BankOperation operation, Func<BankCall, HttpResponseMessage> answer)
    {
        return async request =>
        {
            var body = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync();

            var fault = operation switch
            {
                BankOperation.Request => RequestFault,
                BankOperation.Verify => VerifyFault,
                _ => BankFault.None
            };

            if (fault == BankFault.HtmlErrorPage) return HtmlErrorPage();
            if (fault == BankFault.ServerError) return Text("Internal Server Error", "text/plain", HttpStatusCode.InternalServerError);

            lock (_sync)
            {
                HttpResponseMessage response;

                try
                {
                    response = answer(new BankCall(request, body, fault == BankFault.Reject));
                }
                catch (Exception exception)
                {
                    _simulatorErrors.Add(exception);

                    return Text($"Simulator error: {exception.Message}", "text/plain", HttpStatusCode.InternalServerError);
                }

                if (fault == BankFault.LoseResponseOnce && _lostResponses.Add(operation))
                {
                    // The bank has processed the call; the connection drops while the response is being read.
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new BrokenContent() };
                }

                return response;
            }
        };
    }

    private sealed class BrokenContent : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            Task.FromException(new IOException("The connection was reset by the peer (simulated)."));

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    public static HttpResponseMessage Json(object body, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json") };

    public static HttpResponseMessage Text(string body, string mediaType, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(body, Encoding.UTF8, mediaType) };

    public static HttpResponseMessage HtmlErrorPage() =>
        Text("<html><head><title>502 Bad Gateway</title></head><body><h1>502 Bad Gateway</h1></body></html>", "text/html", HttpStatusCode.BadGateway);

    public static decimal ParseAmount(string? value) =>
        decimal.Parse(value ?? throw new InvalidOperationException("The bank received no amount."), NumberStyles.Number, CultureInfo.InvariantCulture);
}
