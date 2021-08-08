using Moq;
using NUnit.Framework;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using System;
using System.Threading.Tasks;

namespace Parbad.Gateway.FanAva.Tests
{
    public class FanAvaCommonTests
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
            _invoiceBuilder.UseFanAva();

            var invoice = await _invoiceBuilder.BuildAsync();

            Assert.IsNotNull(invoice);
            Assert.IsNotNull(invoice.GatewayName);
            Assert.IsTrue(invoice.GatewayName.Equals("FanAva", StringComparison.OrdinalIgnoreCase));
        }
    }
}
