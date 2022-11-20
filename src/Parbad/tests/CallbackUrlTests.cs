using Parbad.Exceptions;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parbad.Tests;

[TestClass]
public class CallbackUrlTests
{
    private const string ExpectedUrl = "http://www.mysite.com";

    [TestMethod]
    public void Url_Must_Be_Equal_With_ExpectedUrl()
    {
        var instance1 = new CallbackUrl(ExpectedUrl);
        var instance2 = CallbackUrl.Parse(ExpectedUrl);
        CallbackUrl.TryParse(ExpectedUrl, out var instance3);

        Assert.AreEqual(ExpectedUrl, instance1.Url);
        Assert.AreEqual(ExpectedUrl, instance2.Url);
        Assert.AreEqual(ExpectedUrl, instance3.Url);
    }

    [TestMethod]
    public void ToString_Value_Must_Be_Equal_With_Url()
    {
        var instance = new CallbackUrl(ExpectedUrl);

        Assert.AreEqual(ExpectedUrl, instance.ToString());
    }

    [TestMethod]
    public void QueryString_Works()
    {
        const string expectedQueryName = "test";
        const string expectedQueryValue = "test";

        var instance = new CallbackUrl(ExpectedUrl);

        var instance2 = instance.AddQueryString(expectedQueryName, expectedQueryValue);

        var uri = new Uri(instance2.Url);
        var query = uri.Query;

        Assert.IsNotNull(query);
        Assert.IsTrue(query.Contains($"{expectedQueryName}={expectedQueryValue}"));
    }

    [TestMethod]
    public void Casted_Value_Must_Be_Equal_With_Url()
    {
        var url = (string)new CallbackUrl(ExpectedUrl);

        Assert.AreEqual(ExpectedUrl, url);
    }

    [TestMethod]
    public void NonValidUrl_Must_Throw_Exception()
    {
        Assert.ThrowsException<CallbackUrlFormatException>(() => new CallbackUrl("abcd"));
    }
}
