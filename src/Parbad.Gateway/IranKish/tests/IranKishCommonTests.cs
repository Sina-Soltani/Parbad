using NUnit.Framework;
using Moq;
using Parbad.Internal;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.IranKish.Tests;

[TestFixture]
public class IranKishCommonTests
{
    private IInvoiceBuilder _invoiceBuilder = null!;

    [SetUp]
    public void Initialize()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();

        _invoiceBuilder = new DefaultInvoiceBuilder(mockServiceProvider.Object);
    }

    [Test]
    public async Task Invoice_Must_Have_Correct_GatewayName()
    {
        _invoiceBuilder.UseIranKish();

        var invoice = await _invoiceBuilder.BuildAsync();

        Assert.That(invoice, Is.Not.Null);
        Assert.That(invoice.GatewayName, Is.Not.Null);
        Assert.That(invoice.GatewayName.Equals("IranKish", StringComparison.OrdinalIgnoreCase), Is.True);
    }
}