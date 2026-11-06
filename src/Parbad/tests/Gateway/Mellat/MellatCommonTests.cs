using Moq;
using Parbad.Gateway.Mellat;
using Parbad.Gateway.Mellat.Internal;
using Parbad.Internal;
using Parbad.InvoiceBuilder;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Parbad.Tests.Gateway.Mellat;

[TestFixture]
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

        Assert.That(invoice, Is.Not.Null);
        Assert.That(invoice.GatewayName, Is.Not.Null);
        Assert.That(invoice.GatewayName.Equals("Mellat", StringComparison.OrdinalIgnoreCase), Is.True);
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

        Assert.That(invoice, Is.Not.Null);
        Assert.That(invoice.Properties, Is.Not.Null);
        Assert.That(invoice.Properties.Count, Is.EqualTo(1));
        Assert.That(invoice.Properties.ContainsKey(MellatHelper.CumulativeAccountsKey), Is.True);
        var cumulativeAccounts = invoice.Properties[MellatHelper.CumulativeAccountsKey] as List<MellatCumulativeDynamicAccount>;
        Assert.That(cumulativeAccounts, Is.Not.Null);
        Assert.That(cumulativeAccounts.Count, Is.EqualTo(2));

        Assert.That(cumulativeAccounts[0].SubServiceId, Is.EqualTo(1));
        Assert.That(cumulativeAccounts[0].Amount.Value, Is.EqualTo(4));
        Assert.That(cumulativeAccounts[0].PayerId, Is.EqualTo(10));

        Assert.That(cumulativeAccounts[1].SubServiceId, Is.EqualTo(2));
        Assert.That(cumulativeAccounts[1].Amount.Value, Is.EqualTo(6));
        Assert.That(cumulativeAccounts[1].PayerId, Is.EqualTo(0));
    }

    [Test]
    public async Task InvoiceProperties_Must_Contain_AdditionalData()
    {
        var expectedAdditionalData = new MellatGatewayAdditionalDataRequest
                                     {
                                         MobileNumber = "1",
                                         PayerId = "2",
                                         AdditionalData = "3"
                                     };

        _invoiceBuilder
           .SetMellatAdditionalData(expectedAdditionalData)
           .UseMellat();

        var invoice = await _invoiceBuilder.BuildAsync();

        Assert.That(invoice, Is.Not.Null);

        Assert.That(invoice.Properties, Is.Not.Null);
        Assert.That(invoice.Properties.Count, Is.EqualTo(1));
        Assert.That(invoice.Properties.ContainsKey(MellatHelper.AdditionalDataKey), Is.True);

        var additionalData = invoice.Properties[MellatHelper.AdditionalDataKey] as MellatGatewayAdditionalDataRequest;
        Assert.That(additionalData, Is.Not.Null);
        Assert.That(additionalData.MobileNumber, Is.EqualTo(expectedAdditionalData.MobileNumber));
        Assert.That(additionalData.AdditionalData, Is.EqualTo(expectedAdditionalData.AdditionalData));
        Assert.That(additionalData.PayerId, Is.EqualTo(expectedAdditionalData.PayerId));
    }
}