using Parbad.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Parbad.Tests.Helpers;

public static class GatewayOnResultHelper
{
    public static void OnRequestResult(
        IPaymentRequestResult result,
        string expectedGatewayName,
        GatewayTransporterDescriptor.TransportType transportType,
        Action additionalChecks = null,
        string expectedPaymentPageUrl = null,
        IDictionary<string, string> expectedForm = null)
    {
        Assert.That(result, Is.Not.Null);

        AssertFailWhenResultIsNotSucceed(result);
        Assert.That(result.IsSucceed, Is.True);

        Assert.That(result.GatewayName, Is.EqualTo(expectedGatewayName));

        Assert.That(result.GatewayTransporter, Is.Not.Null);
        Assert.That(result.GatewayTransporter.Descriptor, Is.Not.Null);
        Assert.That(result.GatewayTransporter.Descriptor.Type, Is.EqualTo(transportType));
        Assert.That(result.GatewayTransporter.Descriptor.Url, Is.Not.Null);

        if (transportType == GatewayTransporterDescriptor.TransportType.Post)
        {
            Assert.That(result.GatewayTransporter.Descriptor.Form, Is.Not.Null);

            if (expectedForm != null)
            {
                foreach (var item in expectedForm)
                {
                    var form = result
                              .GatewayTransporter
                              .Descriptor
                              .Form
                              .SingleOrDefault(_ => _.Key == item.Key);

                    Assert.That(form.Key, Is.Not.Null);
                    Assert.That(form.Value, Is.Not.Null);
                }
            }
        }
        else
        {
            Assert.That(result.GatewayTransporter.Descriptor.Form, Is.Null);
        }

        if (!string.IsNullOrEmpty(expectedPaymentPageUrl))
        {
            Assert.That(result.GatewayTransporter.Descriptor.Url, Is.EqualTo(expectedPaymentPageUrl));
        }

        additionalChecks?.Invoke();
    }

    public static void OnFetchResult(
        IPaymentFetchResult result,
        long expectedTrackingNumber,
        decimal expectedAmount,
        string expectedGatewayName)
    {
        Assert.That(result, Is.Not.Null);

        AssertFailWhenResultIsNotSucceed(result);
        Assert.That(result.IsSucceed, Is.True);
        Assert.That(result.Status, Is.EqualTo(PaymentFetchResultStatus.ReadyForVerifying));

        Assert.That(result.TrackingNumber, Is.EqualTo(expectedTrackingNumber));

        Assert.That(result.GatewayName, Is.EqualTo(expectedGatewayName));

        Assert.That(result.GatewayAccountName, Is.EqualTo(GatewayAccount.DefaultName));

        Assert.That(result.Amount.Value, Is.EqualTo(expectedAmount));

        Assert.That(result.IsAlreadyVerified, Is.False);
    }

    public static void OnVerifyResult(
        IPaymentVerifyResult result,
        long expectedTrackingNumber,
        decimal expectedAmount,
        string expectedGatewayName,
        string expectedTransactionCode = null)
    {
        Assert.That(result, Is.Not.Null);

        AssertFailWhenResultIsNotSucceed(result);
        Assert.That(result.IsSucceed, Is.True);
        Assert.That(result.Status, Is.EqualTo(PaymentVerifyResultStatus.Succeed));

        Assert.That(result.TrackingNumber, Is.EqualTo(expectedTrackingNumber));

        Assert.That(result.GatewayName, Is.EqualTo(expectedGatewayName));

        Assert.That(result.GatewayAccountName, Is.EqualTo(GatewayAccount.DefaultName));

        Assert.That(result.Amount.Value, Is.EqualTo(expectedAmount));

        Assert.That(result.TransactionCode, Is.Not.Null);

        if (!string.IsNullOrEmpty(expectedTransactionCode))
        {
            Assert.That(result.TransactionCode, Is.EqualTo(expectedTransactionCode));
        }
    }

    private static void AssertFailWhenResultIsNotSucceed(IPaymentResult result)
    {
        if (!result.IsSucceed)
        {
            Assert.Fail($"Result message: {result.Message}");
        }
    }
}