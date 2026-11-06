using Moq;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Parbad.Gateway.Pasargad;

namespace Parbad.Tests.Gateway.Pasargad;

[TestFixture]
public class PasargadCommonTests
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
        _invoiceBuilder.UsePasargad();

        var invoice = await _invoiceBuilder.BuildAsync();

        Assert.That(invoice, Is.Not.Null);
        Assert.That(invoice.GatewayName, Is.Not.Null);
        Assert.That(invoice.GatewayName.Equals("Pasargad", StringComparison.OrdinalIgnoreCase), Is.True);
    }
}