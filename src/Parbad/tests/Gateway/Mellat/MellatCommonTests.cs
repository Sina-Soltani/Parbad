using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using System;
using System.Threading.Tasks;

namespace Parbad.Tests.Gateway.Mellat
{
    [TestClass]
    public class MellatCommonTests
    {
        private IInvoiceBuilder _invoiceBuilder;

        [TestInitialize]
        public void Initialize()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();

            _invoiceBuilder = new DefaultInvoiceBuilder(mockServiceProvider.Object);
        }

        [TestMethod]
        public async Task Invoice_Must_Have_Correct_GatewayName()
        {
            _invoiceBuilder.UseMellat();

            var invoice = await _invoiceBuilder.BuildAsync();

            Assert.IsNotNull(invoice);
            Assert.IsNotNull(invoice.GatewayName);
            Assert.IsTrue(invoice.GatewayName.Equals("Mellat", StringComparison.OrdinalIgnoreCase));
        }
    }
}
