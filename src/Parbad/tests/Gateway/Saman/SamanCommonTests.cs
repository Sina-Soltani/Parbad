using Moq;
using Parbad.Gateway.Saman;
using Parbad.Gateway.Saman.Internal;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Parbad.Tests.Gateway.Saman;

[TestFixture]
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

        Assert.That(invoice, Is.Not.Null);
        Assert.That(invoice.GatewayName, Is.Not.Null);
        Assert.That(invoice.GatewayName.Equals("saman", StringComparison.OrdinalIgnoreCase), Is.True);
    }
}
