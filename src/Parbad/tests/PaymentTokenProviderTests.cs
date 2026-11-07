using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Parbad.Abstraction;
using Parbad.Internal;
using Parbad.PaymentTokenProviders;
using Parbad.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Parbad.Tests;

[TestFixture]
public class PaymentTokenProviderTests
{
    private IHttpContextAccessor _httpContextAccessor;
    private IOptions<QueryStringPaymentTokenOptions> _options;
    private IPaymentTokenProvider _provider;
    private Invoice _invoice;

    [SetUp]
    public void Setup()
    {
        _invoice = new Invoice { CallbackUrl = CallbackUrl.Parse("http://www.mysite.com") };

        _httpContextAccessor = MockHelpers.MockHttpContextAccessor();

        _options = new OptionsWrapper<QueryStringPaymentTokenOptions>(new QueryStringPaymentTokenOptions());

        _provider = new GuidQueryStringPaymentTokenProvider(_httpContextAccessor, _options);
    }

    [Test]
    public async Task Token_Must_Not_Be_Null()
    {
        var token = await _provider.ProvideTokenAsync(_invoice);

        Assert.That(token, Is.Not.Null);
    }

    [Test]
    public async Task Token_Must_Be_Guid()
    {
        var token = await _provider.ProvideTokenAsync(_invoice);

        Assert.That(Guid.TryParse(token, out _), Is.True);
    }

    [Test]
    public async Task CallbackUrl_Must_Have_Default_TokenName()
    {
        await _provider.ProvideTokenAsync(_invoice);

        var uri = new Uri(_invoice.CallbackUrl.Url);

        var query = QueryHelpers.ParseQuery(uri.Query);

        Assert.That(query.ContainsKey(QueryStringPaymentTokenOptions.DefaultQueryName), Is.True);
    }

    [Test]
    public async Task CallbackUrl_Must_Have_TokenName_From_Options()
    {
        const string expectedQueryName = "test";

        _options.Value.QueryName = expectedQueryName;

        await _provider.ProvideTokenAsync(_invoice);

        var uri = new Uri(_invoice.CallbackUrl.Url);

        var query = QueryHelpers.ParseQuery(uri.Query);

        Assert.That(query.ContainsKey(expectedQueryName), Is.True);
    }

    [Test]
    public async Task CallbackUrl_Must_Have_Correct_TokenValue()
    {
        var token = await _provider.ProvideTokenAsync(_invoice);

        var uri = new Uri(_invoice.CallbackUrl.Url);

        var query = QueryHelpers.ParseQuery(uri.Query);

        Assert.That(query.ContainsKey(QueryStringPaymentTokenOptions.DefaultQueryName), Is.True);

        Assert.That((string)query[QueryStringPaymentTokenOptions.DefaultQueryName], Is.EqualTo(token));
    }

    [Test]
    public async Task Must_Provide_Unique_Tokens()
    {
        var token1 = await _provider.ProvideTokenAsync(_invoice);
        var token2 = await _provider.ProvideTokenAsync(_invoice);

        Assert.That(token1, Is.Not.EqualTo(token2));
    }

    [Test]
    public async Task Must_Return_Null_When_No_Token_Exists()
    {
        var token = await _provider.RetrieveTokenAsync();

        Assert.That(token, Is.Null);
    }

    [Test]
    public async Task Must_Return_The_Token_From_QueryString()
    {
        const string expectedToken = "test";

        _httpContextAccessor.HttpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                                                                             {
                                                                                 {QueryStringPaymentTokenOptions.DefaultQueryName, expectedToken}
                                                                             });

        var token = await _provider.RetrieveTokenAsync();

        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.EqualTo(expectedToken));
    }

    [Test]
    public async Task Must_Return_The_Token_From_QueryString_With_CustomQueryName()
    {
        const string expectedToken = "test";
        const string queryName = "pt";

        _options.Value.QueryName = queryName;

        _httpContextAccessor.HttpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                                                                             {
                                                                                 {queryName, expectedToken}
                                                                             });

        var token = await _provider.RetrieveTokenAsync();

        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.EqualTo(expectedToken));
    }
}