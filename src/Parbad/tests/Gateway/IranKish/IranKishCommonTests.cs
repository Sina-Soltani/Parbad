using Moq;
using NUnit.Framework;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using System;
using System.Threading.Tasks;
using Parbad.Gateway.IranKish;

namespace Parbad.Tests.Gateway.IranKish
{
    public class IranKishCommonTests
    {
        private IInvoiceBuilder _invoiceBuilder;

        [SetUp]
        public void Initialize()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();

            _invoiceBuilder = new DefaultInvoiceBuilder(mockServiceProvider.Object);
        }

        [Test]
        public async Task Invoice_Must_Have_Correct_GatewayName()
        {
            _invoiceBuilder.UseIranKish();

            var invoice = await _invoiceBuilder.BuildAsync();

            Assert.IsNotNull(invoice);
            Assert.IsNotNull(invoice.GatewayName);
            Assert.IsTrue(invoice.GatewayName.Equals("IranKish", StringComparison.OrdinalIgnoreCase));
        }
    }
}
