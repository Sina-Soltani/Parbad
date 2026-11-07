using Moq;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Parbad.Gateway.Parsian;

namespace Parbad.Tests.Gateway.Parsian;

[TestFixture]
public class ParsianCommonTests
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
        _invoiceBuilder.UseParsian();

        var invoice = await _invoiceBuilder.BuildAsync();

        Assert.That(invoice, Is.Not.Null);
        Assert.That(invoice.GatewayName, Is.Not.Null);
        Assert.That(invoice.GatewayName.Equals("Parsian", StringComparison.OrdinalIgnoreCase), Is.True);
    }
}