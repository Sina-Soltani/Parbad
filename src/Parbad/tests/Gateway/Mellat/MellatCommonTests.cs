using Moq;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Parbad.Gateway.Mellat;
using Parbad.Gateway.Mellat.Internal;

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

        [Test]
        public async Task InvoiceProperties_Must_Contain_The_CumulativeAccounts()
        {
            _invoiceBuilder
                .SetAmount(10)
                .AddMellatCumulativeAccount(1, 4, 10)
                .AddMellatCumulativeAccount(2, 6)
                .UseMellat();

            var invoice = await _invoiceBuilder.BuildAsync();

            Assert.IsNotNull(invoice);
            Assert.IsNotNull(invoice.Properties);
            Assert.AreEqual(1, invoice.Properties.Count);
            Assert.IsTrue(invoice.Properties.ContainsKey(MellatHelper.CumulativeAccountsKey));
            var cumulativeAccounts = invoice.Properties[MellatHelper.CumulativeAccountsKey] as List<MellatCumulativeDynamicAccount>;
            Assert.IsNotNull(cumulativeAccounts);
            Assert.AreEqual(2, cumulativeAccounts.Count);

            Assert.AreEqual(1, cumulativeAccounts[0].SubServiceId);
            Assert.AreEqual(4, cumulativeAccounts[0].Amount.Value);
            Assert.AreEqual(10, cumulativeAccounts[0].PayerId);

            Assert.AreEqual(2, cumulativeAccounts[1].SubServiceId);
            Assert.AreEqual(6, cumulativeAccounts[1].Amount.Value);
            Assert.AreEqual(0, cumulativeAccounts[1].PayerId);
        }
    }
}
