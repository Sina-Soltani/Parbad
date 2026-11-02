using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Parbad.PaymentFlow.Tests.Infrastructure;

/// <summary>
/// An <see cref="IHttpContextAccessor"/> whose current request can be replaced between the steps of a
/// payment, e.g. the checkout request and the bank's callback request.
/// </summary>
public sealed class TestHttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; } = CreateRequest("GET", "https://shop.test/checkout");

    public static HttpContext CreateRequest(
        string method,
        string url,
        IEnumerable<KeyValuePair<string, string>>? form = null,
        IServiceProvider? requestServices = null)
    {
        var uri = new Uri(url);
        var context = new DefaultHttpContext();
        var request = context.Request;

        request.Method = method;
        request.Scheme = uri.Scheme;
        request.Host = new HostString(uri.Authority);
        request.Path = uri.AbsolutePath;
        request.QueryString = new QueryString(uri.Query);

        if (form != null)
        {
            request.ContentType = "application/x-www-form-urlencoded";
            request.Form = new FormCollection(form
                                              .GroupBy(pair => pair.Key)
                                              .ToDictionary(group => group.Key, group => new StringValues(group.Select(pair => pair.Value).ToArray())));
        }

        if (requestServices != null) context.RequestServices = requestServices;

        return context;
    }
}
