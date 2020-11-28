using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Parbad.Gateway.Melli;
using Parbad.Gateway.Melli.Internal;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using System;

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
        public void Invoice_Must_Have_Correct_GatewayName()
        {
            _invoiceBuilder.UseMelli();

            var invoice = _invoiceBuilder.BuildAsync().GetAwaiter().GetResult();

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
