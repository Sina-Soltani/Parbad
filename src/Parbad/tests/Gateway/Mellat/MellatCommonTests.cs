using Moq;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Parbad.Gateway.Mellat;

namespace Parbad.Tests.Gateway.Mellat
{
    public class MellatCommonTests
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
            _invoiceBuilder.UseMellat();

            var invoice = await _invoiceBuilder.BuildAsync();

            Assert.IsNotNull(invoice);
            Assert.IsNotNull(invoice.GatewayName);
            Assert.IsTrue(invoice.GatewayName.Equals("Mellat", StringComparison.OrdinalIgnoreCase));
        }
    }
}
