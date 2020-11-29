using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Abstraction;
using System.Collections.Generic;
using System.Linq;

namespace Parbad.Tests.Gateway
{
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
            Assert.IsNotNull(result);

            AssertFailWhenResultIsNotSucceed(result);
            Assert.IsTrue(result.IsSucceed);

            Assert.AreEqual(expectedGatewayName, result.GatewayName);

            Assert.IsNotNull(result.GatewayTransporter);
            Assert.IsNotNull(result.GatewayTransporter.Descriptor);
            Assert.AreEqual(transportType, result.GatewayTransporter.Descriptor.Type);
            Assert.IsNotNull(result.GatewayTransporter.Descriptor.Url);

            if (transportType == GatewayTransporterDescriptor.TransportType.Post)
            {
                Assert.IsNotNull(result.GatewayTransporter.Descriptor.Form);

                if (expectedForm != null)
                {
                    foreach (var item in expectedForm)
                    {
                        var form = result
                            .GatewayTransporter
                            .Descriptor
                            .Form
                            .SingleOrDefault(_ => _.Key == item.Key);

                        Assert.IsNotNull(form.Key);
                        Assert.IsNotNull(form.Value);
                    }
                }
            }
            else
            {
                Assert.IsNull(result.GatewayTransporter.Descriptor.Form);
            }

            if (!string.IsNullOrEmpty(expectedPaymentPageUrl))
            {
                Assert.AreEqual(expectedPaymentPageUrl, result.GatewayTransporter.Descriptor.Url);
            }

            additionalChecks?.Invoke();
        }

        public static void OnFetchResult(
            IPaymentFetchResult result,
            long expectedTrackingNumber,
            decimal expectedAmount,
            string expectedGatewayName)
        {
            Assert.IsNotNull(result);

            AssertFailWhenResultIsNotSucceed(result);
            Assert.IsTrue(result.IsSucceed);
            Assert.AreEqual(PaymentFetchResultStatus.ReadyForVerifying, result.Status);

            Assert.AreEqual(expectedTrackingNumber, result.TrackingNumber);

            Assert.AreEqual(expectedGatewayName, result.GatewayName);

            Assert.AreEqual(GatewayAccount.DefaultName, result.GatewayAccountName);

            Assert.AreEqual(result.Amount, expectedAmount);

            Assert.IsFalse(result.IsAlreadyVerified);
        }

        public static void OnVerifyResult(
            IPaymentVerifyResult result,
            long expectedTrackingNumber,
            decimal expectedAmount,
            string expectedGatewayName,
            string expectedTransactionCode = null)
        {
            Assert.IsNotNull(result);

            AssertFailWhenResultIsNotSucceed(result);
            Assert.IsTrue(result.IsSucceed);
            Assert.AreEqual(PaymentVerifyResultStatus.Succeed, result.Status);

            Assert.AreEqual(expectedTrackingNumber, result.TrackingNumber);

            Assert.AreEqual(expectedGatewayName, result.GatewayName);

            Assert.AreEqual(GatewayAccount.DefaultName, result.GatewayAccountName);

            Assert.AreEqual(result.Amount, expectedAmount);

            Assert.IsNotNull(result.TransactionCode);

            if (!string.IsNullOrEmpty(expectedTransactionCode))
            {
                Assert.AreEqual(expectedTransactionCode, result.TransactionCode);
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
}
