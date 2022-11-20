using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Parbad.Internal;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.IranKish.Tests
{
    [TestClass]
    public class IranKishCommonTests
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
            _invoiceBuilder.UseIranKish();

            var invoice = await _invoiceBuilder.BuildAsync();

            Assert.IsNotNull(invoice);
            Assert.IsNotNull(invoice.GatewayName);
            Assert.IsTrue(invoice.GatewayName.Equals("IranKish", StringComparison.OrdinalIgnoreCase));
        }
    }
}
