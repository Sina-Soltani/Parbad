using Parbad.Exceptions;
using System;
using NUnit.Framework;

namespace Parbad.Tests;

[TestFixture]
public class CallbackUrlTests
{
    private const string ExpectedUrl = "http://www.mysite.com";

    [Test]
    public void Url_Must_Be_Equal_With_ExpectedUrl()
    {
        var instance1 = new CallbackUrl(ExpectedUrl);
        var instance2 = CallbackUrl.Parse(ExpectedUrl);
        CallbackUrl.TryParse(ExpectedUrl, out var instance3);

        Assert.That(instance1.Url, Is.EqualTo(ExpectedUrl));
        Assert.That(instance2.Url, Is.EqualTo(ExpectedUrl));
        Assert.That(instance3.Url, Is.EqualTo(ExpectedUrl));
    }

    [Test]
    public void ToString_Value_Must_Be_Equal_With_Url()
    {
        var instance = new CallbackUrl(ExpectedUrl);

        Assert.That(instance.ToString(), Is.EqualTo(ExpectedUrl));
    }

    [Test]
    public void QueryString_Works()
    {
        const string expectedQueryName = "test";
        const string expectedQueryValue = "test";

        var instance = new CallbackUrl(ExpectedUrl);

        var instance2 = instance.AddQueryString(expectedQueryName, expectedQueryValue);

        var uri = new Uri(instance2.Url);
        var query = uri.Query;

        Assert.That(query, Is.Not.Null);
        Assert.That(query.Contains($"{expectedQueryName}={expectedQueryValue}"), Is.True);
    }

    [Test]
    public void Casted_Value_Must_Be_Equal_With_Url()
    {
        var url = (string)new CallbackUrl(ExpectedUrl);

        Assert.That(url, Is.EqualTo(ExpectedUrl));
    }

    [Test]
    public void NonValidUrl_Must_Throw_Exception()
    {
        Assert.Throws<CallbackUrlFormatException>(() => new CallbackUrl("abcd"));
    }
}
