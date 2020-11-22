using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Parbad.Gateway.Melli;
using Parbad.Gateway.Melli.Internal;
using Parbad.Internal;
using Parbad.InvoiceBuilder;

namespace Parbad.Tests.Gateway.Melli
{
    [TestClass]
    public class MelliCommonTests
    {
        [TestMethod]
        public void Invoice_Is_Valid()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();

            IInvoiceBuilder invoiceBuilder = new DefaultInvoiceBuilder(mockServiceProvider.Object);

            invoiceBuilder.UseMelli();

            var invoice = invoiceBuilder.BuildAsync().GetAwaiter().GetResult();

            Assert.IsNotNull(invoice);
            Assert.IsNotNull(invoice.GatewayName);
            Assert.IsTrue(invoice.GatewayName.Equals("melli", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void Signing_Invalid_RequestData_Must_Throw_Exception()
        {
            Assert.ThrowsException<MelliGatewayDataSigningException>(() =>
            {
                MelliHelper.SignRequestData("test", "abcd", 1, 1);
            });
        }

        [TestMethod]
        public void Signing_Invalid_VerificationData_Must_Throw_Exception()
        {
            Assert.ThrowsException<MelliGatewayDataSigningException>(() =>
            {
                MelliHelper.SignVerifyData("abcd", "test");
            });
        }
    }
}
