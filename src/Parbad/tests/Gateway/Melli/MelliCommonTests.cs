using Moq;
using Parbad.Gateway.Melli;
using Parbad.Gateway.Melli.Internal;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parbad.Tests.Gateway.Melli
{
    [TestClass]
    public class MelliCommonTests
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
            _invoiceBuilder.UseMelli();

            var invoice = await _invoiceBuilder.BuildAsync();

            Assert.IsNotNull(invoice);
            Assert.IsNotNull(invoice.GatewayName);
            Assert.IsTrue(invoice.GatewayName.Equals("melli", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void Signing_Invalid_RequestData_Must_Throw_Exception()
        {
            var crypto = new MelliGatewayCrypto();

            const string data = "test";

            Assert.ThrowsException<MelliGatewayDataSigningException>(() =>
            {
                crypto.Encrypt("test", data);
            });
        }
    }
}
