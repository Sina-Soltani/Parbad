using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using System;
using System.Threading.Tasks;

namespace Parbad.Tests
{
    public class InvoiceBuilderTests
    {
        private ServiceProvider _services;
        private IInvoiceBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _services = new ServiceCollection()
                .BuildServiceProvider();

            _builder = new DefaultInvoiceBuilder(_services);
        }

        [TearDown]
        public ValueTask Cleanup()
        {
            return _services.DisposeAsync();
        }

        [Test]
        public async Task Build_Must_Return_An_Invoice()
        {
            var invoice = await _builder.BuildAsync();

            Assert.IsNotNull(invoice);
        }

        [Test]
        public async Task Invoice_Must_Have_The_Expected_TrackingNumber()
        {
            const long expectedValue = 1000;

            _builder.SetTrackingNumber(expectedValue);

            var invoice = await _builder.BuildAsync();

            Assert.AreEqual(expectedValue, invoice.TrackingNumber);
        }

        [Test]
        public async Task Invoice_Must_Have_The_Expected_Amount()
        {
            const decimal expectedValue = 1000;

            _builder.SetAmount(expectedValue);

            var invoice = await _builder.BuildAsync();

            Assert.AreEqual(expectedValue, (decimal)invoice.Amount);
        }

        [Test]
        public async Task Invoice_Must_Have_The_Expected_CallbackUrl()
        {
            const string expectedValue = "http://test.com";

            _builder.SetCallbackUrl(expectedValue);

            var invoice = await _builder.BuildAsync();

            Assert.AreEqual(expectedValue, (string)invoice.CallbackUrl);
        }

        [Test]
        public async Task Invoice_Must_Have_The_Expected_GatewayName()
        {
            const string expectedValue = "Gateway";

            _builder.SetGateway(expectedValue);

            var invoice = await _builder.BuildAsync();

            Assert.AreEqual(expectedValue, invoice.GatewayName);
        }

        [Test]
        public async Task Invoice_Must_Have_The_Expected_AdditionalData()
        {
            const string expectedKey = "key";
            const string expectedValue = "value";

            _builder.AddProperty(expectedKey, expectedValue);

            var invoice = await _builder.BuildAsync();

            Assert.IsNotNull(invoice.Properties);
            Assert.IsTrue(invoice.Properties.ContainsKey(expectedKey));
            Assert.AreEqual(expectedValue, invoice.Properties[expectedKey]);
        }

        [Test]
        public void Invoice_Must_Throw_Exception_When_Duplicate_AdditionalKey_Is_Added()
        {
            const string key = "key";

            _builder.AddProperty(key, "");
            _builder.AddProperty(key, "");

            Assert.ThrowsAsync<ArgumentException>(() => _builder.BuildAsync());
        }

        [Test]
        public async Task Invoice_Must_Have_The_Updated_AdditionalData_Value()
        {
            const string expectedKey = "key";
            const string expectedValue = "value";

            _builder.AddProperty(expectedKey, "");
            _builder.AddOrUpdateProperty(expectedKey, expectedValue);

            var invoice = await _builder.BuildAsync();

            Assert.IsNotNull(invoice.Properties);
            Assert.IsTrue(invoice.Properties.ContainsKey(expectedKey));
            Assert.AreEqual(expectedValue, invoice.Properties[expectedKey]);
        }

        [Test]
        public async Task Invoice_Formatter_Must_Apply_Value_Correctly()
        {
            const long expectedTrackingNumber = 1000;
            const decimal expectedAmount = 1000;
            const string expectedCallbackUrl = "http://test.com";
            const string expectedGateway = "Gateway";

            _builder.AddFormatter(_ =>
            {
                _.TrackingNumber = expectedTrackingNumber;
                _.Amount = expectedAmount;
                _.CallbackUrl = new CallbackUrl(expectedCallbackUrl);
                _.GatewayName = expectedGateway;
            });

            var invoice = await _builder.BuildAsync();

            Assert.AreEqual(expectedTrackingNumber, invoice.TrackingNumber);
            Assert.AreEqual(expectedAmount, (decimal)invoice.Amount);
            Assert.AreEqual(expectedCallbackUrl, (string)invoice.CallbackUrl);
            Assert.AreEqual(expectedGateway, invoice.GatewayName);
        }
    }
}
