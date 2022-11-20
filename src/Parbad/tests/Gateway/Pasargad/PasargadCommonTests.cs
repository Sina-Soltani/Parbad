using Moq;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Gateway.Pasargad;

namespace Parbad.Tests.Gateway.Pasargad
{
    [TestClass]
    public class PasargadCommonTests
    {
        private IInvoiceBuilder _invoiceBuilder;

        [TestInitialize]
        public void Setup()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();

            _invoiceBuilder = new DefaultInvoiceBuilder(mockServiceProvider.Object);
        }

        [TestMethod]
        public async Task Invoice_Must_Have_Correct_GatewayName()
        {
            _invoiceBuilder.UsePasargad();

            var invoice = await _invoiceBuilder.BuildAsync();

            Assert.IsNotNull(invoice);
            Assert.IsNotNull(invoice.GatewayName);
            Assert.IsTrue(invoice.GatewayName.Equals("Pasargad", StringComparison.OrdinalIgnoreCase));
        }
    }
}
