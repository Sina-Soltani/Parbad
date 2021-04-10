using Moq;
using NUnit.Framework;
using Parbad.Gateway.Saman;
using Parbad.Gateway.Saman.Internal;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using System;
using System.Threading.Tasks;

namespace Parbad.Tests.Gateway.Saman
{
    public class SamanCommonTests
    {
        private IInvoiceBuilder _invoiceBuilder;

        [SetUp]
        public void Setup()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();

            _invoiceBuilder = new DefaultInvoiceBuilder(mockServiceProvider.Object);
        }

        [Test]
        public async Task Invoice_Must_Have_Correct_GatewayName()
        {
            _invoiceBuilder.UseSaman();

            var invoice = await _invoiceBuilder.BuildAsync();

            Assert.IsNotNull(invoice);
            Assert.IsNotNull(invoice.GatewayName);
            Assert.IsTrue(invoice.GatewayName.Equals("saman", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public async Task Invoice_Must_Have_Saman_MobileGateway_Enabled()
        {
            _invoiceBuilder.EnableSamanMobileGateway();

            var invoice = await _invoiceBuilder.BuildAsync();

            Assert.IsNotNull(invoice);

            Assert.IsNotNull(invoice.Properties);
            Assert.IsTrue(invoice.Properties.ContainsKey(SamanHelper.MobileGatewayKey));
            Assert.IsInstanceOf<bool>(invoice.Properties[SamanHelper.MobileGatewayKey]);
            Assert.AreEqual(true, invoice.Properties[SamanHelper.MobileGatewayKey]);
        }

        [Test]
        public async Task Invoice_Must_Have_Saman_MobileGateway_Disabled()
        {
            _invoiceBuilder.EnableSamanMobileGateway(false);

            var invoice = await _invoiceBuilder.BuildAsync();

            Assert.IsNotNull(invoice);

            Assert.IsNotNull(invoice.Properties);
            Assert.IsTrue(invoice.Properties.ContainsKey(SamanHelper.MobileGatewayKey));
            Assert.IsInstanceOf<bool>(invoice.Properties[SamanHelper.MobileGatewayKey]);
            Assert.AreEqual(false, invoice.Properties[SamanHelper.MobileGatewayKey]);
        }
    }
}
