using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Parbad.Gateway.Saman.Internal;
using Parbad.Internal;
using Parbad.InvoiceBuilder;

namespace Parbad.Tests.Gateway.Saman
{
    [TestClass]
    public class SamanCommonTests
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
            _invoiceBuilder.UseSaman();

            var invoice = await _invoiceBuilder.BuildAsync();

            Assert.IsNotNull(invoice);
            Assert.IsNotNull(invoice.GatewayName);
            Assert.IsTrue(invoice.GatewayName.Equals("saman", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public async Task Invoice_Must_Have_Saman_MobileGateway_Enabled()
        {
            _invoiceBuilder.EnableSamanMobileGateway();

            var invoice = await _invoiceBuilder.BuildAsync();

            Assert.IsNotNull(invoice);

            Assert.IsNotNull(invoice.AdditionalData);
            Assert.IsTrue(invoice.AdditionalData.ContainsKey(SamanHelper.MobileGatewayKey));
            Assert.IsInstanceOfType(invoice.AdditionalData[SamanHelper.MobileGatewayKey], typeof(bool));
            Assert.AreEqual(true, invoice.AdditionalData[SamanHelper.MobileGatewayKey]);
        }

        [TestMethod]
        public async Task Invoice_Must_Have_Saman_MobileGateway_Disabled()
        {
            _invoiceBuilder.EnableSamanMobileGateway(false);

            var invoice = await _invoiceBuilder.BuildAsync();

            Assert.IsNotNull(invoice);

            Assert.IsNotNull(invoice.AdditionalData);
            Assert.IsTrue(invoice.AdditionalData.ContainsKey(SamanHelper.MobileGatewayKey));
            Assert.IsInstanceOfType(invoice.AdditionalData[SamanHelper.MobileGatewayKey], typeof(bool));
            Assert.AreEqual(false, invoice.AdditionalData[SamanHelper.MobileGatewayKey]);
        }
    }
}
