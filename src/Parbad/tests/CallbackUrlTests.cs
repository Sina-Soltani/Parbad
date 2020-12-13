using Microsoft.AspNetCore.WebUtilities;
using NUnit.Framework;
using Parbad.Exceptions;
using System;

namespace Parbad.Tests
{
    public class CallbackUrlTests
    {
        private const string ExpectedUrl = "http://www.mysite.com";

        [Test]
        public void Url_Must_Be_Equal_With_ExpectedUrl()
        {
            var instance1 = new CallbackUrl(ExpectedUrl);
            var instance2 = CallbackUrl.Parse(ExpectedUrl);
            CallbackUrl.TryParse(ExpectedUrl, out var instance3);

            Assert.AreEqual(ExpectedUrl, instance1.Url);
            Assert.AreEqual(ExpectedUrl, instance2.Url);
            Assert.AreEqual(ExpectedUrl, instance3.Url);
        }

        [Test]
        public void ToString_Value_Must_Be_Equal_With_Url()
        {
            var instance = new CallbackUrl(ExpectedUrl);

            Assert.AreEqual(ExpectedUrl, instance.ToString());
        }

        [Test]
        public void QueryString_Works()
        {
            const string expectedQueryName = "test";
            const string expectedQueryValue = "test";

            var instance = new CallbackUrl(ExpectedUrl);

            var instance2 = instance.AddQueryString("test", "test");

            var uri = new Uri(instance2.Url);
            var query = QueryHelpers.ParseQuery(uri.Query);

            Assert.IsNotNull(query);
            Assert.IsTrue(query.ContainsKey(expectedQueryName));
            Assert.AreEqual(expectedQueryValue, (string)query[expectedQueryName]);
        }

        [Test]
        public void Casted_Value_Must_Be_Equal_With_Url()
        {
            var url = (string)new CallbackUrl(ExpectedUrl);

            Assert.AreEqual(ExpectedUrl, url);
        }

        [Test]
        public void NonValidUrl_Must_Throw_Exception()
        {
            Assert.Throws<CallbackUrlFormatException>(() => new CallbackUrl("abcd"));
        }
    }
}
