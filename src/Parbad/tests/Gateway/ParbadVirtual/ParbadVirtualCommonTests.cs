using Moq;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Parbad.Gateway.ParbadVirtual;

namespace Parbad.Tests.Gateway.ParbadVirtual;

[TestFixture]
public class ParbadVirtualCommonTests
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
        _invoiceBuilder.UseParbadVirtual();

        var invoice = await _invoiceBuilder.BuildAsync();

        Assert.That(invoice, Is.Not.Null);
        Assert.That(invoice.GatewayName, Is.Not.Null);
        Assert.That(invoice.GatewayName.Equals("ParbadVirtual", StringComparison.OrdinalIgnoreCase), Is.True);
    }
}